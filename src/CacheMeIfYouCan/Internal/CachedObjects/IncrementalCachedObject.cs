using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CacheMeIfYouCan.Internal.CachedObjects
{
    internal sealed class IncrementalCachedObject<T, TUpdates> : UpdateableCachedObjectBase<T, TUpdates>, IIncrementalCachedObject<T, TUpdates>
    {
        private readonly Func<T, CancellationToken, Task<TUpdates>> _getUpdatesFunc;
        private readonly Func<T, TUpdates, CancellationToken, Task<T>> _applyUpdatesFunc;
        private readonly Func<TimeSpan> _updateIntervalFactory;
        private readonly object _lock = new Object();
        private Timer _updateTimer;
        private UpdateHandler _queuedUpdateHandler;

        public IncrementalCachedObject(
            Func<CancellationToken, Task<T>> getValueFunc,
            Func<T, CancellationToken, Task<TUpdates>> getUpdatesFunc,
            Func<T, TUpdates, CancellationToken, Task<T>> applyUpdatesFunc,
            Func<TimeSpan> refreshIntervalFactory,
            Func<TimeSpan> updateIntervalFactory,
            TimeSpan? refreshValueFuncTimeout)
            : base(getValueFunc, refreshIntervalFactory, refreshValueFuncTimeout)
        {
            _getUpdatesFunc = getUpdatesFunc;
            _applyUpdatesFunc = applyUpdatesFunc;
            _updateIntervalFactory = updateIntervalFactory;

            if (!(_updateIntervalFactory is null))
                OnInitialized += (_, __) => SetupAutoUpdates();
        }

        private void SetupAutoUpdates()
        {
            var updateInterval = _updateIntervalFactory();
            
            _updateTimer = new Timer(
                async _ => await UpdateValueFromTimer().ConfigureAwait(false),
                null,
                (long)updateInterval.TotalMilliseconds,
                -1);

            RegisterDisposable(_updateTimer);
        }
        
        // Only called from the Timer
        private async Task UpdateValueFromTimer()
        {
            try
            {
                await UpdateValueAsync().ConfigureAwait(false);
            }
            catch
            {
                UpdateNextUpdateDate();
            }
        }

        public void UpdateValue()
        {
            Task.Run(() => UpdateValueAsync()).GetAwaiter().GetResult();
        }

        public Task UpdateValueAsync(CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            cancellationToken.ThrowIfCancellationRequested();

            if (TryAcquireRefreshOrUpdateValueLockWithoutWaiting())
                return GetAndApplyUpdatesWithinLock(cancellationToken);

            var tcs = new TaskCompletionSource<bool>();
            
            lock (_lock)
            {
                var updateHandler = _queuedUpdateHandler ??= UpdateHandler.CreateAndRun(this);
                updateHandler.AddWaiterWithinParentLock(tcs);

                if (cancellationToken.CanBeCanceled)
                    cancellationToken.Register(() => updateHandler.MarkCancellation());
            }

            return tcs.Task;
        }

        private async Task GetAndApplyUpdatesWithinLock(CancellationToken cancellationToken)
        {
            TUpdates updates;
            try
            {
                updates = await _getUpdatesFunc(_value, cancellationToken).ConfigureAwait(false);
            }
            catch
            {
                ReleaseRefreshOrUpdateValueLock();
                throw;
            }

            await UpdateValueWithinLock(_applyUpdatesFunc, updates, cancellationToken).ConfigureAwait(false);
            
            UpdateNextUpdateDate();
        }
        
        private void UpdateNextUpdateDate()
        {
            if (_updateIntervalFactory is null)
                return;
            
            var nextInterval = _updateIntervalFactory();
                    
            if (!IsDisposed())
                _updateTimer.Change((long)nextInterval.TotalMilliseconds, -1);
        }

        private sealed class UpdateHandler : IDisposable
        {
            private readonly IncrementalCachedObject<T, TUpdates> _parent;
            private readonly List<TaskCompletionSource<bool>> _tcsList;
            private readonly CancellationTokenSource _cts;
            private readonly IDisposable _cancellationRegistration;
            private int _waiterCount;

            private UpdateHandler(IncrementalCachedObject<T, TUpdates> parent)
            {
                _parent = parent;
                _tcsList = new List<TaskCompletionSource<bool>>();
                _cts = new CancellationTokenSource();
                _cancellationRegistration = _parent._isDisposedCancellationTokenSource.Token.Register(() => _cts.Cancel());
            }

            public static UpdateHandler CreateAndRun(IncrementalCachedObject<T, TUpdates> parent)
            {
                var updateHandler = new UpdateHandler(parent);
                Task.Run(updateHandler.Run);
                return updateHandler;
            }
            
            private async Task Run()
            {
                await _parent.AcquireUpdateValueLock(_cts.Token).ConfigureAwait(false);
                
                lock (_parent._lock)
                    _parent._queuedUpdateHandler = null;

                try
                {
                    await _parent.GetAndApplyUpdatesWithinLock(_cts.Token).ConfigureAwait(false);

                    foreach (var tcs in _tcsList)
                        tcs.TrySetResult(true);
                }
                catch (Exception ex)
                {
                    foreach (var tcs in _tcsList)
                        tcs.TrySetException(ex);
                }
            }

            public void AddWaiterWithinParentLock(TaskCompletionSource<bool> tcs)
            {
                _tcsList.Add(tcs);
            }

            public void MarkCancellation()
            {
                var waiterCount = Interlocked.Decrement(ref _waiterCount);

                if (waiterCount == 0)
                    _cts.Cancel();
            }

            public void Dispose()
            {
                lock (_parent._lock)
                    Dispose_WithinParentLock();
            }
            
            private void Dispose_WithinParentLock()
            {
                _cancellationRegistration.Dispose();
                _cts.Dispose();
                _parent.ReleaseRefreshOrUpdateValueLock();
            }
        }
    }
}