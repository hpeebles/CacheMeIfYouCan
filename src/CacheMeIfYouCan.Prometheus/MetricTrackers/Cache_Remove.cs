using CacheMeIfYouCan.Notifications;
using Prometheus;

namespace CacheMeIfYouCan.Prometheus.MetricTrackers
{
    internal static class Cache_Remove
    {
        private static readonly Histogram RemoveDurationsMs;
        
        static Cache_Remove()
        {
            var labels = new[] { "name", "cachetype", "success", "keyremoved" };
            var cacheDurationBuckets = new[] { 0.01, 0.03, 0.1, 0.3, 1, 3, 10, 30, 100, 300, 1000, 3000, 10000 };
            
            RemoveDurationsMs = Metrics.CreateHistogram("Cache_RemoveDurationsMs", null, cacheDurationBuckets, labels);
        }
        
        public static void OnCacheRemove(CacheRemoveResult result)
        {
            var labels = new[] { result.CacheName, result.CacheType, result.Success.ToString(), result.KeyRemoved.ToString() };
            
            RemoveDurationsMs
                .Labels(labels)
                .Observe(result.Duration.TotalMilliseconds);
        }
    }
}