using System;
using System.Linq;
using CacheMeIfYouCan.Notifications;
using Prometheus;
using Histogram = Prometheus.Histogram;

namespace CacheMeIfYouCan.Prometheus
{
    internal static class FunctionCacheGetResultMetricsTracker
    {
        private static readonly Counter TotalItemsRequestedCounter;
        private static readonly Histogram RequestDurationsMs;
        
        static FunctionCacheGetResultMetricsTracker()
        {
            TotalItemsRequestedCounter = Metrics.CreateCounter("FunctionCacheGet_TotalItemsRequestedCounter", null, null, "interface", "function", "success");
            RequestDurationsMs = Metrics.CreateHistogram("RequestDurationsMs", null, null, "interface", "function", "success");
        }

        public static void OnResult(FunctionCacheGetResult result)
        {
            var interfaceName = result.FunctionInfo.InterfaceType?.Name ?? String.Empty;
            var functionName = result.FunctionInfo.FunctionName ?? String.Empty;

            var itemsRequested = result.Results.Count();
            
            TotalItemsRequestedCounter
                .Labels(interfaceName, functionName, result.Success.ToString())
                .Inc(itemsRequested);
            
            RequestDurationsMs
                .Labels(interfaceName, functionName, result.Success.ToString())
                .Observe(ConvertToMilliseconds(result.Duration));
        }

        private static double ConvertToMilliseconds(long ticks)
        {
            return (double)ticks / 10000;
        }
    }
}