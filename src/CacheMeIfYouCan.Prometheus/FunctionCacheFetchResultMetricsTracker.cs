using System;
using System.Linq;
using CacheMeIfYouCan.Notifications;
using Prometheus;
using Histogram = Prometheus.Histogram;

namespace CacheMeIfYouCan.Prometheus
{
    internal static class FunctionCacheFetchResultMetricsTracker
    {
        private static readonly Counter TotalItemsRequestedCounter;
        private static readonly Histogram RequestDurationsMs;
        private static readonly Histogram EarlyFetchTimeToLivesMs;
        private const double TicksPerMs = TimeSpan.TicksPerMillisecond;
        
        static FunctionCacheFetchResultMetricsTracker()
        {
            TotalItemsRequestedCounter = Metrics.CreateCounter("FunctionCacheFetch_TotalItemsRequestedCounter", null, "interface", "function", "success");
            RequestDurationsMs = Metrics.CreateHistogram("FunctionCacheFetch_RequestDurationsMs", null, null, "interface", "function", "success", "duplicate", "reason");
            EarlyFetchTimeToLivesMs = Metrics.CreateHistogram("FunctionCacheFetch_EarlyFetchTimeToLivesMs", null, null, "interface", "function", "success", "reason");
        }
        
        public static void OnFetch(FunctionCacheFetchResult result)
        {
            var interfaceName = result.FunctionInfo.InterfaceType?.Name ?? String.Empty;
            var functionName = result.FunctionInfo.FunctionName ?? String.Empty;

            TotalItemsRequestedCounter
                .Labels(interfaceName, functionName, result.Success.ToString())
                .Inc(result.Results.Count());
            
            foreach (var fetch in result.Results)
            {
                RequestDurationsMs
                    .Labels(interfaceName, functionName, fetch.Success.ToString(), fetch.Duplicate.ToString(), result.Reason.ToString())
                    .Observe(ConvertToMilliseconds(fetch.Duration));
            }

//            if (result.ExistingTtl.HasValue)
//            {
//                EarlyFetchTimeToLivesMs
//                    .Labels(interfaceName, functionName, result.Success.ToString(), result.Reason.ToString())
//                    .Observe(result.ExistingTtl.Value.TotalMilliseconds);
//            }
        }

        private static double ConvertToMilliseconds(long ticks)
        {
            return ticks / TicksPerMs;
        }
    }
}