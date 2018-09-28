using CacheMeIfYouCan.Configuration;

namespace CacheMeIfYouCan.Prometheus
{
    public static class FunctionCacheConfigurationManagerPrometheusExtensions
    {
        public static FunctionCacheConfigurationManager<TK, TV> WithMetrics<TK, TV>(this FunctionCacheConfigurationManager<TK, TV> configManager)
        {
            configManager.OnResult(MetricsTracker.OnResult);
            configManager.OnFetch(MetricsTracker.OnFetch);
            
            return configManager;
        }
    }
}