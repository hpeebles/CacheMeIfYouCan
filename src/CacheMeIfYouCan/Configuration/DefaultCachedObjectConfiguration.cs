using System;
using CacheMeIfYouCan.Notifications;

namespace CacheMeIfYouCan.Configuration
{
    public class DefaultCachedObjectConfiguration
    {
        internal Func<CachedObjectRefreshResult, TimeSpan> RefreshIntervalFunc { get; private set; }
        internal double? JitterPercentage { get; private set; }
        internal Action<CachedObjectRefreshResult> OnRefreshAction { get; private set; }
        internal Action<CachedObjectRefreshException> OnExceptionAction { get; private set; }

        public DefaultCachedObjectConfiguration WithRefreshInterval(TimeSpan refreshInterval)
        {
            if (refreshInterval == TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(nameof(refreshInterval));

            return WithRefreshIntervalFunc(r => refreshInterval);
        }

        public DefaultCachedObjectConfiguration WithRefreshIntervalFunc(
            Func<CachedObjectRefreshResult, TimeSpan> refreshIntervalFunc)
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
