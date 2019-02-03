using System.Runtime.CompilerServices;
using CacheMeIfYouCan.Configuration;
using CacheMeIfYouCan.Prometheus.MetricTrackers;

namespace CacheMeIfYouCan.Prometheus
{
    public static class DefaultCacheConfigurationPrometheusExtensions
    {
        public static DefaultCacheConfiguration WithMetrics(
            this DefaultCacheConfiguration config,
            FunctionCacheMetrics functionCacheMetrics = FunctionCacheMetrics.All,
            CacheMetrics cacheMetrics = CacheMetrics.All,
            bool cachedItemCounts = true,
            bool pendingRequestCounts = true)
        {
            if (cachedItemCounts)
                RuntimeHelpers.RunClassConstructor(typeof(Cache_CachedItems).TypeHandle); 
            
            if (pendingRequestCounts)
                RuntimeHelpers.RunClassConstructor(typeof(PendingRequests).TypeHandle);
            
            if (functionCacheMetrics.HasFlag(FunctionCacheMetrics.GetResult))
                config.OnResult(FunctionCache_GetResult.OnResult);

            if (functionCacheMetrics.HasFlag(FunctionCacheMetrics.Fetch))
                config.OnFetch(FunctionCache_Fetch.OnFetch);
            
            if (cacheMetrics.HasFlag(CacheMetrics.Get))
                config.OnCacheGet(Cache_Get.OnCacheGet);
            
            if (cacheMetrics.HasFlag(CacheMetrics.Set))
                config.OnCacheSet(Cache_Set.OnCacheSet);

            if (cacheMetrics.HasFlag(CacheMetrics.Remove))
                config.OnCacheRemove(Cache_Remove.OnCacheRemove);

            if (cacheMetrics.HasFlag(CacheMetrics.Exception))
                config.OnCacheException(Cache_Exception.OnCacheException);

            return config;
        }
    }
}