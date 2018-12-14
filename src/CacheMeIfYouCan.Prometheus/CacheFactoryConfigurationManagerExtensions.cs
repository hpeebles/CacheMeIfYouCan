namespace CacheMeIfYouCan.Prometheus
{
    public static class CacheFactoryConfigurationManagerExtensions
    {
        public static IDistributedCacheFactory WithMetrics(
            this IDistributedCacheFactory configManager)
        {
            return configManager
                .OnGetResult(CacheMetricsTracker.OnCacheGet)
                .OnSetResult(CacheMetricsTracker.OnCacheSet);
        }
        
        public static IDistributedCacheFactory<TK, TV> WithMetrics<TK, TV>(
            this IDistributedCacheFactory<TK, TV> configManager)
        {
            return configManager
                .OnGetResult(CacheMetricsTracker.OnCacheGet)
                .OnSetResult(CacheMetricsTracker.OnCacheSet);
        }
        
        public static ILocalCacheFactory WithMetrics(
            this ILocalCacheFactory configManager)
        {
            return configManager
                .OnGetResult(CacheMetricsTracker.OnCacheGet)
                .OnSetResult(CacheMetricsTracker.OnCacheSet);
        }
        
        public static ILocalCacheFactory<TK, TV> WithMetrics<TK, TV>(
            this ILocalCacheFactory<TK, TV> configManager)
        {
            return configManager
                .OnGetResult(CacheMetricsTracker.OnCacheGet)
                .OnSetResult(CacheMetricsTracker.OnCacheSet);
        }
    }
}