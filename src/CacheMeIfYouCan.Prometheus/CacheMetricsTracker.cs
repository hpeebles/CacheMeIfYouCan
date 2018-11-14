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
            var labels = new[] { "name", "cachetype", "success" };
            var cacheDurationBuckets = new[] { 0.01, 0.03, 0.1, 0.3, 1, 3, 10, 30, 100, 300, 1000, 3000, 10000 };
            
            TotalHitsCounter = Metrics.CreateCounter("Cache_TotalHitsCounter", null, labels);
            TotalMissesCounter = Metrics.CreateCounter("Cache_TotalMissesCounter", null, labels);
            TotalItemsSetCounter = Metrics.CreateCounter("Cache_TotalItemsSetCounter", null, labels);
            GetDurationsMs = Metrics.CreateHistogram("Cache_GetDurationsMs", null, cacheDurationBuckets, labels);
            SetDurationsMs = Metrics.CreateHistogram("Cache_SetDurationsMs", null, cacheDurationBuckets, labels);
            CachedItemsCounter = Metrics.CreateGauge("Cache_ItemsCounter", null, "name", "cachetype");

            Observable
                .Interval(TimeSpan.FromSeconds(5))
                .Do(_ => TrackCachedItemCounts())
                .Retry()
                .Subscribe();
        }
        
        public static void OnCacheGet(CacheGetResult result)
        {
            var labels = new[] { result.CacheName, result.CacheType, result.Success.ToString() };
            
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
            var labels = new[] { result.CacheName, result.CacheType, result.Success.ToString() };
            
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
                CachedItemsCounter
                    .Labels(count.CacheType, count.CacheType)
                    .Set(count.Count);
            }
        }

        private static double ConvertToMilliseconds(long ticks)
        {
            return ticks / TicksPerMs;
        }
    }
}