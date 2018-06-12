namespace CacheMeIfYouCan.Prometheus
{
    public static class FunctionCacheConfigurationManagerPrometheusExtensions
    {
        public static FunctionCacheConfigurationManager<T> WithPrometheus<T>(this FunctionCacheConfigurationManager<T> configManager)
        {
            configManager.OnResult(MetricsTracker.OnResult);
            configManager.OnFetch(MetricsTracker.OnFetch);
            
            return configManager;
        }
    }
}