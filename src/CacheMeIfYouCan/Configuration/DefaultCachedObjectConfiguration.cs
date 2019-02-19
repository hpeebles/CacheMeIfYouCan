using System;
using CacheMeIfYouCan.Notifications;

namespace CacheMeIfYouCan.Configuration
{
    public class DefaultCachedObjectConfiguration
    {
        internal Func<bool, TimeSpan> RefreshIntervalFunc { get; private set; }
        internal double? JitterPercentage { get; private set; }
        internal Action<CachedObjectRefreshResult> OnRefreshAction { get; private set; }
        internal Action<CachedObjectRefreshException> OnExceptionAction { get; private set; }

        public DefaultCachedObjectConfiguration WithRefreshInterval(TimeSpan refreshInterval)
        {
            if (refreshInterval == TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(nameof(refreshInterval));

            return WithRefreshInterval(success => refreshInterval);
        }

        public DefaultCachedObjectConfiguration WithRefreshInterval(TimeSpan onSuccess, TimeSpan onFailure)
        {
            if (onSuccess == TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(nameof(onSuccess));
            
            if (onFailure == TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(nameof(onFailure));

            return WithRefreshInterval(success => success ? onSuccess : onFailure);
        }

        public DefaultCachedObjectConfiguration WithRefreshInterval(Func<bool, TimeSpan> refreshIntervalFunc)
        {
            RefreshIntervalFunc = refreshIntervalFunc;
            return this;
        }

        public DefaultCachedObjectConfiguration WithJitterPercentage(double jitterPercentage)
        {
            if (jitterPercentage < 0 || jitterPercentage > 100)
                throw new ArgumentOutOfRangeException(nameof(jitterPercentage));

            JitterPercentage = jitterPercentage;
            return this;
        }

        public DefaultCachedObjectConfiguration OnRefresh(Action<CachedObjectRefreshResult> onRefresh)
        {
            OnRefreshAction = onRefresh;
            return this;
        }

        public DefaultCachedObjectConfiguration OnException(Action<CachedObjectRefreshException> onException)
        {
            OnExceptionAction = onException;
            return this;
        }
    }
}
