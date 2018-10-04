using CacheMeIfYouCan.Configuration;

namespace CacheMeIfYouCan.Prometheus
{
    public static class InterfaceCacheConfigurationManagerPrometheusExtensions
    {
        public static CachedProxyConfigurationManager<T> WithMetrics<T>(this CachedProxyConfigurationManager<T> configManager)
        {
            configManager.OnResult(FunctionCacheGetResultMetricsTracker.OnResult);
            configManager.OnFetch(FunctionCacheFetchResultMetricsTracker.OnFetch);
            
            return configManager;
        }
    }
}