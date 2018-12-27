using System;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using CacheMeIfYouCan.Notifications;

namespace CacheMeIfYouCan.Internal
{
    internal class CachedObject<T> : ICachedObject<T>
    {
        private readonly Func<Task<T>> _getValueFunc;
        private readonly Func<CachedObjectRefreshResult<T>, TimeSpan> _refreshIntervalFunc;
        private readonly Action<CachedObjectRefreshResult<T>> _onRefresh;
        private readonly Action<Exception> _onError;
        private readonly SemaphoreSlim _semaphore;
        private int _refreshAttemptCount;
        private int _successfulRefreshCount;
        private DateTime _lastRefreshAttempt;
        private DateTime _lastSuccessfulRefresh;
        private T _value;
        private State _state;
        private IDisposable _refreshTask;
        private TimeSpan _nextRefreshInterval;

        public CachedObject(
            Func<Task<T>> getValueFunc,
            Func<CachedObjectRefreshResult<T>, TimeSpan> refreshIntervalFunc,
            Action<CachedObjectRefreshResult<T>> onRefresh,
            Action<Exception> onError)
        {
            _getValueFunc = getValueFunc ?? throw new ArgumentNullException(nameof(getValueFunc));
            _refreshIntervalFunc = refreshIntervalFunc ?? throw new ArgumentNullException(nameof(refreshIntervalFunc));
            _onRefresh = onRefresh;
            _onError = onError;
            _semaphore = new SemaphoreSlim(1);
            _state = State.PendingInitialization;
        }

        public T Value
        {
            get
            {
                if (_state == State.Ready)
                    return _value;
                
                if (_state == State.PendingInitialization)
                    Initialize().GetAwaiter().GetResult();
                
                if (_state == State.Disposed)
                    throw new ObjectDisposedException(nameof(CachedObject<T>));
                
                return _value;
            }
        }

        public async Task<CachedObjectInitializeOutcome> Initialize()
        {
            if (_state == State.PendingInitialization)
            {
                await _semaphore.WaitAsync();

                try
                {
                    if (_state == State.PendingInitialization)
                        await InitImpl();
                }
                finally
                {
                    _semaphore.Release();
                }
            }

            switch (_state)
            {
                case State.Ready:
                    return CachedObjectInitializeOutcome.Success;
                
                case State.Disposed:
                    return CachedObjectInitializeOutcome.Disposed;

                default:
                    return CachedObjectInitializeOutcome.Failure;
            }
            
            async Task InitImpl()
            {
                var result = await RefreshValue();

                if (!result.Success)
                    return;

                _nextRefreshInterval = _refreshIntervalFunc(result);

                _refreshTask = Observable
                    .Defer(() => Observable
                        .FromAsync(RefreshValue)
                        .Do(r => _nextRefreshInterval = _refreshIntervalFunc(r))
                        .DelaySubscription(_nextRefreshInterval))
                    .Repeat()
                    .Subscribe();
                
                _state = State.Ready;
            }
        }

        public void Dispose()
        {
            _state = State.Disposed;
            CachedObjectInitializer.Remove(this);
            _refreshTask?.Dispose();
        }

        private async Task<CachedObjectRefreshResult<T>> RefreshValue()
        {
            var start = DateTime.UtcNow;
            var stopwatchStart = Stopwatch.GetTimestamp();
            Exception exception = null;
            T newValue;

            try
            {
                newValue = await _getValueFunc();
                _value = newValue;
                _successfulRefreshCount++;
            }
            catch (Exception ex)
            {
                exception = ex;
                newValue = default;

                _onError?.Invoke(ex);
            }
            finally
            {
                _refreshAttemptCount++;
            }
            
            var duration = StopwatchHelper.GetDuration(stopwatchStart);
            
            var result = new CachedObjectRefreshResult<T>(
                start,
                TimeSpan.FromTicks(duration),
                exception,
                newValue,
                _refreshAttemptCount,
                _successfulRefreshCount,
                _lastRefreshAttempt,
                _lastSuccessfulRefresh);
            
            _lastRefreshAttempt = DateTime.UtcNow;
            
            if (exception == null)
                _lastSuccessfulRefresh = DateTime.UtcNow;
            
            _onRefresh?.Invoke(result);
            
            return result;
        }

        private enum State
        {
            PendingInitialization,
            Ready,
            Disposed
        }
    }
}