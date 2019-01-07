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
                config.WithOnResultAction(FunctionCache_GetResult.OnResult);

            if (functionCacheMetrics.HasFlag(FunctionCacheMetrics.Fetch))
                config.WithOnFetchAction(FunctionCache_Fetch.OnFetch);
            
            if (cacheMetrics.HasFlag(CacheMetrics.Get))
                config.WithOnCacheGetAction(Cache_Get.OnCacheGet);
            
            if (cacheMetrics.HasFlag(CacheMetrics.Set))
                config.WithOnCacheSetAction(Cache_Set.OnCacheSet);

            if (cacheMetrics.HasFlag(CacheMetrics.Exception))
                config.WithOnCacheExceptionAction(Cache_Exception.OnCacheException);

            return config;
        }
    }
}