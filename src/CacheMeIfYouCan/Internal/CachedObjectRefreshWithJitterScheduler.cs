using System;
using System.Threading;
using System.Threading.Tasks;
using CacheMeIfYouCan.Notifications;

namespace CacheMeIfYouCan.Internal
{
    internal class CachedObjectRefreshWithJitterScheduler<T> : ICachedObjectUpdateScheduler<T, Unit>
    {
        private readonly Func<CachedObjectUpdateResult<T, Unit>, TimeSpan> _refreshIntervalFunc;
        private readonly CancellationTokenSource _cts;

        public CachedObjectRefreshWithJitterScheduler(
            Func<CachedObjectUpdateResult<T, Unit>, TimeSpan> refreshIntervalFunc,
            double jitterPercentage)
        {
            if (jitterPercentage.Equals(0))
            {
                _refreshIntervalFunc = refreshIntervalFunc;
            }
            else
            {
                var random = new Random();

                // This gives a uniformly distributed value between +/- percentage
                double JitterFunc() => (random.NextDouble() - 0.5) * 2 * jitterPercentage;

                _refreshIntervalFunc = result => TimeSpan.FromTicks((long) (refreshIntervalFunc(result).Ticks * (1 + (JitterFunc() / 100))));
            }
            
            _cts = new CancellationTokenSource();
        }
        
        public void Start(
            CachedObjectUpdateResult<T, Unit> initialiseResult, 
            Func<Unit, Task<CachedObjectUpdateResult<T, Unit>>> updateValueFunc)
        {
            var firstInterval = _refreshIntervalFunc(initialiseResult);
            
            Task.Run(() => RunScheduler(firstInterval, () => updateValueFunc(Unit.Instance)));
        }

        public void Dispose()
        {
            _cts.Cancel();
            _cts.Dispose();
        }

        private async Task RunScheduler(TimeSpan nextRefreshInterval, Func<Task<CachedObjectUpdateResult<T, Unit>>> refreshValueFunc)
        {
            while (!_cts.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(nextRefreshInterval, _cts.Token);

                    var result = await refreshValueFunc();

                    nextRefreshInterval = _refreshIntervalFunc(result);
                }
                catch (OperationCanceledException)
                {
                    return;
                }
                catch
                {
                    // Errors that reach here will have triggered the onException action (make sure you use it!)
                }
            }
        }
    }
}