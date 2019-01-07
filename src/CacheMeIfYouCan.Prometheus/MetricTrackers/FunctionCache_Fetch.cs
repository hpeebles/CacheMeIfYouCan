using CacheMeIfYouCan.Notifications;
using Prometheus;

namespace CacheMeIfYouCan.Prometheus.MetricTrackers
{
    internal static class FunctionCache_Fetch
    {
        private static readonly Counter TotalItemsRequestedCounter;
        private static readonly Histogram RequestDurationsMs;
        private static readonly Histogram EarlyFetchTimeToLivesMs;
        
        static FunctionCache_Fetch()
        {
            var requestDurationBuckets = new[] { 0.1, 0.3, 1, 3, 10, 30, 100, 300, 1000, 3000, 10000, 30000 };
            var earlyFetchTimeToLiveBuckets = new[] { 1.0, 3, 10, 30, 100, 300, 1000, 3000, 10000, 30000, 100000, 300000 };
            
            TotalItemsRequestedCounter = Metrics.CreateCounter("FunctionCacheFetch_TotalItemsRequestedCounter", null, "name", "success");
            RequestDurationsMs = Metrics.CreateHistogram("FunctionCacheFetch_RequestDurationsMs", null, requestDurationBuckets, "name", "success", "duplicate", "reason");
            EarlyFetchTimeToLivesMs = Metrics.CreateHistogram("FunctionCacheFetch_EarlyFetchTimeToLivesMs", null, earlyFetchTimeToLiveBuckets, "name", "success", "reason");
        }
        
        public static void OnFetch(FunctionCacheFetchResult result)
        {
            var name = result.FunctionName;
            
            TotalItemsRequestedCounter
                .Labels(name, result.Success.ToString())
                .Inc(result.Results.Count);
            
            foreach (var fetch in result.Results)
            {
                RequestDurationsMs
                    .Labels(name, fetch.Success.ToString(), fetch.Duplicate.ToString(), result.Reason.ToString())
                    .Observe(fetch.Duration.TotalMilliseconds);
            }

//            if (result.ExistingTtl.HasValue)
//            {
//                EarlyFetchTimeToLivesMs
//                    .Labels(interfaceName, functionName, result.Success.ToString(), result.Reason.ToString())
//                    .Observe(result.ExistingTtl.Value.TotalMilliseconds);
//            }
        }
    }
}