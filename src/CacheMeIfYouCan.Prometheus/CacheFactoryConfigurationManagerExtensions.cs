using CacheMeIfYouCan.Prometheus.MetricTrackers;

namespace CacheMeIfYouCan.Prometheus
{
    public static class CacheFactoryConfigurationManagerExtensions
    {
        public static IDistributedCacheFactory WithMetrics(
            this IDistributedCacheFactory configManager,
            CacheMetrics metrics = CacheMetrics.All)
        {
            if (metrics.HasFlag(CacheMetrics.Get))
                configManager.OnGetResult(Cache_Get.OnCacheGet);
            
            if (metrics.HasFlag(CacheMetrics.Set))
                configManager.OnSetResult(Cache_Set.OnCacheSet);

            if (metrics.HasFlag(CacheMetrics.Remove))
                configManager.OnRemoveResult(Cache_Remove.OnCacheRemove);
            
            if (metrics.HasFlag(CacheMetrics.Exception))
                configManager.OnException(Cache_Exception.OnCacheException);

            return configManager;
        }
        
        public static IDistributedCacheFactory<TK, TV> WithMetrics<TK, TV>(
            this IDistributedCacheFactory<TK, TV> configManager,
            CacheMetrics metrics = CacheMetrics.All)
        {
            if (metrics.HasFlag(CacheMetrics.Get))
                configManager.OnGetResult(Cache_Get.OnCacheGet);
            
            if (metrics.HasFlag(CacheMetrics.Set))
                configManager.OnSetResult(Cache_Set.OnCacheSet);
            
            if (metrics.HasFlag(CacheMetrics.Remove))
                configManager.OnRemoveResult(Cache_Remove.OnCacheRemove);

            if (metrics.HasFlag(CacheMetrics.Exception))
                configManager.OnException(Cache_Exception.OnCacheException);

            return configManager;
        }
        
        public static ILocalCacheFactory WithMetrics(
            this ILocalCacheFactory configManager,
            CacheMetrics metrics = CacheMetrics.All)
        {
            if (metrics.HasFlag(CacheMetrics.Get))
                configManager.OnGetResult(Cache_Get.OnCacheGet);
            
            if (metrics.HasFlag(CacheMetrics.Set))
                configManager.OnSetResult(Cache_Set.OnCacheSet);
            
            if (metrics.HasFlag(CacheMetrics.Remove))
                configManager.OnRemoveResult(Cache_Remove.OnCacheRemove);

            if (metrics.HasFlag(CacheMetrics.Exception))
                configManager.OnException(Cache_Exception.OnCacheException);

            return configManager;
        }
        
        public static ILocalCacheFactory<TK, TV> WithMetrics<TK, TV>(
            this ILocalCacheFactory<TK, TV> configManager,
            CacheMetrics metrics = CacheMetrics.All)
        {
            if (metrics.HasFlag(CacheMetrics.Get))
                configManager.OnGetResult(Cache_Get.OnCacheGet);
            
            if (metrics.HasFlag(CacheMetrics.Set))
                configManager.OnSetResult(Cache_Set.OnCacheSet);
            
            if (metrics.HasFlag(CacheMetrics.Remove))
                configManager.OnRemoveResult(Cache_Remove.OnCacheRemove);

            if (metrics.HasFlag(CacheMetrics.Exception))
                configManager.OnException(Cache_Exception.OnCacheException);

            return configManager;
        }
    }
}