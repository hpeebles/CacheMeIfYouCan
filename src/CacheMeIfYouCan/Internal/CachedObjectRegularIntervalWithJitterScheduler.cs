using System;
using System.Threading;
using System.Threading.Tasks;
using CacheMeIfYouCan.Notifications;

namespace CacheMeIfYouCan.Internal
{
    internal class CachedObjectRegularIntervalWithJitterScheduler<T> : ICachedObjectUpdateScheduler<T, Unit>
    {
        private readonly Func<ICachedObjectUpdateAttemptResult<T, Unit>, TimeSpan> _getIntervalFunc;
        private readonly CancellationTokenSource _cts;

        public CachedObjectRegularIntervalWithJitterScheduler(
            Func<ICachedObjectUpdateAttemptResult<T, Unit>, TimeSpan> getIntervalFunc,
            double jitterPercentage)
        {
            if (jitterPercentage.Equals(0))
            {
                _getIntervalFunc = getIntervalFunc;
            }
            else
            {
                var random = new Random();

                // This gives a uniformly distributed value between +/- percentage
                double JitterFunc() => (random.NextDouble() - 0.5) * 2 * jitterPercentage;

                _getIntervalFunc = result => TimeSpan.FromTicks((long) (getIntervalFunc(result).Ticks * (1 + (JitterFunc() / 100))));
            }
            
            _cts = new CancellationTokenSource();
        }
        
        public void Start(
            CachedObjectSuccessfulUpdateResult<T, Unit> initialiseResult, 
            Func<Unit, Task<ICachedObjectUpdateAttemptResult<T, Unit>>> updateValueFunc)
        {
            var firstInterval = _getIntervalFunc(initialiseResult);
            
            Task.Run(() => RunScheduler(firstInterval, () => updateValueFunc(Unit.Instance)));
        }

        public void Dispose()
        {
            _cts.Cancel();
            _cts.Dispose();
        }

        private async Task RunScheduler(TimeSpan nextInterval, Func<Task<ICachedObjectUpdateAttemptResult<T, Unit>>> refreshValueFunc)
        {
            while (!_cts.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(nextInterval, _cts.Token);

                    var result = await refreshValueFunc();

                    nextInterval = _getIntervalFunc(result);
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