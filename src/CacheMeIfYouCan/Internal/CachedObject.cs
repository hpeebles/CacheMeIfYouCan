using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using CacheMeIfYouCan.Notifications;

namespace CacheMeIfYouCan.Internal
{
    internal class CachedObject<T> : ICachedObject<T>
    {
        private readonly Func<Task<T>> _getValueFunc;
        private readonly string _name;
        private readonly Func<bool, T, TimeSpan> _refreshIntervalFunc;
        private readonly Action<CachedObjectRefreshResult<T>> _onRefresh;
        private readonly Action<CachedObjectRefreshException<T>> _onException;
        private readonly SemaphoreSlim _semaphore;
        private readonly CancellationTokenSource _cts;
        private int _refreshAttemptCount;
        private int _successfulRefreshCount;
        private DateTime _lastRefreshAttempt;
        private DateTime _lastSuccessfulRefresh;
        private T _value;
        private TimeSpan _nextRefreshInterval;
        private int _state;

        public CachedObject(
            Func<Task<T>> getValueFunc,
            string name,
            Func<bool, T, TimeSpan> refreshIntervalFunc,
            Action<CachedObjectRefreshResult<T>> onRefresh,
            Action<CachedObjectRefreshException<T>> onException)
        {
            _getValueFunc = getValueFunc ?? throw new ArgumentNullException(nameof(getValueFunc));
            _name = name;
            _refreshIntervalFunc = refreshIntervalFunc ?? throw new ArgumentNullException(nameof(refreshIntervalFunc));
            _onRefresh = onRefresh;
            _onException = onException;
            _semaphore = new SemaphoreSlim(1);
            _cts = new CancellationTokenSource();
        }

        public T Value
        {
            get
            {
                if (_state == 1)
                    return _value;
                
                if (_state == 0)
                    Task.Run(Initialize).GetAwaiter().GetResult();
                
                if (_state == 2)
                    throw new ObjectDisposedException(nameof(CachedObject<T>));
                
                return _value;
            }
        }

        public async Task<CachedObjectInitializeOutcome> Initialize()
        {
            if (_state == 0)
            {
                await _semaphore.WaitAsync();

                try
                {
                    if (_state == 0)
                        await InitializeImpl();
                }
                finally
                {
                    _semaphore.Release();
                }
            }

            switch (_state)
            {
                case 1:
                    return CachedObjectInitializeOutcome.Success;
                
                case 2:
                    return CachedObjectInitializeOutcome.Disposed;

                default:
                    return CachedObjectInitializeOutcome.Failure;
            }
        }

        public void Dispose()
        {
            Interlocked.Exchange(ref _state, 2);
            CachedObjectInitializer.Remove(this);
            _cts.Cancel();
            _cts.Dispose();
        }

        private async Task InitializeImpl()
        {
            var result = await RefreshValue();

            if (!result.Success)
                return;
            
            Interlocked.Exchange(ref _state, 1);

            Task.Run(RunBackgroundRefreshTask);
        }

        private async Task RunBackgroundRefreshTask()
        {
            while (!_cts.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(_nextRefreshInterval, _cts.Token);

                    await RefreshValue();
                }
                catch (OperationCanceledException)
                {
                    return;
                }
                catch
                {
                    // This can only be reached if _onRefresh or _onException throw an exception. In these scenarios
                    // ensure _nextRefreshInterval isn't zero or tiny as we don't want to get caught in a tight loop
                    if (_nextRefreshInterval < TimeSpan.FromSeconds(1))
                        _nextRefreshInterval = TimeSpan.FromSeconds(1);
                }
            }
        }

        private async Task<CachedObjectRefreshResult<T>> RefreshValue()
        {
            var start = DateTime.UtcNow;
            var stopwatchStart = Stopwatch.GetTimestamp();
            CachedObjectRefreshException<T> refreshException = null;
            T newValue;
            _refreshAttemptCount++;

            try
            {
                newValue = await _getValueFunc();
                _value = newValue;
                _successfulRefreshCount++;
            }
            catch (Exception ex)
            {
                refreshException = new CachedObjectRefreshException<T>(_name, ex);
                newValue = default;
                
                _onException?.Invoke(refreshException);
            }
            
            try
            {
                // Don't update _nextRefreshInterval if this is a failed Initialize attempt
                if (!(_state == 0 && refreshException != null))
                    _nextRefreshInterval = _refreshIntervalFunc(refreshException == null, newValue);
            }
            catch (Exception ex)
            {
                var exception = new CachedObjectRefreshException<T>(_name, ex);

                if (refreshException == null)
                    refreshException = exception;
                
                _onException?.Invoke(exception);
            }
            
            var result = new CachedObjectRefreshResult<T>(
                _name,
                start,
                StopwatchHelper.GetDuration(stopwatchStart),
                refreshException,
                newValue,
                _refreshAttemptCount,
                _successfulRefreshCount,
                _lastRefreshAttempt,
                _lastSuccessfulRefresh,
                _nextRefreshInterval);

            _lastRefreshAttempt = DateTime.UtcNow;
            
            _onRefresh?.Invoke(result);
            
            if (refreshException == null)
                _lastSuccessfulRefresh = DateTime.UtcNow;

            return result;
        }
    }
}