using CacheMeIfYouCan.Configuration;
using CacheMeIfYouCan.Prometheus.MetricTrackers;

namespace CacheMeIfYouCan.Prometheus
{
    public static class InterfaceCacheConfigurationManagerPrometheusExtensions
    {
        public static CachedProxyConfigurationManager<T> WithMetrics<T>(
            this CachedProxyConfigurationManager<T> configManager,
            FunctionCacheMetrics functionCacheMetrics = FunctionCacheMetrics.All,
            CacheMetrics cacheMetrics = CacheMetrics.All)
        {
            if (functionCacheMetrics.HasFlag(FunctionCacheMetrics.GetResult))
                configManager.OnResult(FunctionCache_GetResult.OnResult);
            
            if (functionCacheMetrics.HasFlag(FunctionCacheMetrics.Fetch))
                configManager.OnFetch(FunctionCache_Fetch.OnFetch);
            
            if (cacheMetrics.HasFlag(CacheMetrics.Get))
                configManager.OnCacheGet(Cache_Get.OnCacheGet);
            
            if (cacheMetrics.HasFlag(CacheMetrics.Set))
                configManager.OnCacheSet(Cache_Set.OnCacheSet);

            if (cacheMetrics.HasFlag(CacheMetrics.Remove))
                configManager.OnCacheRemove(Cache_Remove.OnCacheRemove);

            if (cacheMetrics.HasFlag(CacheMetrics.Exception))
                configManager.OnCacheException(Cache_Exception.OnCacheException);
            
            return configManager;
        }
    }
}