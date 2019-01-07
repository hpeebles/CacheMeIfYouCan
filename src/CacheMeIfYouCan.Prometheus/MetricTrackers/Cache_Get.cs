using CacheMeIfYouCan.Notifications;
using Prometheus;

namespace CacheMeIfYouCan.Prometheus.MetricTrackers
{
    internal static class Cache_Get
    {
        private static readonly Counter HitsCounter;
        private static readonly Counter MissesCounter;
        private static readonly Counter StatusCodesCounter;
        private static readonly Histogram GetDurationsMs;
        
        static Cache_Get()
        {
            var labels = new[] { "name", "cachetype", "success" };
            var cacheDurationBuckets = new[] { 0.01, 0.03, 0.1, 0.3, 1, 3, 10, 30, 100, 300, 1000, 3000, 10000 };
            
            HitsCounter = Metrics.CreateCounter("Cache_HitsCounter", null, labels);
            MissesCounter = Metrics.CreateCounter("Cache_MissesCounter", null, labels);
            StatusCodesCounter = Metrics.CreateCounter("Cache_StatusCodesCounter", null, "name", "cachetype", "statuscode");
            GetDurationsMs = Metrics.CreateHistogram("Cache_GetDurationsMs", null, cacheDurationBuckets, labels);
        }
        
        public static void OnCacheGet(CacheGetResult result)
        {
            var labels = new[] { result.CacheName, result.CacheType, result.Success.ToString() };
            
            HitsCounter
                .Labels(labels)
                .Inc(result.HitsCount);
            
            MissesCounter
                .Labels(labels)
                .Inc(result.MissesCount);

            foreach (var (statusCode, count) in result.StatusCodeCounts)
            {
                StatusCodesCounter
                    .Labels(result.CacheName, result.CacheType, statusCode.ToString())
                    .Inc(count);
            }
            
            GetDurationsMs
                .Labels(labels)
                .Observe(result.Duration.TotalMilliseconds);
        }
    }
}