using System.Collections.Generic;
using CacheMeIfYouCan.Configuration;
using CacheMeIfYouCan.Prometheus.MetricTrackers;

namespace CacheMeIfYouCan.Prometheus
{
    public static class FunctionCacheConfigurationManagerPrometheusExtensions
    {
        public static SingleKeyFunctionCacheConfigurationManagerNoCanx<TK, TV> WithMetrics<TK, TV>(
            this SingleKeyFunctionCacheConfigurationManagerNoCanx<TK, TV> configManager,
            FunctionCacheMetrics functionCacheMetrics = FunctionCacheMetrics.All,
            CacheMetrics cacheMetrics = CacheMetrics.All)
        {
            return WithMetricsImpl<SingleKeyFunctionCacheConfigurationManagerNoCanx<TK, TV>, TK, TV>(
                configManager,
                functionCacheMetrics,
                cacheMetrics);
        }
        
        public static SingleKeyFunctionCacheConfigurationManagerCanx<TK, TV> WithMetrics<TK, TV>(
            this SingleKeyFunctionCacheConfigurationManagerCanx<TK, TV> configManager,
            FunctionCacheMetrics functionCacheMetrics = FunctionCacheMetrics.All,
            CacheMetrics cacheMetrics = CacheMetrics.All)
        {
            return WithMetricsImpl<SingleKeyFunctionCacheConfigurationManagerCanx<TK, TV>, TK, TV>(
                configManager,
                functionCacheMetrics,
                cacheMetrics);
        }
        
        public static SingleKeyFunctionCacheConfigurationManagerSyncNoCanx<TK, TV> WithMetrics<TK, TV>(
            this SingleKeyFunctionCacheConfigurationManagerSyncNoCanx<TK, TV> configManager,
            FunctionCacheMetrics functionCacheMetrics = FunctionCacheMetrics.All,
            CacheMetrics cacheMetrics = CacheMetrics.All)
        {
            return WithMetricsImpl<SingleKeyFunctionCacheConfigurationManagerSyncNoCanx<TK, TV>, TK, TV>(
                configManager,
                functionCacheMetrics,
                cacheMetrics);
        }
        
        public static SingleKeyFunctionCacheConfigurationManagerSyncCanx<TK, TV> WithMetrics<TK, TV>(
            this SingleKeyFunctionCacheConfigurationManagerSyncCanx<TK, TV> configManager,
            FunctionCacheMetrics functionCacheMetrics = FunctionCacheMetrics.All,
            CacheMetrics cacheMetrics = CacheMetrics.All)
        {
            return WithMetricsImpl<SingleKeyFunctionCacheConfigurationManagerSyncCanx<TK, TV>, TK, TV>(
                configManager,
                functionCacheMetrics,
                cacheMetrics);
        }
        
        public static EnumerableKeyFunctionCacheConfigurationManagerNoCanx<TReq, TRes, TK, TV> WithMetrics<TReq, TRes, TK, TV>(
            this EnumerableKeyFunctionCacheConfigurationManagerNoCanx<TReq, TRes, TK, TV> configManager,
            FunctionCacheMetrics functionCacheMetrics = FunctionCacheMetrics.All,
            CacheMetrics cacheMetrics = CacheMetrics.All)
            where TReq : IEnumerable<TK>
            where TRes : IDictionary<TK, TV>
        {
            return WithMetricsImpl<EnumerableKeyFunctionCacheConfigurationManagerNoCanx<TReq, TRes, TK, TV>, TK, TV>(
                configManager,
                functionCacheMetrics,
                cacheMetrics);
        }
        
        public static EnumerableKeyFunctionCacheConfigurationManagerCanx<TReq, TRes, TK, TV> WithMetrics<TReq, TRes, TK, TV>(
            this EnumerableKeyFunctionCacheConfigurationManagerCanx<TReq, TRes, TK, TV> configManager,
            FunctionCacheMetrics functionCacheMetrics = FunctionCacheMetrics.All,
            CacheMetrics cacheMetrics = CacheMetrics.All)
            where TReq : IEnumerable<TK>
            where TRes : IDictionary<TK, TV>
        {
            return WithMetricsImpl<EnumerableKeyFunctionCacheConfigurationManagerCanx<TReq, TRes, TK, TV>, TK, TV>(
                configManager,
                functionCacheMetrics,
                cacheMetrics);
        }
        
        public static EnumerableKeyFunctionCacheConfigurationManagerSyncNoCanx<TReq, TRes, TK, TV> WithMetrics<TReq, TRes, TK, TV>(
            this EnumerableKeyFunctionCacheConfigurationManagerSyncNoCanx<TReq, TRes, TK, TV> configManager,
            FunctionCacheMetrics functionCacheMetrics = FunctionCacheMetrics.All,
            CacheMetrics cacheMetrics = CacheMetrics.All)
            where TReq : IEnumerable<TK>
            where TRes : IDictionary<TK, TV>
        {
            return WithMetricsImpl<EnumerableKeyFunctionCacheConfigurationManagerSyncNoCanx<TReq, TRes, TK, TV>, TK, TV>(
                configManager,
                functionCacheMetrics,
                cacheMetrics);
        }
        
        public static EnumerableKeyFunctionCacheConfigurationManagerSyncCanx<TReq, TRes, TK, TV> WithMetrics<TReq, TRes, TK, TV>(
            this EnumerableKeyFunctionCacheConfigurationManagerSyncCanx<TReq, TRes, TK, TV> configManager,
            FunctionCacheMetrics functionCacheMetrics = FunctionCacheMetrics.All,
            CacheMetrics cacheMetrics = CacheMetrics.All)
            where TReq : IEnumerable<TK>
            where TRes : IDictionary<TK, TV>
        {
            return WithMetricsImpl<EnumerableKeyFunctionCacheConfigurationManagerSyncCanx<TReq, TRes, TK, TV>, TK, TV>(
                configManager,
                functionCacheMetrics,
                cacheMetrics);
        }
        
        public static MultiParamFunctionCacheConfigurationManagerNoCanx<TK1, TK2, TV> WithMetrics<TK1, TK2, TV>(
            this MultiParamFunctionCacheConfigurationManagerNoCanx<TK1, TK2, TV> configManager,
            FunctionCacheMetrics functionCacheMetrics = FunctionCacheMetrics.All,
            CacheMetrics cacheMetrics = CacheMetrics.All)
        {
            return WithMetricsImpl<MultiParamFunctionCacheConfigurationManagerNoCanx<TK1, TK2, TV>, (TK1, TK2), TV>(
                configManager,
                functionCacheMetrics,
                cacheMetrics);
        }
        
        public static MultiParamFunctionCacheConfigurationManagerCanx<TK1, TK2, TV> WithMetrics<TK1, TK2, TV>(
            this MultiParamFunctionCacheConfigurationManagerCanx<TK1, TK2, TV> configManager,
            FunctionCacheMetrics functionCacheMetrics = FunctionCacheMetrics.All,
            CacheMetrics cacheMetrics = CacheMetrics.All)
        {
            return WithMetricsImpl<MultiParamFunctionCacheConfigurationManagerCanx<TK1, TK2, TV>, (TK1, TK2), TV>(
                configManager,
                functionCacheMetrics,
                cacheMetrics);
        }
        
        public static MultiParamFunctionCacheConfigurationManagerSyncNoCanx<TK1, TK2, TV> WithMetrics<TK1, TK2, TV>(
            this MultiParamFunctionCacheConfigurationManagerSyncNoCanx<TK1, TK2, TV> configManager,
            FunctionCacheMetrics functionCacheMetrics = FunctionCacheMetrics.All,
            CacheMetrics cacheMetrics = CacheMetrics.All)
        {
            return WithMetricsImpl<MultiParamFunctionCacheConfigurationManagerSyncNoCanx<TK1, TK2, TV>, (TK1, TK2), TV>(
                configManager,
                functionCacheMetrics,
                cacheMetrics);
        }
        
        public static MultiParamFunctionCacheConfigurationManagerSyncCanx<TK1, TK2, TV> WithMetrics<TK1, TK2, TV>(
            this MultiParamFunctionCacheConfigurationManagerSyncCanx<TK1, TK2, TV> configManager,
            FunctionCacheMetrics functionCacheMetrics = FunctionCacheMetrics.All,
            CacheMetrics cacheMetrics = CacheMetrics.All)
        {
            return WithMetricsImpl<MultiParamFunctionCacheConfigurationManagerSyncCanx<TK1, TK2, TV>, (TK1, TK2), TV>(
                configManager,
                functionCacheMetrics,
                cacheMetrics);
        }
        
        public static MultiParamFunctionCacheConfigurationManagerNoCanx<TK1, TK2, TK3, TV> WithMetrics<TK1, TK2, TK3, TV>(
            this MultiParamFunctionCacheConfigurationManagerNoCanx<TK1, TK2, TK3, TV> configManager,
            FunctionCacheMetrics functionCacheMetrics = FunctionCacheMetrics.All,
            CacheMetrics cacheMetrics = CacheMetrics.All)
        {
            return WithMetricsImpl<MultiParamFunctionCacheConfigurationManagerNoCanx<TK1, TK2, TK3, TV>, (TK1, TK2, TK3), TV>(
                configManager,
                functionCacheMetrics,
                cacheMetrics);
        }
        
        public static MultiParamFunctionCacheConfigurationManagerCanx<TK1, TK2, TK3, TV> WithMetrics<TK1, TK2, TK3, TV>(
            this MultiParamFunctionCacheConfigurationManagerCanx<TK1, TK2, TK3, TV> configManager,
            FunctionCacheMetrics functionCacheMetrics = FunctionCacheMetrics.All,
            CacheMetrics cacheMetrics = CacheMetrics.All)
        {
            return WithMetricsImpl<MultiParamFunctionCacheConfigurationManagerCanx<TK1, TK2, TK3, TV>, (TK1, TK2, TK3), TV>(
                configManager,
                functionCacheMetrics,
                cacheMetrics);
        }
        
        public static MultiParamFunctionCacheConfigurationManagerSyncNoCanx<TK1, TK2, TK3, TV> WithMetrics<TK1, TK2, TK3, TV>(
            this MultiParamFunctionCacheConfigurationManagerSyncNoCanx<TK1, TK2, TK3, TV> configManager,
            FunctionCacheMetrics functionCacheMetrics = FunctionCacheMetrics.All,
            CacheMetrics cacheMetrics = CacheMetrics.All)
        {
            return WithMetricsImpl<MultiParamFunctionCacheConfigurationManagerSyncNoCanx<TK1, TK2, TK3, TV>, (TK1, TK2, TK3), TV>(
                configManager,
                functionCacheMetrics,
                cacheMetrics);
        }
        
        public static MultiParamFunctionCacheConfigurationManagerSyncCanx<TK1, TK2, TK3, TV> WithMetrics<TK1, TK2, TK3, TV>(
            this MultiParamFunctionCacheConfigurationManagerSyncCanx<TK1, TK2, TK3, TV> configManager,
            FunctionCacheMetrics functionCacheMetrics = FunctionCacheMetrics.All,
            CacheMetrics cacheMetrics = CacheMetrics.All)
        {
            return WithMetricsImpl<MultiParamFunctionCacheConfigurationManagerSyncCanx<TK1, TK2, TK3, TV>, (TK1, TK2, TK3), TV>(
                configManager,
                functionCacheMetrics,
                cacheMetrics);
        }
        
        public static MultiParamFunctionCacheConfigurationManagerNoCanx<TK1, TK2, TK3, TK4, TV> WithMetrics<TK1, TK2, TK3, TK4, TV>(
            this MultiParamFunctionCacheConfigurationManagerNoCanx<TK1, TK2, TK3, TK4, TV> configManager,
            FunctionCacheMetrics functionCacheMetrics = FunctionCacheMetrics.All,
            CacheMetrics cacheMetrics = CacheMetrics.All)
        {
            return WithMetricsImpl<MultiParamFunctionCacheConfigurationManagerNoCanx<TK1, TK2, TK3, TK4, TV>, (TK1, TK2, TK3, TK4), TV>(
                configManager,
                functionCacheMetrics,
                cacheMetrics);
        }
        
        public static MultiParamFunctionCacheConfigurationManagerCanx<TK1, TK2, TK3, TK4, TV> WithMetrics<TK1, TK2, TK3, TK4, TV>(
            this MultiParamFunctionCacheConfigurationManagerCanx<TK1, TK2, TK3, TK4, TV> configManager,
            FunctionCacheMetrics functionCacheMetrics = FunctionCacheMetrics.All,
            CacheMetrics cacheMetrics = CacheMetrics.All)
        {
            return WithMetricsImpl<MultiParamFunctionCacheConfigurationManagerCanx<TK1, TK2, TK3, TK4, TV>, (TK1, TK2, TK3, TK4), TV>(
                configManager,
                functionCacheMetrics,
                cacheMetrics);
        }
        
        public static MultiParamFunctionCacheConfigurationManagerSyncNoCanx<TK1, TK2, TK3, TK4, TV> WithMetrics<TK1, TK2, TK3, TK4, TV>(
            this MultiParamFunctionCacheConfigurationManagerSyncNoCanx<TK1, TK2, TK3, TK4, TV> configManager,
            FunctionCacheMetrics functionCacheMetrics = FunctionCacheMetrics.All,
            CacheMetrics cacheMetrics = CacheMetrics.All)
        {
            return WithMetricsImpl<MultiParamFunctionCacheConfigurationManagerSyncNoCanx<TK1, TK2, TK3, TK4, TV>, (TK1, TK2, TK3, TK4), TV>(
                configManager,
                functionCacheMetrics,
                cacheMetrics);
        }
        
        public static MultiParamFunctionCacheConfigurationManagerSyncCanx<TK1, TK2, TK3, TK4, TV> WithMetrics<TK1, TK2, TK3, TK4, TV>(
            this MultiParamFunctionCacheConfigurationManagerSyncCanx<TK1, TK2, TK3, TK4, TV> configManager,
            FunctionCacheMetrics functionCacheMetrics = FunctionCacheMetrics.All,
            CacheMetrics cacheMetrics = CacheMetrics.All)
        {
            return WithMetricsImpl<MultiParamFunctionCacheConfigurationManagerSyncCanx<TK1, TK2, TK3, TK4, TV>, (TK1, TK2, TK3, TK4), TV>(
                configManager,
                functionCacheMetrics,
                cacheMetrics);
        }
        
        public static MultiParamEnumerableKeyFunctionCacheConfigurationManagerNoCanx<TKOuter, TKInnerEnumerable, TRes, TKInner, TV> WithMetrics<TKOuter, TKInnerEnumerable, TRes, TKInner, TV>(
            this MultiParamEnumerableKeyFunctionCacheConfigurationManagerNoCanx<TKOuter, TKInnerEnumerable, TRes, TKInner, TV> configManager,
            FunctionCacheMetrics functionCacheMetrics = FunctionCacheMetrics.All,
            CacheMetrics cacheMetrics = CacheMetrics.All)
            where TKInnerEnumerable : IEnumerable<TKInner>
            where TRes : IDictionary<TKInner, TV>
        {
            return WithMetricsImpl<MultiParamEnumerableKeyFunctionCacheConfigurationManagerNoCanx<TKOuter, TKInnerEnumerable, TRes, TKInner, TV>, (TKOuter, TKInner), TV>(
                configManager,
                functionCacheMetrics,
                cacheMetrics);
        }
        
        public static MultiParamEnumerableKeyFunctionCacheConfigurationManagerCanx<TKOuter, TKInnerEnumerable, TRes, TKInner, TV> WithMetrics<TKOuter, TKInnerEnumerable, TRes, TKInner, TV>(
            this MultiParamEnumerableKeyFunctionCacheConfigurationManagerCanx<TKOuter, TKInnerEnumerable, TRes, TKInner, TV> configManager,
            FunctionCacheMetrics functionCacheMetrics = FunctionCacheMetrics.All,
            CacheMetrics cacheMetrics = CacheMetrics.All)
            where TKInnerEnumerable : IEnumerable<TKInner>
            where TRes : IDictionary<TKInner, TV>
        {
            return WithMetricsImpl<MultiParamEnumerableKeyFunctionCacheConfigurationManagerCanx<TKOuter, TKInnerEnumerable, TRes, TKInner, TV>, (TKOuter, TKInner), TV>(
                configManager,
                functionCacheMetrics,
                cacheMetrics);
        }
        
        public static MultiParamEnumerableKeyFunctionCacheConfigurationManagerSyncNoCanx<TKOuter, TKInnerEnumerable, TRes, TKInner, TV> WithMetrics<TKOuter, TKInnerEnumerable, TRes, TKInner, TV>(
            this MultiParamEnumerableKeyFunctionCacheConfigurationManagerSyncNoCanx<TKOuter, TKInnerEnumerable, TRes, TKInner, TV> configManager,
            FunctionCacheMetrics functionCacheMetrics = FunctionCacheMetrics.All,
            CacheMetrics cacheMetrics = CacheMetrics.All)
            where TKInnerEnumerable : IEnumerable<TKInner>
            where TRes : IDictionary<TKInner, TV>
        {
            return WithMetricsImpl<MultiParamEnumerableKeyFunctionCacheConfigurationManagerSyncNoCanx<TKOuter, TKInnerEnumerable, TRes, TKInner, TV>, (TKOuter, TKInner), TV>(
                configManager,
                functionCacheMetrics,
                cacheMetrics);
        }
        
        public static MultiParamEnumerableKeyFunctionCacheConfigurationManagerSyncCanx<TKOuter, TKInnerEnumerable, TRes, TKInner, TV> WithMetrics<TKOuter, TKInnerEnumerable, TRes, TKInner, TV>(
            this MultiParamEnumerableKeyFunctionCacheConfigurationManagerSyncCanx<TKOuter, TKInnerEnumerable, TRes, TKInner, TV> configManager,
            FunctionCacheMetrics functionCacheMetrics = FunctionCacheMetrics.All,
            CacheMetrics cacheMetrics = CacheMetrics.All)
            where TKInnerEnumerable : IEnumerable<TKInner>
            where TRes : IDictionary<TKInner, TV>
        {
            return WithMetricsImpl<MultiParamEnumerableKeyFunctionCacheConfigurationManagerSyncCanx<TKOuter, TKInnerEnumerable, TRes, TKInner, TV>, (TKOuter, TKInner), TV>(
                configManager,
                functionCacheMetrics,
                cacheMetrics);
        }
        
        public static MultiParamEnumerableKeyFunctionCacheConfigurationManagerNoCanx<TKOuter1, TKOuter2, TKInnerEnumerable, TRes, TKInner, TV> WithMetrics<TKOuter1, TKOuter2, TKInnerEnumerable, TRes, TKInner, TV>(
            this MultiParamEnumerableKeyFunctionCacheConfigurationManagerNoCanx<TKOuter1, TKOuter2, TKInnerEnumerable, TRes, TKInner, TV> configManager,
            FunctionCacheMetrics functionCacheMetrics = FunctionCacheMetrics.All,
            CacheMetrics cacheMetrics = CacheMetrics.All)
            where TKInnerEnumerable : IEnumerable<TKInner>
            where TRes : IDictionary<TKInner, TV>
        {
            return WithMetricsImpl<MultiParamEnumerableKeyFunctionCacheConfigurationManagerNoCanx<TKOuter1, TKOuter2, TKInnerEnumerable, TRes, TKInner, TV>, ((TKOuter1, TKOuter2), TKInner), TV>(
                configManager,
                functionCacheMetrics,
                cacheMetrics);
        }
        
        public static MultiParamEnumerableKeyFunctionCacheConfigurationManagerCanx<TKOuter1, TKOuter2, TKInnerEnumerable, TRes, TKInner, TV> WithMetrics<TKOuter1, TKOuter2, TKInnerEnumerable, TRes, TKInner, TV>(
            this MultiParamEnumerableKeyFunctionCacheConfigurationManagerCanx<TKOuter1, TKOuter2, TKInnerEnumerable, TRes, TKInner, TV> configManager,
            FunctionCacheMetrics functionCacheMetrics = FunctionCacheMetrics.All,
            CacheMetrics cacheMetrics = CacheMetrics.All)
            where TKInnerEnumerable : IEnumerable<TKInner>
            where TRes : IDictionary<TKInner, TV>
        {
            return WithMetricsImpl<MultiParamEnumerableKeyFunctionCacheConfigurationManagerCanx<TKOuter1, TKOuter2, TKInnerEnumerable, TRes, TKInner, TV>, ((TKOuter1, TKOuter2), TKInner), TV>(
                configManager,
                functionCacheMetrics,
                cacheMetrics);
        }
        
        public static MultiParamEnumerableKeyFunctionCacheConfigurationManagerSyncNoCanx<TKOuter1, TKOuter2, TKInnerEnumerable, TRes, TKInner, TV> WithMetrics<TKOuter1, TKOuter2, TKInnerEnumerable, TRes, TKInner, TV>(
            this MultiParamEnumerableKeyFunctionCacheConfigurationManagerSyncNoCanx<TKOuter1, TKOuter2, TKInnerEnumerable, TRes, TKInner, TV> configManager,
            FunctionCacheMetrics functionCacheMetrics = FunctionCacheMetrics.All,
            CacheMetrics cacheMetrics = CacheMetrics.All)
            where TKInnerEnumerable : IEnumerable<TKInner>
            where TRes : IDictionary<TKInner, TV>
        {
            return WithMetricsImpl<MultiParamEnumerableKeyFunctionCacheConfigurationManagerSyncNoCanx<TKOuter1, TKOuter2, TKInnerEnumerable, TRes, TKInner, TV>, ((TKOuter1, TKOuter2), TKInner), TV>(
                configManager,
                functionCacheMetrics,
                cacheMetrics);
        }
        
        public static MultiParamEnumerableKeyFunctionCacheConfigurationManagerSyncCanx<TKOuter1, TKOuter2, TKInnerEnumerable, TRes, TKInner, TV> WithMetrics<TKOuter1, TKOuter2, TKInnerEnumerable, TRes, TKInner, TV>(
            this MultiParamEnumerableKeyFunctionCacheConfigurationManagerSyncCanx<TKOuter1, TKOuter2, TKInnerEnumerable, TRes, TKInner, TV> configManager,
            FunctionCacheMetrics functionCacheMetrics = FunctionCacheMetrics.All,
            CacheMetrics cacheMetrics = CacheMetrics.All)
            where TKInnerEnumerable : IEnumerable<TKInner>
            where TRes : IDictionary<TKInner, TV>
        {
            return WithMetricsImpl<MultiParamEnumerableKeyFunctionCacheConfigurationManagerSyncCanx<TKOuter1, TKOuter2, TKInnerEnumerable, TRes, TKInner, TV>, ((TKOuter1, TKOuter2), TKInner), TV>(
                configManager,
                functionCacheMetrics,
                cacheMetrics);
        }
        
        public static MultiParamEnumerableKeyFunctionCacheConfigurationManagerNoCanx<TKOuter1, TKOuter2, TKOuter3, TKInnerEnumerable, TRes, TKInner, TV> WithMetrics<TKOuter1, TKOuter2, TKOuter3, TKInnerEnumerable, TRes, TKInner, TV>(
            this MultiParamEnumerableKeyFunctionCacheConfigurationManagerNoCanx<TKOuter1, TKOuter2, TKOuter3, TKInnerEnumerable, TRes, TKInner, TV> configManager,
            FunctionCacheMetrics functionCacheMetrics = FunctionCacheMetrics.All,
            CacheMetrics cacheMetrics = CacheMetrics.All)
            where TKInnerEnumerable : IEnumerable<TKInner>
            where TRes : IDictionary<TKInner, TV>
        {
            return WithMetricsImpl<MultiParamEnumerableKeyFunctionCacheConfigurationManagerNoCanx<TKOuter1, TKOuter2, TKOuter3, TKInnerEnumerable, TRes, TKInner, TV>, ((TKOuter1, TKOuter2, TKOuter3), TKInner), TV>(
                configManager,
                functionCacheMetrics,
                cacheMetrics);
        }
        
        public static MultiParamEnumerableKeyFunctionCacheConfigurationManagerCanx<TKOuter1, TKOuter2, TKOuter3, TKInnerEnumerable, TRes, TKInner, TV> WithMetrics<TKOuter1, TKOuter2, TKOuter3, TKInnerEnumerable, TRes, TKInner, TV>(
            this MultiParamEnumerableKeyFunctionCacheConfigurationManagerCanx<TKOuter1, TKOuter2, TKOuter3, TKInnerEnumerable, TRes, TKInner, TV> configManager,
            FunctionCacheMetrics functionCacheMetrics = FunctionCacheMetrics.All,
            CacheMetrics cacheMetrics = CacheMetrics.All)
            where TKInnerEnumerable : IEnumerable<TKInner>
            where TRes : IDictionary<TKInner, TV>
        {
            return WithMetricsImpl<MultiParamEnumerableKeyFunctionCacheConfigurationManagerCanx<TKOuter1, TKOuter2, TKOuter3, TKInnerEnumerable, TRes, TKInner, TV>, ((TKOuter1, TKOuter2, TKOuter3), TKInner), TV>(
                configManager,
                functionCacheMetrics,
                cacheMetrics);
        }
        
        public static MultiParamEnumerableKeyFunctionCacheConfigurationManagerSyncNoCanx<TKOuter1, TKOuter2, TKOuter3, TKInnerEnumerable, TRes, TKInner, TV> WithMetrics<TKOuter1, TKOuter2, TKOuter3, TKInnerEnumerable, TRes, TKInner, TV>(
            this MultiParamEnumerableKeyFunctionCacheConfigurationManagerSyncNoCanx<TKOuter1, TKOuter2, TKOuter3, TKInnerEnumerable, TRes, TKInner, TV> configManager,
            FunctionCacheMetrics functionCacheMetrics = FunctionCacheMetrics.All,
            CacheMetrics cacheMetrics = CacheMetrics.All)
            where TKInnerEnumerable : IEnumerable<TKInner>
            where TRes : IDictionary<TKInner, TV>
        {
            return WithMetricsImpl<MultiParamEnumerableKeyFunctionCacheConfigurationManagerSyncNoCanx<TKOuter1, TKOuter2, TKOuter3, TKInnerEnumerable, TRes, TKInner, TV>, ((TKOuter1, TKOuter2, TKOuter3), TKInner), TV>(
                configManager,
                functionCacheMetrics,
                cacheMetrics);
        }
        
        public static MultiParamEnumerableKeyFunctionCacheConfigurationManagerSyncCanx<TKOuter1, TKOuter2, TKOuter3, TKInnerEnumerable, TRes, TKInner, TV> WithMetrics<TKOuter1, TKOuter2, TKOuter3, TKInnerEnumerable, TRes, TKInner, TV>(
            this MultiParamEnumerableKeyFunctionCacheConfigurationManagerSyncCanx<TKOuter1, TKOuter2, TKOuter3, TKInnerEnumerable, TRes, TKInner, TV> configManager,
            FunctionCacheMetrics functionCacheMetrics = FunctionCacheMetrics.All,
            CacheMetrics cacheMetrics = CacheMetrics.All)
            where TKInnerEnumerable : IEnumerable<TKInner>
            where TRes : IDictionary<TKInner, TV>
        {
            return WithMetricsImpl<MultiParamEnumerableKeyFunctionCacheConfigurationManagerSyncCanx<TKOuter1, TKOuter2, TKOuter3, TKInnerEnumerable, TRes, TKInner, TV>, ((TKOuter1, TKOuter2, TKOuter3), TKInner), TV>(
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
            
            if (cacheMetrics.HasFlag(CacheMetrics.Remove))
                configManager.OnCacheRemove(Cache_Remove.OnCacheRemove);

            if (cacheMetrics.HasFlag(CacheMetrics.Exception))
                configManager.OnCacheException(Cache_Exception.OnCacheException);
            
            return configManager;
        }
    }
}