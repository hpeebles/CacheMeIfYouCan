using CacheMeIfYouCan.Configuration;

namespace CacheMeIfYouCan.Prometheus
{
    public static class DefaultCacheConfigurationExtensions
    {
        public static DefaultCacheConfiguration WithMetrics(this DefaultCacheConfiguration config)
        {
            config.OnResult = MetricsTracker.OnResult;
            config.OnFetch = MetricsTracker.OnFetch;
            
            return config;
        }
    }
}