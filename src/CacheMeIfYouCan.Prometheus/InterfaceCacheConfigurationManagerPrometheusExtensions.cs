using CacheMeIfYouCan.Configuration;

namespace CacheMeIfYouCan.Prometheus
{
    public static class InterfaceCacheConfigurationManagerPrometheusExtensions
    {
        public static CachedProxyConfigurationManager<T> WithMetrics<T>(this CachedProxyConfigurationManager<T> configManager)
        {
            configManager.OnResult(FunctionCacheGetResultMetricsTracker.OnResult);
            configManager.OnFetch(FunctionCacheFetchResultMetricsTracker.OnFetch);
            configManager.OnCacheGet(CacheMetricsTracker.OnCacheGet);
            configManager.OnCacheSet(CacheMetricsTracker.OnCacheSet);
            
            return configManager;
        }
    }
}