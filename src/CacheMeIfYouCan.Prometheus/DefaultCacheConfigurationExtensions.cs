using CacheMeIfYouCan.Configuration;

namespace CacheMeIfYouCan.Prometheus
{
    public static class DefaultCacheConfigurationExtensions
    {
        public static DefaultCacheConfiguration WithMetrics(this DefaultCacheConfiguration config)
        {
            return config
                .WithOnResultAction(FunctionCacheGetResultMetricsTracker.OnResult)
                .WithOnFetchAction(FunctionCacheFetchResultMetricsTracker.OnFetch)
                .WithOnCacheGetAction(CacheMetricsTracker.OnCacheGet)
                .WithOnCacheSetAction(CacheMetricsTracker.OnCacheSet);
        }
    }
}