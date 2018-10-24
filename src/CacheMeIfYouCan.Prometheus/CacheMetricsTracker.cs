using System;
using CacheMeIfYouCan.Notifications;
using Prometheus;

namespace CacheMeIfYouCan.Prometheus
{
    internal static class CacheMetricsTracker
    {
        private static readonly Counter TotalHitsCounter;
        private static readonly Counter TotalMissesCounter;
        private static readonly Counter TotalItemsSetCounter;
        private static readonly Histogram GetDurationsMs;
        private static readonly Histogram SetDurationsMs;
        private const double TicksPerMs = TimeSpan.TicksPerMillisecond;
        
        static CacheMetricsTracker()
        {
            var labels = new[] { "interface", "function", "success", "cachetype" };
            
            TotalHitsCounter = Metrics.CreateCounter("Cache_TotalHitsCounter", null, labels);
            TotalMissesCounter = Metrics.CreateCounter("Cache_TotalMissesCounter", null, labels);
            TotalItemsSetCounter = Metrics.CreateCounter("Cache_TotalItemsSetCounter", null, labels);
            GetDurationsMs = Metrics.CreateHistogram("Cache_GetDurationsMs", null, null, labels);
            SetDurationsMs = Metrics.CreateHistogram("Cache_SetDurationsMs", null, null, labels);
        }
        
        public static void OnCacheGet(CacheGetResult result)
        {
            var interfaceName = result.FunctionInfo.InterfaceType?.Name ?? String.Empty;
            var functionName = result.FunctionInfo.FunctionName ?? String.Empty;

            var labels = new[] { interfaceName, functionName, result.Success.ToString(), result.CacheType };
            
            TotalHitsCounter
                .Labels(labels)
                .Inc(result.HitsCount);
            
            TotalMissesCounter
                .Labels(labels)
                .Inc(result.MissesCount);
            
            GetDurationsMs
                .Labels(labels)
                .Observe(ConvertToMilliseconds(result.Duration));
        }
        
        public static void OnCacheSet(CacheSetResult result)
        {
            var interfaceName = result.FunctionInfo.InterfaceType?.Name ?? String.Empty;
            var functionName = result.FunctionInfo.FunctionName ?? String.Empty;
            
            var labels = new[] { interfaceName, functionName, result.Success.ToString(), result.CacheType };
            
            TotalItemsSetCounter
                .Labels(labels)
                .Inc(result.KeysCount);
            
            SetDurationsMs
                .Labels(labels)
                .Observe(ConvertToMilliseconds(result.Duration));
        }

        private static double ConvertToMilliseconds(long ticks)
        {
            return ticks / TicksPerMs;
        }
    }
}