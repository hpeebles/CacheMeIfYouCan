using System;
using System.Threading.Tasks;
using CacheMeIfYouCan.Internal;
using CacheMeIfYouCan.Notifications;

namespace CacheMeIfYouCan.Configuration
{
    public class CachedObjectConfigManager<T>
    {
        private readonly Func<Task<T>> _getValueFunc;
        private Func<CachedObjectRefreshResult<T>, TimeSpan> _refreshIntervalFunc;
        private double _jitterPercentage;
        private Action<CachedObjectRefreshResult<T>> _onRefresh;
        private Action<CachedObjectRefreshException<T>> _onException;

        internal CachedObjectConfigManager(Func<Task<T>> getValueFunc)
        {
            _getValueFunc = getValueFunc;

            if (DefaultSettings.CachedObject.RefreshIntervalFunc != null)
                WithRefreshInterval(DefaultSettings.CachedObject.RefreshIntervalFunc);

            if (DefaultSettings.CachedObject.JitterPercentage.HasValue)
                WithJitterPercentage(DefaultSettings.CachedObject.JitterPercentage.Value);

            if (DefaultSettings.CachedObject.OnRefreshAction != null)
                OnRefresh(DefaultSettings.CachedObject.OnRefreshAction);
            
            if (DefaultSettings.CachedObject.OnExceptionAction != null)
                OnException(DefaultSettings.CachedObject.OnExceptionAction);
        }
        
        public CachedObjectConfigManager<T> WithRefreshInterval(TimeSpan refreshInterval)
        {
            if (refreshInterval == TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(nameof(refreshInterval));

            return WithRefreshInterval(r => refreshInterval);
        }

        public CachedObjectConfigManager<T> WithRefreshInterval(Func<TimeSpan> refreshIntervalFunc)
        {
            return WithRefreshInterval(r => refreshIntervalFunc());
        }

        public CachedObjectConfigManager<T> WithRefreshInterval(
            Func<CachedObjectRefreshResult<T>, TimeSpan> refreshIntervalFunc)
        {
            _refreshIntervalFunc = refreshIntervalFunc;
            return this;
        }
        
        public CachedObjectConfigManager<T> WithJitterPercentage(double percentage)
        {
            if (percentage < 0 || percentage > 100)
                throw new ArgumentOutOfRangeException(nameof(percentage));

            _jitterPercentage = percentage;
            return this;
        }

        public CachedObjectConfigManager<T> OnRefresh(Action<CachedObjectRefreshResult<T>> onRefresh)
        {
            _onRefresh = onRefresh;
            return this;
        }

        public CachedObjectConfigManager<T> OnException(Action<CachedObjectRefreshException<T>> onException)
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

            var cachedObject = new CachedObject<T>(_getValueFunc, refreshIntervalFunc, _onRefresh, _onException);
            
            CachedObjectInitializer.Add(cachedObject);

            return cachedObject;
        }
    }
}