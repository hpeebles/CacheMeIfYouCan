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
            return WithMetricsImpl<SingleKeyFunctionCacheConfigurationManager<TK, TV>, TK, TV>(
                configManager,
                functionCacheMetrics,
                cacheMetrics);
        }
        
        public static SingleKeyFunctionCacheConfigurationManagerSync<TK, TV> WithMetrics<TK, TV>(
            this SingleKeyFunctionCacheConfigurationManagerSync<TK, TV> configManager,
            FunctionCacheMetrics functionCacheMetrics = FunctionCacheMetrics.All,
            CacheMetrics cacheMetrics = CacheMetrics.All)
        {
            return WithMetricsImpl<SingleKeyFunctionCacheConfigurationManagerSync<TK, TV>, TK, TV>(
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
            return WithMetricsImpl<EnumerableKeyFunctionCacheConfigurationManager<TReq, TRes, TK, TV>, TK, TV>(
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
            return WithMetricsImpl<EnumerableKeyFunctionCacheConfigurationManagerSync<TReq, TRes, TK, TV>, TK, TV>(
                configManager,
                functionCacheMetrics,
                cacheMetrics);
        }
        
        public static MultiParamFunctionCacheConfigurationManager<TK1, TK2, TV> WithMetrics<TK1, TK2, TV>(
            this MultiParamFunctionCacheConfigurationManager<TK1, TK2, TV> configManager,
            FunctionCacheMetrics functionCacheMetrics = FunctionCacheMetrics.All,
            CacheMetrics cacheMetrics = CacheMetrics.All)
        {
            return WithMetricsImpl<MultiParamFunctionCacheConfigurationManager<TK1, TK2, TV>, (TK1, TK2), TV>(
                configManager,
                functionCacheMetrics,
                cacheMetrics);
        }
        
        public static MultiParamFunctionCacheConfigurationManagerSync<TK1, TK2, TV> WithMetrics<TK1, TK2, TV>(
            this MultiParamFunctionCacheConfigurationManagerSync<TK1, TK2, TV> configManager,
            FunctionCacheMetrics functionCacheMetrics = FunctionCacheMetrics.All,
            CacheMetrics cacheMetrics = CacheMetrics.All)
        {
            return WithMetricsImpl<MultiParamFunctionCacheConfigurationManagerSync<TK1, TK2, TV>, (TK1, TK2), TV>(
                configManager,
                functionCacheMetrics,
                cacheMetrics);
        }
        
        public static MultiParamFunctionCacheConfigurationManager<TK1, TK2, TK3, TV> WithMetrics<TK1, TK2, TK3, TV>(
            this MultiParamFunctionCacheConfigurationManager<TK1, TK2, TK3, TV> configManager,
            FunctionCacheMetrics functionCacheMetrics = FunctionCacheMetrics.All,
            CacheMetrics cacheMetrics = CacheMetrics.All)
        {
            return WithMetricsImpl<MultiParamFunctionCacheConfigurationManager<TK1, TK2, TK3, TV>, (TK1, TK2, TK3), TV>(
                configManager,
                functionCacheMetrics,
                cacheMetrics);
        }
        
        public static MultiParamFunctionCacheConfigurationManagerSync<TK1, TK2, TK3, TV> WithMetrics<TK1, TK2, TK3, TV>(
            this MultiParamFunctionCacheConfigurationManagerSync<TK1, TK2, TK3, TV> configManager,
            FunctionCacheMetrics functionCacheMetrics = FunctionCacheMetrics.All,
            CacheMetrics cacheMetrics = CacheMetrics.All)
        {
            return WithMetricsImpl<MultiParamFunctionCacheConfigurationManagerSync<TK1, TK2, TK3, TV>, (TK1, TK2, TK3), TV>(
                configManager,
                functionCacheMetrics,
                cacheMetrics);
        }
        
        public static MultiParamFunctionCacheConfigurationManager<TK1, TK2, TK3, TK4, TV> WithMetrics<TK1, TK2, TK3, TK4, TV>(
            this MultiParamFunctionCacheConfigurationManager<TK1, TK2, TK3, TK4, TV> configManager,
            FunctionCacheMetrics functionCacheMetrics = FunctionCacheMetrics.All,
            CacheMetrics cacheMetrics = CacheMetrics.All)
        {
            return WithMetricsImpl<MultiParamFunctionCacheConfigurationManager<TK1, TK2, TK3, TK4, TV>, (TK1, TK2, TK3, TK4), TV>(
                configManager,
                functionCacheMetrics,
                cacheMetrics);
        }
        
        public static MultiParamFunctionCacheConfigurationManagerSync<TK1, TK2, TK3, TK4, TV> WithMetrics<TK1, TK2, TK3, TK4, TV>(
            this MultiParamFunctionCacheConfigurationManagerSync<TK1, TK2, TK3, TK4, TV> configManager,
            FunctionCacheMetrics functionCacheMetrics = FunctionCacheMetrics.All,
            CacheMetrics cacheMetrics = CacheMetrics.All)
        {
            return WithMetricsImpl<MultiParamFunctionCacheConfigurationManagerSync<TK1, TK2, TK3, TK4, TV>, (TK1, TK2, TK3, TK4), TV>(
                configManager,
                functionCacheMetrics,
                cacheMetrics);
        }
        
        public static MultiParamEnumerableKeyFunctionCacheConfigurationManager<TKOuter, TKInnerEnumerable, TRes, TKInner, TV> WithMetrics<TKOuter, TKInnerEnumerable, TRes, TKInner, TV>(
            this MultiParamEnumerableKeyFunctionCacheConfigurationManager<TKOuter, TKInnerEnumerable, TRes, TKInner, TV> configManager,
            FunctionCacheMetrics functionCacheMetrics = FunctionCacheMetrics.All,
            CacheMetrics cacheMetrics = CacheMetrics.All)
            where TKInnerEnumerable : IEnumerable<TKInner>
            where TRes : IDictionary<TKInner, TV>
        {
            return WithMetricsImpl<MultiParamEnumerableKeyFunctionCacheConfigurationManager<TKOuter, TKInnerEnumerable, TRes, TKInner, TV>, (TKOuter, TKInner), TV>(
                configManager,
                functionCacheMetrics,
                cacheMetrics);
        }
        
        public static MultiParamEnumerableKeyFunctionCacheConfigurationManagerSync<TKOuter, TKInnerEnumerable, TRes, TKInner, TV> WithMetrics<TKOuter, TKInnerEnumerable, TRes, TKInner, TV>(
            this MultiParamEnumerableKeyFunctionCacheConfigurationManagerSync<TKOuter, TKInnerEnumerable, TRes, TKInner, TV> configManager,
            FunctionCacheMetrics functionCacheMetrics = FunctionCacheMetrics.All,
            CacheMetrics cacheMetrics = CacheMetrics.All)
            where TKInnerEnumerable : IEnumerable<TKInner>
            where TRes : IDictionary<TKInner, TV>
        {
            return WithMetricsImpl<MultiParamEnumerableKeyFunctionCacheConfigurationManagerSync<TKOuter, TKInnerEnumerable, TRes, TKInner, TV>, (TKOuter, TKInner), TV>(
                configManager,
                functionCacheMetrics,
                cacheMetrics);
        }
        
        public static MultiParamEnumerableKeyFunctionCacheConfigurationManager<TKOuter1, TKOuter2, TKInnerEnumerable, TRes, TKInner, TV> WithMetrics<TKOuter1, TKOuter2, TKInnerEnumerable, TRes, TKInner, TV>(
            this MultiParamEnumerableKeyFunctionCacheConfigurationManager<TKOuter1, TKOuter2, TKInnerEnumerable, TRes, TKInner, TV> configManager,
            FunctionCacheMetrics functionCacheMetrics = FunctionCacheMetrics.All,
            CacheMetrics cacheMetrics = CacheMetrics.All)
            where TKInnerEnumerable : IEnumerable<TKInner>
            where TRes : IDictionary<TKInner, TV>
        {
            return WithMetricsImpl<MultiParamEnumerableKeyFunctionCacheConfigurationManager<TKOuter1, TKOuter2, TKInnerEnumerable, TRes, TKInner, TV>, ((TKOuter1, TKOuter2), TKInner), TV>(
                configManager,
                functionCacheMetrics,
                cacheMetrics);
        }
        
        public static MultiParamEnumerableKeyFunctionCacheConfigurationManagerSync<TKOuter1, TKOuter2, TKInnerEnumerable, TRes, TKInner, TV> WithMetrics<TKOuter1, TKOuter2, TKInnerEnumerable, TRes, TKInner, TV>(
            this MultiParamEnumerableKeyFunctionCacheConfigurationManagerSync<TKOuter1, TKOuter2, TKInnerEnumerable, TRes, TKInner, TV> configManager,
            FunctionCacheMetrics functionCacheMetrics = FunctionCacheMetrics.All,
            CacheMetrics cacheMetrics = CacheMetrics.All)
            where TKInnerEnumerable : IEnumerable<TKInner>
            where TRes : IDictionary<TKInner, TV>
        {
            return WithMetricsImpl<MultiParamEnumerableKeyFunctionCacheConfigurationManagerSync<TKOuter1, TKOuter2, TKInnerEnumerable, TRes, TKInner, TV>, ((TKOuter1, TKOuter2), TKInner), TV>(
                configManager,
                functionCacheMetrics,
                cacheMetrics);
        }
        
        public static MultiParamEnumerableKeyFunctionCacheConfigurationManager<TKOuter1, TKOuter2, TKOuter3, TKInnerEnumerable, TRes, TKInner, TV> WithMetrics<TKOuter1, TKOuter2, TKOuter3, TKInnerEnumerable, TRes, TKInner, TV>(
            this MultiParamEnumerableKeyFunctionCacheConfigurationManager<TKOuter1, TKOuter2, TKOuter3, TKInnerEnumerable, TRes, TKInner, TV> configManager,
            FunctionCacheMetrics functionCacheMetrics = FunctionCacheMetrics.All,
            CacheMetrics cacheMetrics = CacheMetrics.All)
            where TKInnerEnumerable : IEnumerable<TKInner>
            where TRes : IDictionary<TKInner, TV>
        {
            return WithMetricsImpl<MultiParamEnumerableKeyFunctionCacheConfigurationManager<TKOuter1, TKOuter2, TKOuter3, TKInnerEnumerable, TRes, TKInner, TV>, ((TKOuter1, TKOuter2, TKOuter3), TKInner), TV>(
                configManager,
                functionCacheMetrics,
                cacheMetrics);
        }
        
        public static MultiParamEnumerableKeyFunctionCacheConfigurationManagerSync<TKOuter1, TKOuter2, TKOuter3, TKInnerEnumerable, TRes, TKInner, TV> WithMetrics<TKOuter1, TKOuter2, TKOuter3, TKInnerEnumerable, TRes, TKInner, TV>(
            this MultiParamEnumerableKeyFunctionCacheConfigurationManagerSync<TKOuter1, TKOuter2, TKOuter3, TKInnerEnumerable, TRes, TKInner, TV> configManager,
            FunctionCacheMetrics functionCacheMetrics = FunctionCacheMetrics.All,
            CacheMetrics cacheMetrics = CacheMetrics.All)
            where TKInnerEnumerable : IEnumerable<TKInner>
            where TRes : IDictionary<TKInner, TV>
        {
            return WithMetricsImpl<MultiParamEnumerableKeyFunctionCacheConfigurationManagerSync<TKOuter1, TKOuter2, TKOuter3, TKInnerEnumerable, TRes, TKInner, TV>, ((TKOuter1, TKOuter2, TKOuter3), TKInner), TV>(
                configManager,
                functionCacheMetrics,
                cacheMetrics);
        }
        
        private static TConfig WithMetricsImpl<TConfig, TK, TV>(
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