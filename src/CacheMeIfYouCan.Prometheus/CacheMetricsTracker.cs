using System;
using System.Reactive.Linq;
using CacheMeIfYouCan.Notifications;
using Prometheus;

namespace CacheMeIfYouCan.Prometheus
{
    internal static class CacheMetricsTracker
    {
        private static readonly Counter HitsCounter;
        private static readonly Counter MissesCounter;
        private static readonly Counter ItemsSetCounter;
        private static readonly Counter ExceptionsCounter;
        private static readonly Counter StatusCodesCounter;
        private static readonly Histogram GetDurationsMs;
        private static readonly Histogram SetDurationsMs;
        private static readonly Gauge CachedItemsCounter;
        private static readonly Gauge PendingRequestsCounter;
        private const string NullString = "null";
        
        static CacheMetricsTracker()
        {
            var labels = new[] { "name", "cachetype", "success" };
            var cacheDurationBuckets = new[] { 0.01, 0.03, 0.1, 0.3, 1, 3, 10, 30, 100, 300, 1000, 3000, 10000 };
            
            HitsCounter = Metrics.CreateCounter("Cache_HitsCounter", null, labels);
            MissesCounter = Metrics.CreateCounter("Cache_MissesCounter", null, labels);
            ItemsSetCounter = Metrics.CreateCounter("Cache_ItemsSetCounter", null, labels);
            ExceptionsCounter = Metrics.CreateCounter("Cache_ExceptionsCounter", null, "name", "cachetype", "exceptiontype", "innerexceptiontype");
            StatusCodesCounter = Metrics.CreateCounter("Cache_StatusCodesCounter", null, "name", "cachetype", "statuscode");
            GetDurationsMs = Metrics.CreateHistogram("Cache_GetDurationsMs", null, cacheDurationBuckets, labels);
            SetDurationsMs = Metrics.CreateHistogram("Cache_SetDurationsMs", null, cacheDurationBuckets, labels);
            CachedItemsCounter = Metrics.CreateGauge("Cache_ItemsCounter", null, "name", "cachetype");
            PendingRequestsCounter = Metrics.CreateGauge("PendingRequestsCounter", null, "name", "type");

            Observable
                .Interval(TimeSpan.FromSeconds(5))
                .Do(_ => TrackCachedItemCounts())
                .Do(_ => TrackPendingRequestCounts())
                .Retry()
                .Subscribe();
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
        
        public static void OnCacheSet(CacheSetResult result)
        {
            var labels = new[] { result.CacheName, result.CacheType, result.Success.ToString() };
            
            ItemsSetCounter
                .Labels(labels)
                .Inc(result.KeysCount);
            
            SetDurationsMs
                .Labels(labels)
                .Observe(result.Duration.TotalMilliseconds);
        }

        public static void OnCacheException(CacheException exception)
        {
            var labels = new[]
            {
                exception.CacheName,
                exception.CacheType,
                exception.GetType().Name,
                exception.InnerException?.GetType().Name ?? NullString
            };
            
            ExceptionsCounter
                .Labels(labels)
                .Inc();
        }

        private static void TrackCachedItemCounts()
        {
            foreach (var count in CachedItemCounterContainer.GetCounts())
            {
                CachedItemsCounter
                    .Labels(count.CacheName, count.CacheType)
                    .Set(count.Count);
            }
        }

        private static void TrackPendingRequestCounts()
        {
            foreach (var count in PendingRequestsCounterContainer.GetCounts())
            {
                PendingRequestsCounter
                    .Labels(count.Name, count.Type)
                    .Set(count.Count);
            }
        }
    }
}