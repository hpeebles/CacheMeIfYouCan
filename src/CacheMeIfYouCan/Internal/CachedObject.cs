using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using CacheMeIfYouCan.Events.CachedObject;

namespace CacheMeIfYouCan.Internal
{
    internal sealed class CachedObject<T, TUpdateFuncInput> : CachedObjectBase<T>, ICachedObject<T>, ICachedObjectWithUpdates<T, TUpdateFuncInput>
    {
        private readonly Func<CancellationToken, Task<T>> _getValueFunc;
        private readonly Func<T, TUpdateFuncInput, CancellationToken, Task<T>> _updateValueFunc;
        private readonly Func<TimeSpan> _refreshIntervalFactory;
        private readonly TimeSpan? _refreshValueFuncTimeout;
        private readonly Channel<(TUpdateFuncInput Input, TaskCompletionSource<bool> Tcs, CancellationToken CancellationToken)> _updatesQueue;
        
        // This is to ensure we never run updates and refreshes at the same time. Refreshes are the priority so no
        // updates will start while there are pending refreshes.
        private readonly SemaphoreSlim _refreshOrUpdateMutex;
        
        private readonly object _lock = new object();
        private CachedObjectRefreshHandler _currentRefreshHandler;
        private CachedObjectRefreshHandler _queuedRefreshHandler;
        private Timer _refreshTimer;

        public CachedObject(
            Func<CancellationToken, Task<T>> getValueFunc,
            Func<T, TUpdateFuncInput, CancellationToken, Task<T>> updateValueFunc,
            Func<TimeSpan> refreshIntervalFactory,
            TimeSpan? refreshValueFuncTimeout)
            : this(getValueFunc, refreshIntervalFactory, refreshValueFuncTimeout)
        {
            _updateValueFunc = updateValueFunc;
            _updatesQueue = Channel.CreateUnbounded<(TUpdateFuncInput, TaskCompletionSource<bool>, CancellationToken)>(new UnboundedChannelOptions { SingleReader = true });
            Task.Run(ProcessUpdates);
        }
        
        public CachedObject(
            Func<CancellationToken, Task<T>> getValueFunc,
            Func<TimeSpan> refreshIntervalFactory,
            TimeSpan? refreshValueFuncTimeout)
        {
            _getValueFunc = getValueFunc;
            _refreshIntervalFactory = refreshIntervalFactory;
            _refreshValueFuncTimeout = refreshValueFuncTimeout;
            _refreshOrUpdateMutex = new SemaphoreSlim(1, 1);
        }

        protected override async Task<T> GetInitialValue(CancellationToken cancellationToken)
        {
            CancellationTokenSource cts = null;
            if (_refreshValueFuncTimeout.HasValue)
            {
                cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

                cts.CancelAfter(_refreshValueFuncTimeout.Value);

                cancellationToken = cts.Token;
            }

            try
            {
                return await _getValueFunc(cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                cts?.Dispose();
            }
        }
        
        protected override void PostInitializationAction()
        {
            if (_refreshIntervalFactory is null)
                return;

            var refreshInterval = _refreshIntervalFactory();

            _refreshTimer = new Timer(
                async _ => await RefreshValueFromTimer().ConfigureAwait(false),
                null,
                (long)refreshInterval.TotalMilliseconds,
                -1);
            
            RegisterDisposable(_refreshTimer);
        }

        public event EventHandler<ValueUpdatedEvent<T, TUpdateFuncInput>> OnValueUpdated;
        public event EventHandler<ValueUpdateExceptionEvent<T, TUpdateFuncInput>> OnValueUpdateException;
        
        public override Task RefreshValueAsync(
            TimeSpan skipIfPreviousRefreshStartedWithinTimeFrame = default,
            CancellationToken cancellationToken = default)
        {
            var task = RefreshValueImpl(skipIfPreviousRefreshStartedWithinTimeFrame, cancellationToken);

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

        void ICachedObjectWithUpdates<T, TUpdateFuncInput>.UpdateValue(TUpdateFuncInput updateFuncInput, CancellationToken cancellationToken)
        {
            Task.Run(() => ((ICachedObjectWithUpdates<T, TUpdateFuncInput>)this).UpdateValueAsync(updateFuncInput, cancellationToken), cancellationToken)
                .GetAwaiter().GetResult();
        }

        Task ICachedObjectWithUpdates<T, TUpdateFuncInput>.UpdateValueAsync(TUpdateFuncInput updateFuncInput, CancellationToken cancellationToken)
        {
            if (_updatesQueue is null)
                throw new InvalidOperationException();
            
            var tcs = new TaskCompletionSource<bool>();

            if (!_updatesQueue.Writer.TryWrite((updateFuncInput, tcs, cancellationToken)))
                throw GetObjectDisposedException();

            return tcs.Task;
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
                    
                if (!IsDisposed())
                    _refreshTimer.Change((long)nextInterval.TotalMilliseconds, -1);
            }
        }

        private async Task RefreshValueImpl(
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
                        handler.AddWaiter_WithinParentLock(tcs);
                        handler.Start_WithinParentLock(true);
                        return handler;
                    }

                    if (_currentRefreshHandler.AddWaiter_WithinParentLock(tcs))
                        return _currentRefreshHandler;

                    if (_queuedRefreshHandler is null)
                        _queuedRefreshHandler = new CachedObjectRefreshHandler(this);
                    
                    _queuedRefreshHandler.AddWaiter_WithinParentLock(tcs, skipIfPreviousRefreshStartedSince);
                    return _queuedRefreshHandler;
                }
            }
        }

        private async void ProcessUpdates()
        {
            try
            {
                while (await _updatesQueue
                    .Reader
                    .WaitToReadAsync(_isDisposedCancellationTokenSource.Token)
                    .ConfigureAwait(false))
                {
                    while (_updatesQueue.Reader.TryRead(out var next))
                    {
                        if (next.CancellationToken.IsCancellationRequested)
                            continue;

                        using var cts = CancellationTokenSource.CreateLinkedTokenSource(
                            _isDisposedCancellationTokenSource.Token,
                            next.CancellationToken);

                        if (!_refreshOrUpdateMutex.Wait(0))
                        {
                            try
                            {
                                await _refreshOrUpdateMutex.WaitAsync(cts.Token).ConfigureAwait(false);
                            }
                            catch (OperationCanceledException)
                            {
                                if (_isDisposedCancellationTokenSource.IsCancellationRequested)
                                    return;

                                continue;
                            }
                        }

                        var stopwatch = Stopwatch.StartNew();
                        try
                        {
                            var updatedValue = await _updateValueFunc(_value, next.Input, cts.Token).ConfigureAwait(false);
                            var previousValue = _value;
                            _value = updatedValue;

                            Interlocked.Increment(ref _version);
                            
                            PublishValueUpdatedEvent(previousValue, next.Input, stopwatch.Elapsed);
                            next.Tcs.TrySetResult(true);
                        }
                        catch (Exception ex)
                        {
                            PublishValueUpdateExceptionEvent(ex, next.Input, stopwatch.Elapsed);
                            
                            if (cts.Token.IsCancellationRequested)
                                next.Tcs.TrySetCanceled();
                            else
                                next.Tcs.TrySetException(ex);
                        }

                        _refreshOrUpdateMutex.Release();
                    }
                }
            }
            catch (OperationCanceledException)
            { }
        }
        
        private void PublishValueUpdatedEvent(T previousValue, TUpdateFuncInput updateFuncInput, TimeSpan duration)
        {
            var onValueUpdatedEvent = OnValueUpdated;
            if (onValueUpdatedEvent is null)
                return;
            
            var message = new ValueUpdatedEvent<T, TUpdateFuncInput>(
                _value,
                previousValue,
                updateFuncInput,
                duration,
                _datePreviousSuccessfulRefreshFinished,
                _version);

            onValueUpdatedEvent(this, message);
        }
        
        private void PublishValueUpdateExceptionEvent(Exception exception, TUpdateFuncInput updateFuncInput, TimeSpan duration)
        {
            var onValueUpdateExceptionEvent = OnValueUpdateException;
            if (onValueUpdateExceptionEvent is null)
                return;
            
            var message = new ValueUpdateExceptionEvent<T, TUpdateFuncInput>(
                exception,
                _value,
                updateFuncInput,
                duration,
                _datePreviousSuccessfulRefreshFinished,
                _version);

            onValueUpdateExceptionEvent(this, message);
        }

        private sealed class CachedObjectRefreshHandler : IDisposable
        {
            private readonly CachedObject<T, TUpdateFuncInput> _parent;
            private readonly List<TaskCompletionSource<bool>> _tcsList;
            private readonly List<(DateTime SkipIfStartedSince, TaskCompletionSource<bool> Tcs)> _skippableTcsList; 
            private readonly CancellationTokenSource _cts;
            private readonly IDisposable _cancellationRegistration;
            private State _state;
            private int _waiterCount;

            public CachedObjectRefreshHandler(CachedObject<T, TUpdateFuncInput> parent)
            {
                _parent = parent;
                _tcsList = new List<TaskCompletionSource<bool>>();
                _skippableTcsList = new List<(DateTime, TaskCompletionSource<bool>)>();
                _cts = new CancellationTokenSource();
                _cancellationRegistration = _parent._isDisposedCancellationTokenSource.Token.Register(() => _cts.Cancel());
            }

            public bool AddWaiter_WithinParentLock(TaskCompletionSource<bool> tcs, DateTime? skipIfPreviousRefreshStartedSince = null)
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
                    throw new ObjectDisposedException(this.GetType().ToString());
                
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
                    _parent._refreshOrUpdateMutex.Release(); // Allow updates to run
                else
                    _parent._currentRefreshHandler.Start_WithinParentLock(false);
            }

            private async Task RunAsync(bool acquireRefreshAndUpdateMutex)
            {
                if (acquireRefreshAndUpdateMutex)
                {
                    if (!_parent._refreshOrUpdateMutex.Wait(0))
                    {
                        try
                        {
                            await _parent._refreshOrUpdateMutex.WaitAsync(_cts.Token).ConfigureAwait(false);
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
                var stopwatch = Stopwatch.StartNew();
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
                    
                    var nextInterval = _parent._refreshIntervalFactory();
                    
                    if (!_parent.IsDisposed())
                        _parent._refreshTimer?.Change((long)nextInterval.TotalMilliseconds, -1);
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