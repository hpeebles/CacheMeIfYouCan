using System;
using Prometheus;
using Histogram = Prometheus.Histogram;

namespace CacheMeIfYouCan.Prometheus
{
    public static class MetricsTracker
    {
        private static readonly Histogram RequestDurationsMs;
        private static readonly Histogram FetchDurationsMs;
        private static readonly Histogram EarlyFetchTimeToLivesMs;
        
        static MetricsTracker()
        {
            RequestDurationsMs = Metrics.CreateHistogram("RequestDurationsMs", null, null, "name", "outcome", "cache_type");
            FetchDurationsMs = Metrics.CreateHistogram("FetchDurationsMs", null, null, "name", "success", "duplicate", "is_early_fetch");
            EarlyFetchTimeToLivesMs = Metrics.CreateHistogram("EarlyFetchTimeToLivesMs", null, null, "name", "success");
        }

        public static void OnResult(FunctionCacheGetResult result)
        {
            RequestDurationsMs
                .Labels(result.CacheName ?? String.Empty, result.Outcome.ToString(), result.CacheType ?? String.Empty)
                .Observe(ConvertToMilliseconds(result.Duration));
        }
        
        public static void OnFetch(FunctionCacheFetchResult result)
        {
            var isEarlyFetch = result.ExistingTtl.HasValue;
            
            FetchDurationsMs
                .Labels(result.CacheName ?? String.Empty, result.Success.ToString(), result.Duplicate.ToString(), isEarlyFetch.ToString())
                .Observe(ConvertToMilliseconds(result.Duration));

            if (isEarlyFetch)
            {
                EarlyFetchTimeToLivesMs
                    .Labels(result.CacheName ?? String.Empty, result.Success.ToString())
                    .Observe(result.ExistingTtl.Value.TotalMilliseconds);
            }
        }

        private static double ConvertToMilliseconds(long ticks)
        {
            return (double)ticks / 10000;
        }
    }
}