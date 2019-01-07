using System;
using System.Reactive.Linq;
using Prometheus;

namespace CacheMeIfYouCan.Prometheus.MetricTrackers
{
    internal static class Cache_CachedItems
    {
        private static readonly Gauge CachedItemsCounter;
        
        static Cache_CachedItems()
        {
            CachedItemsCounter = Metrics.CreateGauge("Cache_ItemsCounter", null, "name", "cachetype");
            
            Observable
                .Interval(TimeSpan.FromSeconds(15))
                .Do(_ => TrackCachedItemCounts())
                .Retry()
                .Subscribe();
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
    }
}