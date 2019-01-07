using CacheMeIfYouCan.Notifications;
using Prometheus;

namespace CacheMeIfYouCan.Prometheus.MetricTrackers
{
    internal static class FunctionCache_GetResult
    {
        private static readonly Counter TotalItemsRequestedCounter;
        private static readonly Histogram RequestDurationsMs;
        
        static FunctionCache_GetResult()
        {
            var labels = new[] { "name", "success" };
            var requestDurationBuckets = new[] { 0.1, 0.3, 1, 3, 10, 30, 100, 300, 1000, 3000, 10000, 30000 };
            
            TotalItemsRequestedCounter = Metrics.CreateCounter("FunctionCacheGet_TotalItemsRequestedCounter", null, labels);
            RequestDurationsMs = Metrics.CreateHistogram("FunctionCacheGet_RequestDurationsMs", null, requestDurationBuckets, labels);
        }

        public static void OnResult(FunctionCacheGetResult result)
        {
            var labels = new[] { result.FunctionName, result.Success.ToString() };
            
            TotalItemsRequestedCounter
                .Labels(labels)
                .Inc(result.Results.Count);
            
            RequestDurationsMs
                .Labels(labels)
                .Observe(result.Duration.TotalMilliseconds);
        }
    }
}