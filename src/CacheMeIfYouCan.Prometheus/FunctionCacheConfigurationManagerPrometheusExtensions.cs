using CacheMeIfYouCan.Configuration;

namespace CacheMeIfYouCan.Prometheus
{
    public static class FunctionCacheConfigurationManagerPrometheusExtensions
    {
        public static FunctionCacheConfigurationManager<TK, TV> WithMetrics<TK, TV>(
            this FunctionCacheConfigurationManager<TK, TV> configManager)
        {
            return WithMetrics<FunctionCacheConfigurationManager<TK, TV>, TK, TV>(configManager);
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> WithMetrics<TK, TV>(
            this MultiKeyFunctionCacheConfigurationManager<TK, TV> configManager)
        {
            return WithMetrics<MultiKeyFunctionCacheConfigurationManager<TK, TV>, TK, TV>(configManager);
        }
        
        private static TConfig WithMetrics<TConfig, TK, TV>(
            TConfig configManager)
            where TConfig : FunctionCacheConfigurationManagerBase<TConfig, TK, TV>
        {
            configManager.OnResult(FunctionCacheGetResultMetricsTracker.OnResult);
            configManager.OnFetch(FunctionCacheFetchResultMetricsTracker.OnFetch);
            
            return configManager;
        }
    }
}