using System;

namespace CacheMeIfYouCan.Configuration
{
    public class DefaultCachedObjectConfiguration
    {
        internal TimeSpan? RefreshInterval { get; private set; }
        internal double? JitterPercentage { get; private set; }

        public DefaultCachedObjectConfiguration WithRefreshInterval(TimeSpan refreshInterval)
        {
            if (refreshInterval == TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(nameof(refreshInterval));

            RefreshInterval = refreshInterval;
            return this;
        }

        public DefaultCachedObjectConfiguration WithJitterPercentage(double jitterPercentage)
        {
            if (jitterPercentage < 0 || jitterPercentage > 100)
                throw new ArgumentOutOfRangeException(nameof(jitterPercentage));

            JitterPercentage = jitterPercentage;
            return this;
        }
    }

    public static class DefaultCachedObjectConfig
    {
        public static readonly DefaultCachedObjectConfiguration Configuration = new DefaultCachedObjectConfiguration();
    }
}
