using System.Collections.Generic;
using CacheMeIfYouCan.Configuration;
using CacheMeIfYouCan.Prometheus.MetricTrackers;

namespace CacheMeIfYouCan.Prometheus
{
    public static class FunctionCacheConfigurationManagerPrometheusExtensions
    {
        public static SingleKeyFunctionCacheConfigurationManager<TK, TV> WithMetrics<TK, TV>(
            this SingleKeyFunctionCacheConfigurationManager<TK, TV> configManager,
            FunctionCacheMetrics functionCacheMetrics = FunctionCacheMetrics.All,
            CacheMetrics cacheMetrics = CacheMetrics.All)
        {
            return WithMetrics<SingleKeyFunctionCacheConfigurationManager<TK, TV>, TK, TV>(
                configManager,
                functionCacheMetrics,
                cacheMetrics);
        }
        
        public static SingleKeyFunctionCacheConfigurationManagerSync<TK, TV> WithMetrics<TK, TV>(
            this SingleKeyFunctionCacheConfigurationManagerSync<TK, TV> configManager,
            FunctionCacheMetrics functionCacheMetrics = FunctionCacheMetrics.All,
            CacheMetrics cacheMetrics = CacheMetrics.All)
        {
            return WithMetrics<SingleKeyFunctionCacheConfigurationManagerSync<TK, TV>, TK, TV>(
                configManager,
                functionCacheMetrics,
                cacheMetrics);
        }
        
        public static EnumerableKeyFunctionCacheConfigurationManager<TReq, TRes, TK, TV> WithMetrics<TReq, TRes, TK, TV>(
            this EnumerableKeyFunctionCacheConfigurationManager<TReq, TRes, TK, TV> configManager,
            FunctionCacheMetrics functionCacheMetrics = FunctionCacheMetrics.All,
            CacheMetrics cacheMetrics = CacheMetrics.All)
            where TReq : IEnumerable<TK>
            where TRes : IDictionary<TK, TV>
        {
            return WithMetrics<EnumerableKeyFunctionCacheConfigurationManager<TReq, TRes, TK, TV>, TK, TV>(
                configManager,
                functionCacheMetrics,
                cacheMetrics);
        }
        
        public static EnumerableKeyFunctionCacheConfigurationManagerSync<TReq, TRes, TK, TV> WithMetrics<TReq, TRes, TK, TV>(
            this EnumerableKeyFunctionCacheConfigurationManagerSync<TReq, TRes, TK, TV> configManager,
            FunctionCacheMetrics functionCacheMetrics = FunctionCacheMetrics.All,
            CacheMetrics cacheMetrics = CacheMetrics.All)
            where TReq : IEnumerable<TK>
            where TRes : IDictionary<TK, TV>
        {
            return WithMetrics<EnumerableKeyFunctionCacheConfigurationManagerSync<TReq, TRes, TK, TV>, TK, TV>(
                configManager,
                functionCacheMetrics,
                cacheMetrics);
        }
        
        public static MultiParamFunctionCacheConfigurationManager<TK1, TK2, TV> WithMetrics<TK1, TK2, TV>(
            this MultiParamFunctionCacheConfigurationManager<TK1, TK2, TV> configManager,
            FunctionCacheMetrics functionCacheMetrics = FunctionCacheMetrics.All,
            CacheMetrics cacheMetrics = CacheMetrics.All)
        {
            return WithMetrics<MultiParamFunctionCacheConfigurationManager<TK1, TK2, TV>, (TK1, TK2), TV>(
                configManager,
                functionCacheMetrics,
                cacheMetrics);
        }
        
        public static MultiParamFunctionCacheConfigurationManagerSync<TK1, TK2, TV> WithMetrics<TK1, TK2, TV>(
            this MultiParamFunctionCacheConfigurationManagerSync<TK1, TK2, TV> configManager,
            FunctionCacheMetrics functionCacheMetrics = FunctionCacheMetrics.All,
            CacheMetrics cacheMetrics = CacheMetrics.All)
        {
            return WithMetrics<MultiParamFunctionCacheConfigurationManagerSync<TK1, TK2, TV>, (TK1, TK2), TV>(
                configManager,
                functionCacheMetrics,
                cacheMetrics);
        }
        
        public static MultiParamFunctionCacheConfigurationManager<TK1, TK2, TK3, TV> WithMetrics<TK1, TK2, TK3, TV>(
            this MultiParamFunctionCacheConfigurationManager<TK1, TK2, TK3, TV> configManager,
            FunctionCacheMetrics functionCacheMetrics = FunctionCacheMetrics.All,
            CacheMetrics cacheMetrics = CacheMetrics.All)
        {
            return WithMetrics<MultiParamFunctionCacheConfigurationManager<TK1, TK2, TK3, TV>, (TK1, TK2, TK3), TV>(
                configManager,
                functionCacheMetrics,
                cacheMetrics);
        }
        
        public static MultiParamFunctionCacheConfigurationManagerSync<TK1, TK2, TK3, TV> WithMetrics<TK1, TK2, TK3, TV>(
            this MultiParamFunctionCacheConfigurationManagerSync<TK1, TK2, TK3, TV> configManager,
            FunctionCacheMetrics functionCacheMetrics = FunctionCacheMetrics.All,
            CacheMetrics cacheMetrics = CacheMetrics.All)
        {
            return WithMetrics<MultiParamFunctionCacheConfigurationManagerSync<TK1, TK2, TK3, TV>, (TK1, TK2, TK3), TV>(
                configManager,
                functionCacheMetrics,
                cacheMetrics);
        }
        
        public static MultiParamFunctionCacheConfigurationManager<TK1, TK2, TK3, TK4, TV> WithMetrics<TK1, TK2, TK3, TK4, TV>(
            this MultiParamFunctionCacheConfigurationManager<TK1, TK2, TK3, TK4, TV> configManager,
            FunctionCacheMetrics functionCacheMetrics = FunctionCacheMetrics.All,
            CacheMetrics cacheMetrics = CacheMetrics.All)
        {
            return WithMetrics<MultiParamFunctionCacheConfigurationManager<TK1, TK2, TK3, TK4, TV>, (TK1, TK2, TK3, TK4), TV>(
                configManager,
                functionCacheMetrics,
                cacheMetrics);
        }
        
        public static MultiParamFunctionCacheConfigurationManagerSync<TK1, TK2, TK3, TK4, TV> WithMetrics<TK1, TK2, TK3, TK4, TV>(
            this MultiParamFunctionCacheConfigurationManagerSync<TK1, TK2, TK3, TK4, TV> configManager,
            FunctionCacheMetrics functionCacheMetrics = FunctionCacheMetrics.All,
            CacheMetrics cacheMetrics = CacheMetrics.All)
        {
            return WithMetrics<MultiParamFunctionCacheConfigurationManagerSync<TK1, TK2, TK3, TK4, TV>, (TK1, TK2, TK3, TK4), TV>(
                configManager,
                functionCacheMetrics,
                cacheMetrics);
        }
        
        private static TConfig WithMetrics<TConfig, TK, TV>(
            TConfig configManager,
            FunctionCacheMetrics functionCacheMetrics,
            CacheMetrics cacheMetrics)
            where TConfig : FunctionCacheConfigurationManagerBase<TConfig, TK, TV>
        {
            if (functionCacheMetrics.HasFlag(FunctionCacheMetrics.GetResult))
                configManager.OnResult(FunctionCache_GetResult.OnResult);
            
            if (functionCacheMetrics.HasFlag(FunctionCacheMetrics.Fetch))
                configManager.OnFetch(FunctionCache_Fetch.OnFetch);
            
            if (cacheMetrics.HasFlag(CacheMetrics.Get))
                configManager.OnCacheGet(Cache_Get.OnCacheGet);
            
            if (cacheMetrics.HasFlag(CacheMetrics.Set))
                configManager.OnCacheSet(Cache_Set.OnCacheSet);

            if (cacheMetrics.HasFlag(CacheMetrics.Exception))
                configManager.OnCacheException(Cache_Exception.OnCacheException);
            
            return configManager;
        }
    }
}