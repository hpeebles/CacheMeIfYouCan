using CacheMeIfYouCan.Notifications;
using Prometheus;

namespace CacheMeIfYouCan.Prometheus.MetricTrackers
{
    internal static class Cache_Set
    {
        private static readonly Counter ItemsSetCounter;
        private static readonly Histogram SetDurationsMs;

        static Cache_Set()
        {
            var labels = new[] {"name", "cachetype", "success"};
            var cacheDurationBuckets = new[] {0.01, 0.03, 0.1, 0.3, 1, 3, 10, 30, 100, 300, 1000, 3000, 10000};

            ItemsSetCounter = Metrics.CreateCounter("Cache_ItemsSetCounter", null, labels);
            SetDurationsMs = Metrics.CreateHistogram("Cache_SetDurationsMs", null, cacheDurationBuckets, labels);
        }
        
        public static void OnCacheSet(CacheSetResult result)
        {
            var labels = new[] { result.CacheName, result.CacheType, result.Success.ToString() };
            
            ItemsSetCounter
                .Labels(labels)
                .Inc(result.KeysCount);
            
            SetDurationsMs
                .Labels(labels)
                .Observe(result.Duration.TotalMilliseconds);
        }
    }
}