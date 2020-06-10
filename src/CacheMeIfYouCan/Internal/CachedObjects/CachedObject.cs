using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using CacheMeIfYouCan.Events.CachedObject;

namespace CacheMeIfYouCan.Internal.CachedObjects
{
    internal class CachedObject<T> : ICachedObject<T>
    {
        private const int PendingInitialization = (int)CachedObjectState.PendingInitialization;
        private const int InitializationInProgress = (int)CachedObjectState.InitializationInProgress;
        private const int Ready = (int)CachedObjectState.Ready;
        private const int Disposed = (int)CachedObjectState.Disposed;

        private readonly Func<CancellationToken, Task<T>> _getValueFunc;
        private readonly Func<TimeSpan> _refreshIntervalFactory;
        private readonly TimeSpan? _refreshValueFuncTimeout;
        private readonly List<IDisposable> _toDispose = new List<IDisposable>();
        private readonly object _lock = new object();
        protected readonly CancellationTokenSource _isDisposedCancellationTokenSource = new CancellationTokenSource();
        
        protected T _value;
        protected long _version;
        private volatile int _state;
        private DateTime _datePreviousSuccessfulRefreshStarted;
        private DateTime _datePreviousSuccessfulRefreshFinished;
        private TaskCompletionSource<bool> _initializationTaskCompletionSource;
        private Timer _refreshTimer;
        private RefreshHandler _currentRefreshHandler;
        private RefreshHandler _queuedRefreshHandler;
        
        // This is to ensure we never run updates and refreshes at the same time. Refreshes are the priority so no
        // updates will start while there are pending refreshes.
        private readonly HighLowPrioritySemaphore _refreshOrUpdateMutex = new HighLowPrioritySemaphore(1, 1);

        public CachedObject(
            Func<CancellationToken, Task<T>> getValueFunc,
            Func<TimeSpan> refreshIntervalFactory,
            TimeSpan? refreshValueFuncTimeout)
        {
            _getValueFunc = getValueFunc;
            _refreshIntervalFactory = refreshIntervalFactory;
            _refreshValueFuncTimeout = refreshValueFuncTimeout;
        }

        public event EventHandler OnInitialized;
        public event EventHandler OnDisposed;
        public event EventHandler<ValueRefreshedEvent<T>> OnValueRefreshed;
        public event EventHandler<ValueRefreshExceptionEvent<T>> OnValueRefreshException;

        public T Value
        {
            get
            {
                var value = _value;
                if (_state == Ready)
                    return value;
                
                ThrowIfDisposed();
                
                Initialize();

                Thread.MemoryBarrier();
                return _value;
            }
        }

        public CachedObjectState State => (CachedObjectState)_state;
        public long Version => _version;
        
        public void Initialize(CancellationToken cancellationToken = default)
        {
            Task.Run(() => InitializeAsync(cancellationToken), cancellationToken).GetAwaiter().GetResult();
        }

        public async Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            TaskCompletionSource<bool> tcs;
            var initializationAlreadyInProgress = false;
            lock (_lock)
            {
                switch (_state)
                {
                    case Disposed:
                        throw GetObjectDisposedException();

                    case Ready:
                        return;

                    case InitializationInProgress:
                        tcs = _initializationTaskCompletionSource;
                        initializationAlreadyInProgress = true;
                        break;

                    case PendingInitialization:
                        tcs = _initializationTaskCompletionSource = new TaskCompletionSource<bool>();
                        _state = InitializationInProgress;
                        break;
                    
                    default:
                        throw new InvalidOperationException("Invalid state - " + _state);
                }
            }

            if (initializationAlreadyInProgress)
            {
                await tcs.Task.ConfigureAwait(false);
                return;
            }

            var start = DateTime.UtcNow;
            var stopwatch = StopwatchStruct.StartNew();
            
            CancellationTokenSource cts = null;
            try
            {
                if (_refreshValueFuncTimeout.HasValue)
                {
                    cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

                    cts.CancelAfter(_refreshValueFuncTimeout.Value);

                    cancellationToken = cts.Token;
                }

                _value = await _getValueFunc(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                lock (_lock)
                {
                    ThrowIfDisposed();
                    _state = PendingInitialization;
                    _initializationTaskCompletionSource = null;
                }

                PublishValueRefreshExceptionEvent(ex, stopwatch.Elapsed);
                tcs.TrySetException(ex);

                throw;
            }
            finally
            {
                cts?.Dispose();
            }

            lock (_lock)
            {
                ThrowIfDisposed();
                _version = 1;
                _state = Ready;
                _initializationTaskCompletionSource = null;
            }

            OnInitialized?.Invoke(this, null);
            tcs.TrySetResult(true);

            PublishValueRefreshedEvent(default, stopwatch.Elapsed);

            _datePreviousSuccessfulRefreshStarted = start;
            _datePreviousSuccessfulRefreshFinished = DateTime.UtcNow;

            if (_refreshIntervalFactory is null)
                return;

            var refreshInterval = _refreshIntervalFactory();

            _refreshTimer = new Timer(
                _ => RefreshValueFromTimer(),
                null,
                (long)refreshInterval.TotalMilliseconds,
                -1);

            RegisterDisposable(_refreshTimer);
        }

        public virtual void RefreshValue(
            TimeSpan skipIfPreviousRefreshStartedWithinTimeFrame = default,
            CancellationToken cancellationToken = default)
        {
            Task.Run(() => RefreshValueAsync(
                skipIfPreviousRefreshStartedWithinTimeFrame,
                cancellationToken), cancellationToken).GetAwaiter().GetResult();
        }

        public async Task RefreshValueAsync(
            TimeSpan skipIfPreviousRefreshStartedWithinTimeFrame = default,
            CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            cancellationToken.ThrowIfCancellationRequested();

            DateTime? skipIfPreviousRefreshStartedSince = null;
            if (skipIfPreviousRefreshStartedWithinTimeFrame > TimeSpan.Zero)
            {
                skipIfPreviousRefreshStartedSince = DateTime.UtcNow - skipIfPreviousRefreshStartedWithinTimeFrame;
                
                if (_datePreviousSuccessfulRefreshStarted > skipIfPreviousRefreshStartedSince)
                    return;
            }
            
            var tcs = new TaskCompletionSource<bool>();

            var refreshHandler = GetRefreshHandler();

            CancellationTokenRegistration registration = default;
            if (cancellationToken.CanBeCanceled)
            {
                registration = cancellationToken.Register(() =>
                {
                    refreshHandler.MarkCancellation();
                    tcs.TrySetCanceled();
                });
            }

            try
            {
                await tcs.Task.ConfigureAwait(false);
            }
            finally
            {
                registration.Dispose();
            }

            RefreshHandler GetRefreshHandler()
            {
                lock (_lock)
                {
                    if (_currentRefreshHandler is null)
                    {
                        var handler = _currentRefreshHandler = new RefreshHandler(this);
                        handler.AddWaiter_WithinParentLock(tcs);
                        handler.Start_WithinParentLock(true);
                        return handler;
                    }

                    if (_currentRefreshHandler.AddWaiter_WithinParentLock(tcs))
                        return _currentRefreshHandler;

                    if (_queuedRefreshHandler is null)
                        _queuedRefreshHandler = new RefreshHandler(this);
                    
                    _queuedRefreshHandler.AddWaiter_WithinParentLock(tcs, skipIfPreviousRefreshStartedSince);
                    return _queuedRefreshHandler;
                }
            }
        }

        public ICachedObject<TOut> Map<TOut>(Func<T, TOut> map)
        {
            return MapAsync(v => Task.FromResult(map(v)));
        }

        public ICachedObject<TOut> MapAsync<TOut>(Func<T, Task<TOut>> map)
        {
            return new MappedCachedObject<T, Unit, TOut>(this, map);
        }
        
        public virtual void Dispose()
        {
            if (IsDisposed())
                return;
            
            T finalValue;
            lock (_lock)
            {
                if (IsDisposed())
                    return;

                _state = Disposed;
                _isDisposedCancellationTokenSource.Cancel();
                _isDisposedCancellationTokenSource.Dispose();

                var objectDisposedException = GetObjectDisposedException();
                
                var disposedTcs = new TaskCompletionSource<bool>();
                disposedTcs.SetException(objectDisposedException);

                var initializationTcs = Interlocked.Exchange(ref _initializationTaskCompletionSource, disposedTcs);

                initializationTcs?.TrySetException(objectDisposedException);

                finalValue = _value;
                _value = default;
            }
            
            foreach (var item in _toDispose)
                item.Dispose();

            if (finalValue is IDisposable disposable)
                disposable.Dispose();
            
            OnDisposed?.Invoke(this, null);
        }
        
        protected bool TryAcquireRefreshOrUpdateValueLockWithoutWaiting()
        {
            return _refreshOrUpdateMutex.TryAcquireWithoutWaiting();
        }
        
        protected Task AcquireUpdateValueLock(CancellationToken cancellationToken)
        {
            return _refreshOrUpdateMutex.WaitAsync(false, cancellationToken);
        }
        
        protected void ReleaseRefreshOrUpdateValueLock()
        {
            _refreshOrUpdateMutex.Release();
        }
        
        private Task AcquireRefreshValueLock(CancellationToken cancellationToken)
        {
            return _refreshOrUpdateMutex.WaitAsync(true, cancellationToken);
        }
        
        // Only called from the Timer
        private async Task RefreshValueFromTimer()
        {
            try
            {
                await RefreshValueAsync().ConfigureAwait(false);
            }
            catch
            {
                UpdateNextRefreshDate();
            }
        }

        private void UpdateNextRefreshDate()
        {
            if (_refreshIntervalFactory is null)
                return;
            
            var nextInterval = _refreshIntervalFactory();
                    
            if (!IsDisposed())
                _refreshTimer.Change((long)nextInterval.TotalMilliseconds, -1);
        }
        
        protected void PublishValueRefreshedEvent(T previousValue, TimeSpan duration)
        {
            var onValueRefreshedEvent = OnValueRefreshed;
            if (onValueRefreshedEvent is null)
                return;
            
            var message = new ValueRefreshedEvent<T>(
                _value,
                previousValue,
                duration,
                _datePreviousSuccessfulRefreshFinished,
                _version);

            onValueRefreshedEvent(this, message);
        }
        
        protected void PublishValueRefreshExceptionEvent(Exception exception, TimeSpan duration)
        {
            var onValueRefreshExceptionEvent = OnValueRefreshException;
            if (onValueRefreshExceptionEvent is null)
                return;
            
            var message = new ValueRefreshExceptionEvent<T>(
                exception,
                _value,
                duration,
                _datePreviousSuccessfulRefreshFinished,
                _version);

            onValueRefreshExceptionEvent(this, message);
        }
        
        protected void RegisterDisposable(IDisposable disposable) => _toDispose.Add(disposable);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected bool IsDisposed() => _state == Disposed;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void ThrowIfDisposed()
        {
            if (IsDisposed())
                throw GetObjectDisposedException();
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected ObjectDisposedException GetObjectDisposedException()
        {
            return new ObjectDisposedException(this.GetType().ToString());
        }
        
        private sealed class RefreshHandler : IDisposable
        {
            private readonly CachedObject<T> _parent;
            private readonly List<TaskCompletionSource<bool>> _tcsList;
            private readonly List<(DateTime SkipIfStartedSince, TaskCompletionSource<bool> Tcs)> _skippableTcsList; 
            private readonly CancellationTokenSource _cts;
            private readonly IDisposable _cancellationRegistration;
            private State _state;
            private int _waiterCount;

            public RefreshHandler(CachedObject<T> parent)
            {
                _parent = parent;
                _tcsList = new List<TaskCompletionSource<bool>>();
                _skippableTcsList = new List<(DateTime, TaskCompletionSource<bool>)>();
                _cts = new CancellationTokenSource();
                _cancellationRegistration = _parent._isDisposedCancellationTokenSource.Token.Register(() => _cts.Cancel());
            }

            public bool AddWaiter_WithinParentLock(
                TaskCompletionSource<bool> tcs,
                DateTime? skipIfPreviousRefreshStartedSince = null)
            {
                if (_state != State.Queued)
                    return false;

                _tcsList.Add(tcs);
                _waiterCount++;
                
                if (skipIfPreviousRefreshStartedSince.HasValue)
                    _skippableTcsList.Add((skipIfPreviousRefreshStartedSince.Value, tcs));
                
                return true;
            }
            
            public void MarkCancellation()
            {
                var waiterCount = Interlocked.Decrement(ref _waiterCount);

                if (waiterCount > 0)
                    return;
                
                lock (_parent._lock)
                {
                    if (_state != State.Running)
                        return;

                    _cts.Cancel();
                }
            }

            public void Start_WithinParentLock(bool acquireRefreshAndUpdateMutex)
            {
                if (_state == State.Disposed)
                    throw new ObjectDisposedException(nameof(RefreshHandler));
                
                _parent.ThrowIfDisposed();
                
                if (_parent._refreshValueFuncTimeout.HasValue)
                    _cts.CancelAfter(_parent._refreshValueFuncTimeout.Value);

                _state = State.Running;

                foreach (var (skipIfStartedSince, tcs) in _skippableTcsList)
                {
                    if (_parent._datePreviousSuccessfulRefreshStarted < skipIfStartedSince)
                        continue;
                    
                    tcs.TrySetResult(true);

                    if (Interlocked.Decrement(ref _waiterCount) == 0)
                    {
                        this.Dispose_WithinParentLock();
                        return;
                    }
                }

                Task.Run(() => RunAsync(acquireRefreshAndUpdateMutex));
            }

            public void Dispose()
            {
                lock (_parent._lock)
                    Dispose_WithinParentLock();
            }

            private void Dispose_WithinParentLock()
            {
                _state = State.Disposed;
                _cancellationRegistration.Dispose();
                _cts.Dispose();
                
                _parent._currentRefreshHandler = _parent._queuedRefreshHandler;
                _parent._queuedRefreshHandler = null;
                
                if (_parent._currentRefreshHandler is null)
                    _parent.ReleaseRefreshOrUpdateValueLock(); // Allow updates to run
                else
                    _parent._currentRefreshHandler.Start_WithinParentLock(false);
            }

            private async Task RunAsync(bool acquireRefreshAndUpdateMutex)
            {
                if (acquireRefreshAndUpdateMutex)
                {
                    if (!_parent.TryAcquireRefreshOrUpdateValueLockWithoutWaiting())
                    {
                        try
                        {
                            await _parent.AcquireRefreshValueLock(_cts.Token).ConfigureAwait(false);
                        }
                        catch (OperationCanceledException)
                        {
                            foreach (var tcs in _tcsList)
                                tcs.TrySetCanceled();

                            Dispose();
                            return;
                        }
                    }
                }

                var start = DateTime.UtcNow;
                var stopwatch = StopwatchStruct.StartNew();
                try
                {
                    _cts.Token.ThrowIfCancellationRequested();
                    
                    var newValue = await _parent._getValueFunc(_cts.Token).ConfigureAwait(false);
                    var oldValue = _parent._value;
                    _parent._value = newValue;
                
                    Interlocked.Increment(ref _parent._version);
                
                    _parent.PublishValueRefreshedEvent(oldValue, stopwatch.Elapsed);
                
                    foreach (var tcs in _tcsList)
                        tcs.TrySetResult(true);

                    _parent._datePreviousSuccessfulRefreshStarted = start;
                    _parent._datePreviousSuccessfulRefreshFinished = DateTime.UtcNow;
                    
                    _parent.UpdateNextRefreshDate();
                }
                catch (Exception ex)
                {
                    var wasCancelled = _cts.IsCancellationRequested;
                    
                    _parent.PublishValueRefreshExceptionEvent(ex, stopwatch.Elapsed);
                    
                    if (wasCancelled)
                    {
                        foreach (var tcs in _tcsList)
                            tcs.TrySetCanceled();
                    }
                    else
                    {
                        foreach (var tcs in _tcsList)
                            tcs.TrySetException(ex);
                    }
                }
                finally
                {
                    Dispose();
                }
            }

            private enum State
            {
                Queued,
                Running,
                Disposed
            }
        }
    }
}