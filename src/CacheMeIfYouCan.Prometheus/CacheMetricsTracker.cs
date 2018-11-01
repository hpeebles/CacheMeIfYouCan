using System;
using System.Reactive.Linq;
using CacheMeIfYouCan.Caches;
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
        private static readonly Gauge CachedItemsCounter;
        private const double TicksPerMs = TimeSpan.TicksPerMillisecond;
        
        static CacheMetricsTracker()
        {
            var labels = new[] { "interface", "function", "success", "cachetype" };
            var cacheDurationBuckets = new[] { 0.1, 0.3, 1, 3, 10, 30, 100, 300, 1000, 3000, 10000, 30000 };
            
            TotalHitsCounter = Metrics.CreateCounter("Cache_TotalHitsCounter", null, labels);
            TotalMissesCounter = Metrics.CreateCounter("Cache_TotalMissesCounter", null, labels);
            TotalItemsSetCounter = Metrics.CreateCounter("Cache_TotalItemsSetCounter", null, labels);
            GetDurationsMs = Metrics.CreateHistogram("Cache_GetDurationsMs", null, cacheDurationBuckets, labels);
            SetDurationsMs = Metrics.CreateHistogram("Cache_SetDurationsMs", null, cacheDurationBuckets, labels);
            CachedItemsCounter = Metrics.CreateGauge("Cache_ItemsCounter", null, "interface", "function", "cachetype");

            Observable
                .Interval(TimeSpan.FromSeconds(5))
                .Do(_ => TrackCachedItemCounts())
                .Retry()
                .Subscribe();
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

        private static void TrackCachedItemCounts()
        {
            foreach (var count in CachedItemCounterContainer.GetCounts())
            {
                var interfaceName = count.FunctionInfo.InterfaceType?.Name ?? String.Empty;
                var functionName = count.FunctionInfo.FunctionName ?? String.Empty;
                
                CachedItemsCounter
                    .Labels(interfaceName, functionName, count.CacheType)
                    .Set(count.Count);
            }
        }

        private static double ConvertToMilliseconds(long ticks)
        {
            return ticks / TicksPerMs;
        }
    }
}