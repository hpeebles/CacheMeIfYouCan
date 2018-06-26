using System;
using Prometheus;
using Histogram = Prometheus.Histogram;

namespace CacheMeIfYouCan.Prometheus
{
    internal static class MetricsTracker
    {
        private static readonly Histogram RequestDurationsMs;
        private static readonly Histogram FetchDurationsMs;
        private static readonly Histogram EarlyFetchTimeToLivesMs;
        
        static MetricsTracker()
        {
            RequestDurationsMs = Metrics.CreateHistogram("RequestDurationsMs", null, null, "interface", "function", "outcome", "cache_type");
            FetchDurationsMs = Metrics.CreateHistogram("FetchDurationsMs", null, null, "interface", "function", "success", "duplicate", "is_early_fetch");
            EarlyFetchTimeToLivesMs = Metrics.CreateHistogram("EarlyFetchTimeToLivesMs", null, null, "interface", "function", "success");
        }

        public static void OnResult(FunctionCacheGetResult result)
        {
            RequestDurationsMs
                .Labels(result.InterfaceName ?? String.Empty, result.FunctionName ?? String.Empty, result.Outcome.ToString(), result.CacheType ?? String.Empty)
                .Observe(ConvertToMilliseconds(result.Duration));
        }
        
        public static void OnFetch(FunctionCacheFetchResult result)
        {
            var isEarlyFetch = result.ExistingTtl.HasValue;
            
            FetchDurationsMs
                .Labels(result.InterfaceName ?? String.Empty, result.FunctionName ?? String.Empty, result.Success.ToString(), result.Duplicate.ToString(), isEarlyFetch.ToString())
                .Observe(ConvertToMilliseconds(result.Duration));

            if (isEarlyFetch)
            {
                EarlyFetchTimeToLivesMs
                    .Labels(result.InterfaceName ?? String.Empty, result.FunctionName ?? String.Empty, result.Success.ToString())
                    .Observe(result.ExistingTtl.Value.TotalMilliseconds);
            }
        }

        private static double ConvertToMilliseconds(long ticks)
        {
            return (double)ticks / 10000;
        }
    }
}