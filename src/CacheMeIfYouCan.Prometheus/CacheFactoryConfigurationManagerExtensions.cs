using CacheMeIfYouCan.Configuration;

namespace CacheMeIfYouCan.Prometheus
{
    public static class CacheFactoryConfigurationManagerExtensions
    {
        public static DistributedCacheFactoryConfigurationManager WithMetrics(
            this DistributedCacheFactoryConfigurationManager configManager)
        {
            return configManager
                .OnGetResult(CacheMetricsTracker.OnCacheGet)
                .OnSetResult(CacheMetricsTracker.OnCacheSet);
        }
        
        public static DistributedCacheFactoryConfigurationManager<TK, TV> WithMetrics<TK, TV>(
            this DistributedCacheFactoryConfigurationManager<TK, TV> configManager)
        {
            return configManager
                .OnGetResult(CacheMetricsTracker.OnCacheGet)
                .OnSetResult(CacheMetricsTracker.OnCacheSet);
        }
        
        public static LocalCacheFactoryConfigurationManager WithMetrics(
            this LocalCacheFactoryConfigurationManager configManager)
        {
            return configManager
                .OnGetResult(CacheMetricsTracker.OnCacheGet)
                .OnSetResult(CacheMetricsTracker.OnCacheSet);
        }
        
        public static LocalCacheFactoryConfigurationManager<TK, TV> WithMetrics<TK, TV>(
            this LocalCacheFactoryConfigurationManager<TK, TV> configManager)
        {
            return configManager
                .OnGetResult(CacheMetricsTracker.OnCacheGet)
                .OnSetResult(CacheMetricsTracker.OnCacheSet);
        }
    }
}