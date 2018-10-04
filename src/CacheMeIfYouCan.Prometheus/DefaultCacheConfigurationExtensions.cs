using CacheMeIfYouCan.Configuration;

namespace CacheMeIfYouCan.Prometheus
{
    public static class DefaultCacheConfigurationExtensions
    {
        public static DefaultCacheConfiguration WithMetrics(this DefaultCacheConfiguration config)
        {
            config.OnResult = FunctionCacheGetResultMetricsTracker.OnResult;
            config.OnFetch = FunctionCacheFetchResultMetricsTracker.OnFetch;
            
            return config;
        }
    }
}