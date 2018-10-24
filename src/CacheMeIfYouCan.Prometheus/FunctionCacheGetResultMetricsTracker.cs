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
        private const double TicksPerMs = TimeSpan.TicksPerMillisecond;
        
        static FunctionCacheGetResultMetricsTracker()
        {
            var labels = new[] { "interface", "function", "success" };
            
            TotalItemsRequestedCounter = Metrics.CreateCounter("FunctionCacheGet_TotalItemsRequestedCounter", null, labels);
            RequestDurationsMs = Metrics.CreateHistogram("RequestDurationsMs", null, null, labels);
        }

        public static void OnResult(FunctionCacheGetResult result)
        {
            var interfaceName = result.FunctionInfo.InterfaceType?.Name ?? String.Empty;
            var functionName = result.FunctionInfo.FunctionName ?? String.Empty;

            var labels = new[] { interfaceName, functionName, result.Success.ToString() };
            
            TotalItemsRequestedCounter
                .Labels(labels)
                .Inc(result.Results.Count());
            
            RequestDurationsMs
                .Labels(labels)
                .Observe(ConvertToMilliseconds(result.Duration));
        }

        private static double ConvertToMilliseconds(long ticks)
        {
            return ticks / TicksPerMs;
        }
    }
}