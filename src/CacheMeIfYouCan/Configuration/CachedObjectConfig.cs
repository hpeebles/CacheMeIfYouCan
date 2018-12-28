using System;
using System.Threading.Tasks;
using CacheMeIfYouCan.Internal;
using CacheMeIfYouCan.Notifications;

namespace CacheMeIfYouCan.Configuration
{
    public class CachedObjectConfig<T>
    {
        private readonly Func<Task<T>> _getValueFunc;
        private Func<CachedObjectRefreshResult<T>, TimeSpan> _refreshIntervalFunc;
        private double _jitterPercentage;
        private Action<CachedObjectRefreshResult<T>> _onRefreshResult;
        private Action<Exception> _onException;

        internal CachedObjectConfig(Func<Task<T>> getValueFunc)
        {
            _getValueFunc = getValueFunc;

            if (DefaultCachedObjectConfig.Configuration.RefreshIntervalFunc != null)
                WithRefreshInterval(DefaultCachedObjectConfig.Configuration.RefreshIntervalFunc);

            if (DefaultCachedObjectConfig.Configuration.JitterPercentage.HasValue)
                WithJitterPercentage(DefaultCachedObjectConfig.Configuration.JitterPercentage.Value);

            if (DefaultCachedObjectConfig.Configuration.OnRefreshResult != null)
                OnRefreshResult(DefaultCachedObjectConfig.Configuration.OnRefreshResult);
            
            if (DefaultCachedObjectConfig.Configuration.OnException != null)
                OnException(DefaultCachedObjectConfig.Configuration.OnException);
        }
        
        public CachedObjectConfig<T> WithRefreshInterval(TimeSpan refreshInterval)
        {
            if (refreshInterval == TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(nameof(refreshInterval));

            return WithRefreshInterval(r => refreshInterval);
        }

        public CachedObjectConfig<T> WithRefreshInterval(Func<TimeSpan> refreshIntervalFunc)
        {
            return WithRefreshInterval(r => refreshIntervalFunc());
        }

        public CachedObjectConfig<T> WithRefreshInterval(
            Func<CachedObjectRefreshResult<T>, TimeSpan> refreshIntervalFunc)
        {
            _refreshIntervalFunc = refreshIntervalFunc;
            return this;
        }
        
        public CachedObjectConfig<T> WithJitterPercentage(double percentage)
        {
            if (percentage < 0 || percentage > 100)
                throw new ArgumentOutOfRangeException(nameof(percentage));

            _jitterPercentage = percentage;
            return this;
        }

        public CachedObjectConfig<T> OnRefreshResult(Action<CachedObjectRefreshResult<T>> onRefreshResult)
        {
            _onRefreshResult = onRefreshResult;
            return this;
        }

        public CachedObjectConfig<T> OnException(Action<Exception> onException)
        {
            _onException = onException;
            return this;
        }

        public ICachedObject<T> Build()
        {
            Func<CachedObjectRefreshResult<T>, TimeSpan> refreshIntervalFunc;

            if (_jitterPercentage.Equals(0))
            {
                refreshIntervalFunc = _refreshIntervalFunc;
            }
            else
            {
                var random = new Random();

                var jitterPercentage = _jitterPercentage;

                // This gives a uniformly distributed value between +/- percentage
                double JitterFunc() => (random.NextDouble() - 0.5) * 2 * jitterPercentage;

                refreshIntervalFunc = r => TimeSpan.FromTicks((long)(_refreshIntervalFunc(r).Ticks * (1 + (JitterFunc() / 100))));
            }

            var cachedObject = new CachedObject<T>(_getValueFunc, refreshIntervalFunc, _onRefreshResult, _onException);
            
            CachedObjectInitializer.Add(cachedObject);

            return cachedObject;
        }
    }
}