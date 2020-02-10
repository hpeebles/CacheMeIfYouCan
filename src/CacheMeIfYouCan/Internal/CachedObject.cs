using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace CacheMeIfYouCan.Internal
{
    internal sealed class CachedObject<T> : ICachedObject<T>
    {
        private readonly Func<CancellationToken, Task<T>> _getValueFunc;
        private readonly Func<TimeSpan> _refreshIntervalFactory;
        private readonly TimeSpan? _refreshValueFuncTimeout;
        private readonly CancellationTokenSource _isDisposedCancellationTokenSource;
        private readonly object _lock = new object();
        private TaskCompletionSource<bool> _initializationTaskCompletionSource;
        private CachedObjectRefreshHandler _currentRefreshHandler;
        private CachedObjectRefreshHandler _queuedRefreshHandler;
        private T _value;
        private volatile int _state;
        private DateTime _dateOfPreviousSuccessfulRefresh;
        private Timer _refreshTimer;
        private long _version;
        private const int PendingInitialization = (int)CachedObjectState.PendingInitialization;
        private const int InitializationInProgress = (int)CachedObjectState.InitializationInProgress;
        private const int Ready = (int)CachedObjectState.Ready;
        private const int Disposed = (int)CachedObjectState.Disposed;

        public CachedObject(
            Func<CancellationToken, Task<T>> getValueFunc,
            Func<TimeSpan> refreshIntervalFactory,
            TimeSpan? refreshValueFuncTimeout)
        {
            _getValueFunc = getValueFunc;
            _refreshIntervalFactory = refreshIntervalFactory;
            _refreshValueFuncTimeout = refreshValueFuncTimeout;
            _isDisposedCancellationTokenSource = new CancellationTokenSource();
        }

        public T Value
        {
            get
            {
                var value = _value;
                if (_state == Ready)
                    return value;
                
                ThrowIfDisposed();
                
                Task.Run(() => InitializeAsync()).GetAwaiter().GetResult();

                Thread.MemoryBarrier();
                return _value;
            }
        }

        public CachedObjectState State => (CachedObjectState)_state;
        public long Version => _version;
        public event EventHandler<CachedObjectValueRefreshedEvent<T>> OnValueRefreshed;
        public event EventHandler<CachedObjectValueRefreshExceptionEvent<T>> OnValueRefreshException;

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

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _isDisposedCancellationTokenSource.Token);
            
            if (_refreshValueFuncTimeout.HasValue)
                cts.CancelAfter(_refreshValueFuncTimeout.Value);
            
            cancellationToken = cts.Token;
            
            TimeSpan refreshInterval;
            var stopwatch = Stopwatch.StartNew();
            try
            {
                _value = await _getValueFunc(cancellationToken).ConfigureAwait(false);
                refreshInterval = _refreshIntervalFactory();
                tcs.TrySetResult(true);
                
                Interlocked.Increment(ref _version);

                PublishValueRefreshedEvent(default, stopwatch.Elapsed);
                
                _dateOfPreviousSuccessfulRefresh = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                lock (_lock)
                {
                    ThrowIfDisposed();
                    _state = PendingInitialization;
                    _initializationTaskCompletionSource = null;
                }
                
                tcs.TrySetException(ex);
                
                PublishValueRefreshExceptionEvent(ex, stopwatch.Elapsed);
                
                throw;
            }

            lock (_lock)
            {
                ThrowIfDisposed();
                _state = Ready;
                _initializationTaskCompletionSource = null;
            }

            _refreshTimer = new Timer(
                async _ => await RefreshValueFromTimer().ConfigureAwait(false),
                null,
                (long)refreshInterval.TotalMilliseconds,
                -1);
        }

        public void RefreshValue(CancellationToken cancellationToken = default)
        {
            Task.Run(() => RefreshValueAsync(cancellationToken), cancellationToken).GetAwaiter().GetResult();
        }

        public Task RefreshValueAsync(CancellationToken cancellationToken = default)
        {
            var task = RefreshValueImpl(cancellationToken);

            if (!cancellationToken.CanBeCanceled)
                return task;
            
            var cancellableTcs = new TaskCompletionSource<bool>();
            task.ContinueWith(t =>
            {
                if (t.Status == TaskStatus.RanToCompletion)
                    cancellableTcs.TrySetResult(true);
                else if (t.IsCanceled)
                    cancellableTcs.TrySetCanceled();
                else
                    cancellableTcs.TrySetException(t.Exception);
            }, cancellationToken);
            
            cancellationToken.Register(() => cancellableTcs.TrySetCanceled());

            return cancellableTcs.Task;
        }

        public void Dispose()
        {
            T finalValue;
            lock (_lock)
            {
                if (_state == Disposed)
                    return;

                _state = Disposed;
                _isDisposedCancellationTokenSource.Cancel();
                _isDisposedCancellationTokenSource.Dispose();

                var objectDisposedException = GetObjectDisposedException();
                
                var disposedTcs = new TaskCompletionSource<bool>();
                disposedTcs.SetException(objectDisposedException);

                var initializationTcs = Interlocked.Exchange(ref _initializationTaskCompletionSource, disposedTcs);

                initializationTcs?.TrySetException(objectDisposedException);

                _refreshTimer.Dispose();

                finalValue = _value;
                _value = default;
            }
            
            if (finalValue is IDisposable disposable)
                disposable.Dispose();
        }

        // Only called from the Timer
        private async Task RefreshValueFromTimer()
        {
            try
            {
                await RefreshValueImpl().ConfigureAwait(false);
            }
            catch
            {
                var nextInterval = _refreshIntervalFactory();
                    
                if (_state != Disposed)
                    _refreshTimer.Change((long)nextInterval.TotalMilliseconds, -1);
            }
        }

        private async Task RefreshValueImpl(CancellationToken cancellationToken = default)
        {
            if (_state == Disposed)
                throw GetObjectDisposedException();

            cancellationToken.ThrowIfCancellationRequested();

            var tcs = new TaskCompletionSource<bool>();

            var refreshHandler = GetRefreshHandler();

            if (cancellationToken.CanBeCanceled)
                cancellationToken.Register(() => refreshHandler.MarkCancellation());

            await tcs.Task.ConfigureAwait(false);

            CachedObjectRefreshHandler GetRefreshHandler()
            {
                lock (_lock)
                {
                    if (_currentRefreshHandler is null)
                    {
                        var handler = _currentRefreshHandler = new CachedObjectRefreshHandler(this);
                        handler.AddWaiter(tcs);
                        handler.Start();
                        return handler;
                    }

                    if (_currentRefreshHandler.AddWaiter(tcs))
                        return _currentRefreshHandler;

                    if (_queuedRefreshHandler is null)
                        _queuedRefreshHandler = new CachedObjectRefreshHandler(this);
                    
                    _queuedRefreshHandler.AddWaiter(tcs);
                    return _queuedRefreshHandler;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void PublishValueRefreshedEvent(T previousValue, TimeSpan duration)
        {
            var onValueRefreshedEvent = OnValueRefreshed;
            if (onValueRefreshedEvent is null)
                return;
            
            var message = new CachedObjectValueRefreshedEvent<T>(
                _value,
                previousValue,
                duration,
                _dateOfPreviousSuccessfulRefresh,
                _version);

            onValueRefreshedEvent(this, message);
        }
        
        private void PublishValueRefreshExceptionEvent(Exception exception, TimeSpan duration)
        {
            var onValueRefreshExceptionEvent = OnValueRefreshException;
            if (onValueRefreshExceptionEvent is null)
                return;
            
            var message = new CachedObjectValueRefreshExceptionEvent<T>(
                exception,
                _value,
                duration,
                _dateOfPreviousSuccessfulRefresh,
                _version);

            onValueRefreshExceptionEvent(this, message);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ThrowIfDisposed()
        {
            if (_state == Disposed)
                throw GetObjectDisposedException();
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ObjectDisposedException GetObjectDisposedException()
        {
            return new ObjectDisposedException(this.GetType().ToString());
        }

        private sealed class CachedObjectRefreshHandler : IDisposable
        {
            private readonly CachedObject<T> _parent;
            private readonly List<TaskCompletionSource<bool>> _tcsList;
            private readonly CancellationTokenSource _cts;
            private readonly IDisposable _cancellationRegistration;
            private readonly object _lock = new object();
            private State _state;
            private int _cancellationCount;

            public CachedObjectRefreshHandler(CachedObject<T> parent)
            {
                _parent = parent;
                _tcsList = new List<TaskCompletionSource<bool>>();
                _cts = new CancellationTokenSource();
                _cancellationRegistration = _parent._isDisposedCancellationTokenSource.Token.Register(() => _cts.Cancel());
            }

            public bool AddWaiter(TaskCompletionSource<bool> tcs)
            {
                lock (_lock)
                {
                    if (_state != State.Queued)
                        return false;

                    _tcsList.Add(tcs);
                    return true;
                }
            }
            
            public void MarkCancellation()
            {
                var cancellationCount = Interlocked.Increment(ref _cancellationCount);

                if (cancellationCount != _tcsList.Count)
                    return;
                
                lock (_lock)
                {
                    if (_state != State.Running)
                        return;

                    _cts.Cancel();
                }
            }

            public void Start()
            {
                lock (_lock)
                {
                    if (_state == State.Disposed)
                        throw new ObjectDisposedException(this.GetType().ToString());
                    
                    _parent.ThrowIfDisposed();
                    
                    if (_parent._refreshValueFuncTimeout.HasValue)
                        _cts.CancelAfter(_parent._refreshValueFuncTimeout.Value);

                    _state = State.Running;
                }
                
                Task.Run(RunAsync);
            }

            public void Dispose()
            {
                lock (_lock)
                {
                    _state = State.Disposed;
                    _cancellationRegistration.Dispose();
                    _cts.Dispose();
                }
            }

            private async Task RunAsync()
            {
                var stopwatch = Stopwatch.StartNew();
                try
                {
                    _cts.Token.ThrowIfCancellationRequested();
                    
                    var newValue = await _parent._getValueFunc(_cts.Token).ConfigureAwait(false);
                    var oldValue = _parent._value;
                    _parent._value = newValue;
                
                    Interlocked.Increment(ref _parent._version);
                
                    _parent.PublishValueRefreshedEvent(oldValue, stopwatch.Elapsed);
                
                    _parent._dateOfPreviousSuccessfulRefresh = DateTime.UtcNow;
                
                    foreach (var tcs in _tcsList)
                        tcs.TrySetResult(true);

                    var nextInterval = _parent._refreshIntervalFactory();
                    
                    if (_parent._state != Disposed)
                        _parent._refreshTimer.Change((long)nextInterval.TotalMilliseconds, -1);
                }
                catch (Exception ex)
                {
                    _parent.PublishValueRefreshExceptionEvent(ex, stopwatch.Elapsed);
                    if (_cts.IsCancellationRequested)
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
                    this.Dispose();
                    
                    lock (_parent._lock)
                    {
                        _parent._currentRefreshHandler = _parent._queuedRefreshHandler;
                        _parent._queuedRefreshHandler = null;

                        _parent._currentRefreshHandler?.Start();
                    }
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