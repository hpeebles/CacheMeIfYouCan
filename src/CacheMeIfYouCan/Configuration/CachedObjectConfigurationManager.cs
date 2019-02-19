using System;
using System.Threading.Tasks;
using CacheMeIfYouCan.Internal;
using CacheMeIfYouCan.Notifications;

namespace CacheMeIfYouCan.Configuration
{
    public class CachedObjectConfigurationManager<T>
    {
        private readonly Func<Task<T>> _getValueFunc;
        private string _name;
        private Func<bool, T, TimeSpan> _refreshIntervalFunc;
        private double _jitterPercentage;
        private Action<CachedObjectRefreshResult<T>> _onRefresh;
        private Action<CachedObjectRefreshException> _onException;

        internal CachedObjectConfigurationManager(Func<Task<T>> getValueFunc)
        {
            _getValueFunc = getValueFunc;

            if (DefaultSettings.CachedObject.RefreshIntervalFunc != null)
                WithRefreshInterval((success, value) => DefaultSettings.CachedObject.RefreshIntervalFunc(success));

            if (DefaultSettings.CachedObject.JitterPercentage.HasValue)
                WithJitterPercentage(DefaultSettings.CachedObject.JitterPercentage.Value);

            if (DefaultSettings.CachedObject.OnRefreshAction != null)
                OnRefresh(DefaultSettings.CachedObject.OnRefreshAction);
            
            if (DefaultSettings.CachedObject.OnExceptionAction != null)
                OnException(DefaultSettings.CachedObject.OnExceptionAction);
        }

        public CachedObjectConfigurationManager<T> Named(string name)
        {
            _name = name;
            return this;
        }
        
        public CachedObjectConfigurationManager<T> WithRefreshInterval(TimeSpan refreshInterval)
        {
            if (refreshInterval == TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(nameof(refreshInterval));

            return WithRefreshInterval((success, value) => refreshInterval);
        }
        
        public CachedObjectConfigurationManager<T> WithRefreshInterval(TimeSpan onSuccess, TimeSpan onFailure)
        {
            if (onSuccess == TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(nameof(onSuccess));
            
            if (onFailure == TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(nameof(onFailure));

            return WithRefreshInterval((success, value) => success ? onSuccess : onFailure);
        }

        public CachedObjectConfigurationManager<T> WithRefreshInterval(Func<TimeSpan> refreshIntervalFunc)
        {
            return WithRefreshInterval((success, value) => refreshIntervalFunc());
        }

        public CachedObjectConfigurationManager<T> WithRefreshInterval(Func<bool, T, TimeSpan> refreshIntervalFunc)
        {
            _refreshIntervalFunc = refreshIntervalFunc;
            return this;
        }
        
        public CachedObjectConfigurationManager<T> WithJitterPercentage(double percentage)
        {
            if (percentage < 0 || percentage > 100)
                throw new ArgumentOutOfRangeException(nameof(percentage));

            _jitterPercentage = percentage;
            return this;
        }

        public CachedObjectConfigurationManager<T> OnRefresh(Action<CachedObjectRefreshResult<T>> onRefresh)
        {
            _onRefresh = onRefresh;
            return this;
        }

        public CachedObjectConfigurationManager<T> OnException(Action<CachedObjectRefreshException> onException)
        {
            _onException = onException;
            return this;
        }

        public ICachedObject<T> Build()
        {
            Func<bool, T, TimeSpan> refreshIntervalFunc;

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

                refreshIntervalFunc = (success, value) => TimeSpan.FromTicks((long)(_refreshIntervalFunc(success, value).Ticks * (1 + (JitterFunc() / 100))));
            }

            var name = _name ?? $"{nameof(CachedObject<T>)}_{typeof(T).Name}";
            
            var cachedObject = new CachedObject<T>(_getValueFunc, name, refreshIntervalFunc, _onRefresh, _onException);
            
            CachedObjectInitializer.Add(cachedObject);

            return cachedObject;
        }
    }
}