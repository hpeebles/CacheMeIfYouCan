using System;
using System.Collections.Generic;
using CacheMeIfYouCan.Configuration;

namespace CacheMeIfYouCan.Redis
{
    public static class FunctionCacheConfigurationManagerRedisExtensions
    {
        public static SingleKeyFunctionCacheConfigurationManagerNoCanx<TK, TV> WithRedis<TK, TV>(
            this SingleKeyFunctionCacheConfigurationManagerNoCanx<TK, TV> configManager,
            Action<RedisCacheFactoryConfig> redisConfigAction,
            Func<IDistributedCacheFactory, IDistributedCacheFactory> cacheConfigAction = null)
        {
            return WithRedisImpl<SingleKeyFunctionCacheConfigurationManagerNoCanx<TK, TV>, TK, TV>(
                configManager, redisConfigAction, cacheConfigAction);
        }
        
        public static SingleKeyFunctionCacheConfigurationManagerCanx<TK, TV> WithRedis<TK, TV>(
            this SingleKeyFunctionCacheConfigurationManagerCanx<TK, TV> configManager,
            Action<RedisCacheFactoryConfig> redisConfigAction,
            Func<IDistributedCacheFactory, IDistributedCacheFactory> cacheConfigAction = null)
        {
            return WithRedisImpl<SingleKeyFunctionCacheConfigurationManagerCanx<TK, TV>, TK, TV>(
                configManager, redisConfigAction, cacheConfigAction);
        }
        
        public static SingleKeyFunctionCacheConfigurationManagerSyncNoCanx<TK, TV> WithRedis<TK, TV>(
            this SingleKeyFunctionCacheConfigurationManagerSyncNoCanx<TK, TV> configManager,
            Action<RedisCacheFactoryConfig> redisConfigAction,
            Func<IDistributedCacheFactory, IDistributedCacheFactory> cacheConfigAction = null)
        {
            return WithRedisImpl<SingleKeyFunctionCacheConfigurationManagerSyncNoCanx<TK, TV>, TK, TV>(
                configManager, redisConfigAction, cacheConfigAction);
        }
        
        public static SingleKeyFunctionCacheConfigurationManagerSyncCanx<TK, TV> WithRedis<TK, TV>(
            this SingleKeyFunctionCacheConfigurationManagerSyncCanx<TK, TV> configManager,
            Action<RedisCacheFactoryConfig> redisConfigAction,
            Func<IDistributedCacheFactory, IDistributedCacheFactory> cacheConfigAction = null)
        {
            return WithRedisImpl<SingleKeyFunctionCacheConfigurationManagerSyncCanx<TK, TV>, TK, TV>(
                configManager, redisConfigAction, cacheConfigAction);
        }
        
        public static EnumerableKeyFunctionCacheConfigurationManagerNoCanx<TReq, TRes, TK, TV> WithRedis<TReq, TRes, TK, TV>(
            this EnumerableKeyFunctionCacheConfigurationManagerNoCanx<TReq, TRes, TK, TV> configManager,
            Action<RedisCacheFactoryConfig> redisConfigAction,
            Func<IDistributedCacheFactory, IDistributedCacheFactory> cacheConfigAction = null)
            where TReq : IEnumerable<TK>
            where TRes : IDictionary<TK, TV>
        {
            return WithRedisImpl<EnumerableKeyFunctionCacheConfigurationManagerNoCanx<TReq, TRes, TK, TV>, TK, TV>(
                configManager, redisConfigAction, cacheConfigAction);
        }
        
        public static EnumerableKeyFunctionCacheConfigurationManagerCanx<TReq, TRes, TK, TV> WithRedis<TReq, TRes, TK, TV>(
            this EnumerableKeyFunctionCacheConfigurationManagerCanx<TReq, TRes, TK, TV> configManager,
            Action<RedisCacheFactoryConfig> redisConfigAction,
            Func<IDistributedCacheFactory, IDistributedCacheFactory> cacheConfigAction = null)
            where TReq : IEnumerable<TK>
            where TRes : IDictionary<TK, TV>
        {
            return WithRedisImpl<EnumerableKeyFunctionCacheConfigurationManagerCanx<TReq, TRes, TK, TV>, TK, TV>(
                configManager, redisConfigAction, cacheConfigAction);
        }
        
        public static EnumerableKeyFunctionCacheConfigurationManagerSyncNoCanx<TReq, TRes, TK, TV> WithRedis<TReq, TRes, TK, TV>(
            this EnumerableKeyFunctionCacheConfigurationManagerSyncNoCanx<TReq, TRes, TK, TV> configManager,
            Action<RedisCacheFactoryConfig> redisConfigAction,
            Func<IDistributedCacheFactory, IDistributedCacheFactory> cacheConfigAction = null)
            where TReq : IEnumerable<TK>
            where TRes : IDictionary<TK, TV>
        {
            return WithRedisImpl<EnumerableKeyFunctionCacheConfigurationManagerSyncNoCanx<TReq, TRes, TK, TV>, TK, TV>(
                configManager, redisConfigAction, cacheConfigAction);
        }
        
        public static EnumerableKeyFunctionCacheConfigurationManagerSyncCanx<TReq, TRes, TK, TV> WithRedis<TReq, TRes, TK, TV>(
            this EnumerableKeyFunctionCacheConfigurationManagerSyncCanx<TReq, TRes, TK, TV> configManager,
            Action<RedisCacheFactoryConfig> redisConfigAction,
            Func<IDistributedCacheFactory, IDistributedCacheFactory> cacheConfigAction = null)
            where TReq : IEnumerable<TK>
            where TRes : IDictionary<TK, TV>
        {
            return WithRedisImpl<EnumerableKeyFunctionCacheConfigurationManagerSyncCanx<TReq, TRes, TK, TV>, TK, TV>(
                configManager, redisConfigAction, cacheConfigAction);
        }
        
        public static MultiParamFunctionCacheConfigurationManagerNoCanx<TK1, TK2, TV> WithRedis<TK1, TK2, TV>(
            this MultiParamFunctionCacheConfigurationManagerNoCanx<TK1, TK2, TV> configManager,
            Action<RedisCacheFactoryConfig> redisConfigAction,
            Func<IDistributedCacheFactory, IDistributedCacheFactory> cacheConfigAction = null)
        {
            return WithRedisImpl<MultiParamFunctionCacheConfigurationManagerNoCanx<TK1, TK2, TV>, (TK1, TK2), TV>(
                configManager, redisConfigAction, cacheConfigAction);
        }
        
        public static MultiParamFunctionCacheConfigurationManagerCanx<TK1, TK2, TV> WithRedis<TK1, TK2, TV>(
            this MultiParamFunctionCacheConfigurationManagerCanx<TK1, TK2, TV> configManager,
            Action<RedisCacheFactoryConfig> redisConfigAction,
            Func<IDistributedCacheFactory, IDistributedCacheFactory> cacheConfigAction = null)
        {
            return WithRedisImpl<MultiParamFunctionCacheConfigurationManagerCanx<TK1, TK2, TV>, (TK1, TK2), TV>(
                configManager, redisConfigAction, cacheConfigAction);
        }
        
        public static MultiParamFunctionCacheConfigurationManagerSyncNoCanx<TK1, TK2, TV> WithRedis<TK1, TK2, TV>(
            this MultiParamFunctionCacheConfigurationManagerSyncNoCanx<TK1, TK2, TV> configManager,
            Action<RedisCacheFactoryConfig> redisConfigAction,
            Func<IDistributedCacheFactory, IDistributedCacheFactory> cacheConfigAction = null)
        {
            return WithRedisImpl<MultiParamFunctionCacheConfigurationManagerSyncNoCanx<TK1, TK2, TV>, (TK1, TK2), TV>(
                configManager, redisConfigAction, cacheConfigAction);
        }
        
        public static MultiParamFunctionCacheConfigurationManagerSyncCanx<TK1, TK2, TV> WithRedis<TK1, TK2, TV>(
            this MultiParamFunctionCacheConfigurationManagerSyncCanx<TK1, TK2, TV> configManager,
            Action<RedisCacheFactoryConfig> redisConfigAction,
            Func<IDistributedCacheFactory, IDistributedCacheFactory> cacheConfigAction = null)
        {
            return WithRedisImpl<MultiParamFunctionCacheConfigurationManagerSyncCanx<TK1, TK2, TV>, (TK1, TK2), TV>(
                configManager, redisConfigAction, cacheConfigAction);
        }
        
        public static MultiParamFunctionCacheConfigurationManagerNoCanx<TK1, TK2, TK3, TV> WithRedis<TK1, TK2, TK3, TV>(
            this MultiParamFunctionCacheConfigurationManagerNoCanx<TK1, TK2, TK3, TV> configManager,
            Action<RedisCacheFactoryConfig> redisConfigAction,
            Func<IDistributedCacheFactory, IDistributedCacheFactory> cacheConfigAction = null)
        {
            return WithRedisImpl<MultiParamFunctionCacheConfigurationManagerNoCanx<TK1, TK2, TK3, TV>, (TK1, TK2, TK3), TV>(
                configManager, redisConfigAction, cacheConfigAction);
        }
        
        public static MultiParamFunctionCacheConfigurationManagerCanx<TK1, TK2, TK3, TV> WithRedis<TK1, TK2, TK3, TV>(
            this MultiParamFunctionCacheConfigurationManagerCanx<TK1, TK2, TK3, TV> configManager,
            Action<RedisCacheFactoryConfig> redisConfigAction,
            Func<IDistributedCacheFactory, IDistributedCacheFactory> cacheConfigAction = null)
        {
            return WithRedisImpl<MultiParamFunctionCacheConfigurationManagerCanx<TK1, TK2, TK3, TV>, (TK1, TK2, TK3), TV>(
                configManager, redisConfigAction, cacheConfigAction);
        }
        
        public static MultiParamFunctionCacheConfigurationManagerSyncNoCanx<TK1, TK2, TK3, TV> WithRedis<TK1, TK2, TK3, TV>(
            this MultiParamFunctionCacheConfigurationManagerSyncNoCanx<TK1, TK2, TK3, TV> configManager,
            Action<RedisCacheFactoryConfig> redisConfigAction,
            Func<IDistributedCacheFactory, IDistributedCacheFactory> cacheConfigAction = null)
        {
            return WithRedisImpl<MultiParamFunctionCacheConfigurationManagerSyncNoCanx<TK1, TK2, TK3, TV>, (TK1, TK2, TK3), TV>(
                configManager, redisConfigAction, cacheConfigAction);
        }
        
        public static MultiParamFunctionCacheConfigurationManagerSyncCanx<TK1, TK2, TK3, TV> WithRedis<TK1, TK2, TK3, TV>(
            this MultiParamFunctionCacheConfigurationManagerSyncCanx<TK1, TK2, TK3, TV> configManager,
            Action<RedisCacheFactoryConfig> redisConfigAction,
            Func<IDistributedCacheFactory, IDistributedCacheFactory> cacheConfigAction = null)
        {
            return WithRedisImpl<MultiParamFunctionCacheConfigurationManagerSyncCanx<TK1, TK2, TK3, TV>, (TK1, TK2, TK3), TV>(
                configManager, redisConfigAction, cacheConfigAction);
        }
        
        public static MultiParamFunctionCacheConfigurationManagerNoCanx<TK1, TK2, TK3, TK4, TV> WithRedis<TK1, TK2, TK3, TK4, TV>(
            this MultiParamFunctionCacheConfigurationManagerNoCanx<TK1, TK2, TK3, TK4, TV> configManager,
            Action<RedisCacheFactoryConfig> redisConfigAction,
            Func<IDistributedCacheFactory, IDistributedCacheFactory> cacheConfigAction = null)
        {
            return WithRedisImpl<MultiParamFunctionCacheConfigurationManagerNoCanx<TK1, TK2, TK3, TK4, TV>, (TK1, TK2, TK3, TK4), TV>(
                configManager, redisConfigAction, cacheConfigAction);
        }
        
        public static MultiParamFunctionCacheConfigurationManagerCanx<TK1, TK2, TK3, TK4, TV> WithRedis<TK1, TK2, TK3, TK4, TV>(
            this MultiParamFunctionCacheConfigurationManagerCanx<TK1, TK2, TK3, TK4, TV> configManager,
            Action<RedisCacheFactoryConfig> redisConfigAction,
            Func<IDistributedCacheFactory, IDistributedCacheFactory> cacheConfigAction = null)
        {
            return WithRedisImpl<MultiParamFunctionCacheConfigurationManagerCanx<TK1, TK2, TK3, TK4, TV>, (TK1, TK2, TK3, TK4), TV>(
                configManager, redisConfigAction, cacheConfigAction);
        }
        
        public static MultiParamFunctionCacheConfigurationManagerSyncNoCanx<TK1, TK2, TK3, TK4, TV> WithRedis<TK1, TK2, TK3, TK4, TV>(
            this MultiParamFunctionCacheConfigurationManagerSyncNoCanx<TK1, TK2, TK3, TK4, TV> configManager,
            Action<RedisCacheFactoryConfig> redisConfigAction,
            Func<IDistributedCacheFactory, IDistributedCacheFactory> cacheConfigAction = null)
        {
            return WithRedisImpl<MultiParamFunctionCacheConfigurationManagerSyncNoCanx<TK1, TK2, TK3, TK4, TV>, (TK1, TK2, TK3, TK4), TV>(
                configManager, redisConfigAction, cacheConfigAction);
        }
        
        public static MultiParamFunctionCacheConfigurationManagerSyncCanx<TK1, TK2, TK3, TK4, TV> WithRedis<TK1, TK2, TK3, TK4, TV>(
            this MultiParamFunctionCacheConfigurationManagerSyncCanx<TK1, TK2, TK3, TK4, TV> configManager,
            Action<RedisCacheFactoryConfig> redisConfigAction,
            Func<IDistributedCacheFactory, IDistributedCacheFactory> cacheConfigAction = null)
        {
            return WithRedisImpl<MultiParamFunctionCacheConfigurationManagerSyncCanx<TK1, TK2, TK3, TK4, TV>, (TK1, TK2, TK3, TK4), TV>(
                configManager, redisConfigAction, cacheConfigAction);
        }
        
        public static MultiParamEnumerableKeyFunctionCacheConfigurationManagerNoCanx<TKOuter, TKInnerEnumerable, TRes, TKInner, TV> WithMetrics<TKOuter, TKInnerEnumerable, TRes, TKInner, TV>(
            this MultiParamEnumerableKeyFunctionCacheConfigurationManagerNoCanx<TKOuter, TKInnerEnumerable, TRes, TKInner, TV> configManager,
            Action<RedisCacheFactoryConfig> redisConfigAction,
            Func<IDistributedCacheFactory, IDistributedCacheFactory> cacheConfigAction = null)
            where TKInnerEnumerable : IEnumerable<TKInner>
            where TRes : IDictionary<TKInner, TV>
        {
            return WithRedisImpl<MultiParamEnumerableKeyFunctionCacheConfigurationManagerNoCanx<TKOuter, TKInnerEnumerable, TRes, TKInner, TV>, (TKOuter, TKInner), TV>(
                configManager, redisConfigAction, cacheConfigAction);
        }
        
        public static MultiParamEnumerableKeyFunctionCacheConfigurationManagerCanx<TKOuter, TKInnerEnumerable, TRes, TKInner, TV> WithMetrics<TKOuter, TKInnerEnumerable, TRes, TKInner, TV>(
            this MultiParamEnumerableKeyFunctionCacheConfigurationManagerCanx<TKOuter, TKInnerEnumerable, TRes, TKInner, TV> configManager,
            Action<RedisCacheFactoryConfig> redisConfigAction,
            Func<IDistributedCacheFactory, IDistributedCacheFactory> cacheConfigAction = null)
            where TKInnerEnumerable : IEnumerable<TKInner>
            where TRes : IDictionary<TKInner, TV>
        {
            return WithRedisImpl<MultiParamEnumerableKeyFunctionCacheConfigurationManagerCanx<TKOuter, TKInnerEnumerable, TRes, TKInner, TV>, (TKOuter, TKInner), TV>(
                configManager, redisConfigAction, cacheConfigAction);
        }
        
        public static MultiParamEnumerableKeyFunctionCacheConfigurationManagerSyncNoCanx<TKOuter, TKInnerEnumerable, TRes, TKInner, TV> WithMetrics<TKOuter, TKInnerEnumerable, TRes, TKInner, TV>(
            this MultiParamEnumerableKeyFunctionCacheConfigurationManagerSyncNoCanx<TKOuter, TKInnerEnumerable, TRes, TKInner, TV> configManager,
            Action<RedisCacheFactoryConfig> redisConfigAction,
            Func<IDistributedCacheFactory, IDistributedCacheFactory> cacheConfigAction = null)
            where TKInnerEnumerable : IEnumerable<TKInner>
            where TRes : IDictionary<TKInner, TV>
        {
            return WithRedisImpl<MultiParamEnumerableKeyFunctionCacheConfigurationManagerSyncNoCanx<TKOuter, TKInnerEnumerable, TRes, TKInner, TV>, (TKOuter, TKInner), TV>(
                configManager, redisConfigAction, cacheConfigAction);
        }
        
        public static MultiParamEnumerableKeyFunctionCacheConfigurationManagerSyncCanx<TKOuter, TKInnerEnumerable, TRes, TKInner, TV> WithMetrics<TKOuter, TKInnerEnumerable, TRes, TKInner, TV>(
            this MultiParamEnumerableKeyFunctionCacheConfigurationManagerSyncCanx<TKOuter, TKInnerEnumerable, TRes, TKInner, TV> configManager,
            Action<RedisCacheFactoryConfig> redisConfigAction,
            Func<IDistributedCacheFactory, IDistributedCacheFactory> cacheConfigAction = null)
            where TKInnerEnumerable : IEnumerable<TKInner>
            where TRes : IDictionary<TKInner, TV>
        {
            return WithRedisImpl<MultiParamEnumerableKeyFunctionCacheConfigurationManagerSyncCanx<TKOuter, TKInnerEnumerable, TRes, TKInner, TV>, (TKOuter, TKInner), TV>(
                configManager, redisConfigAction, cacheConfigAction);
        }
        
        public static MultiParamEnumerableKeyFunctionCacheConfigurationManagerNoCanx<TKOuter1, TKOuter2, TKInnerEnumerable, TRes, TKInner, TV> WithMetrics<TKOuter1, TKOuter2, TKInnerEnumerable, TRes, TKInner, TV>(
            this MultiParamEnumerableKeyFunctionCacheConfigurationManagerNoCanx<TKOuter1, TKOuter2, TKInnerEnumerable, TRes, TKInner, TV> configManager,
            Action<RedisCacheFactoryConfig> redisConfigAction,
            Func<IDistributedCacheFactory, IDistributedCacheFactory> cacheConfigAction = null)
            where TKInnerEnumerable : IEnumerable<TKInner>
            where TRes : IDictionary<TKInner, TV>
        {
            return WithRedisImpl<MultiParamEnumerableKeyFunctionCacheConfigurationManagerNoCanx<TKOuter1, TKOuter2, TKInnerEnumerable, TRes, TKInner, TV>, ((TKOuter1, TKOuter2), TKInner), TV>(
                configManager, redisConfigAction, cacheConfigAction);
        }
        
        public static MultiParamEnumerableKeyFunctionCacheConfigurationManagerCanx<TKOuter1, TKOuter2, TKInnerEnumerable, TRes, TKInner, TV> WithMetrics<TKOuter1, TKOuter2, TKInnerEnumerable, TRes, TKInner, TV>(
            this MultiParamEnumerableKeyFunctionCacheConfigurationManagerCanx<TKOuter1, TKOuter2, TKInnerEnumerable, TRes, TKInner, TV> configManager,
            Action<RedisCacheFactoryConfig> redisConfigAction,
            Func<IDistributedCacheFactory, IDistributedCacheFactory> cacheConfigAction = null)
            where TKInnerEnumerable : IEnumerable<TKInner>
            where TRes : IDictionary<TKInner, TV>
        {
            return WithRedisImpl<MultiParamEnumerableKeyFunctionCacheConfigurationManagerCanx<TKOuter1, TKOuter2, TKInnerEnumerable, TRes, TKInner, TV>, ((TKOuter1, TKOuter2), TKInner), TV>(
                configManager, redisConfigAction, cacheConfigAction);
        }
        
        public static MultiParamEnumerableKeyFunctionCacheConfigurationManagerSyncNoCanx<TKOuter1, TKOuter2, TKInnerEnumerable, TRes, TKInner, TV> WithMetrics<TKOuter1, TKOuter2, TKInnerEnumerable, TRes, TKInner, TV>(
            this MultiParamEnumerableKeyFunctionCacheConfigurationManagerSyncNoCanx<TKOuter1, TKOuter2, TKInnerEnumerable, TRes, TKInner, TV> configManager,
            Action<RedisCacheFactoryConfig> redisConfigAction,
            Func<IDistributedCacheFactory, IDistributedCacheFactory> cacheConfigAction = null)
            where TKInnerEnumerable : IEnumerable<TKInner>
            where TRes : IDictionary<TKInner, TV>
        {
            return WithRedisImpl<MultiParamEnumerableKeyFunctionCacheConfigurationManagerSyncNoCanx<TKOuter1, TKOuter2, TKInnerEnumerable, TRes, TKInner, TV>, ((TKOuter1, TKOuter2), TKInner), TV>(
                configManager, redisConfigAction, cacheConfigAction);
        }
        
        public static MultiParamEnumerableKeyFunctionCacheConfigurationManagerSyncCanx<TKOuter1, TKOuter2, TKInnerEnumerable, TRes, TKInner, TV> WithMetrics<TKOuter1, TKOuter2, TKInnerEnumerable, TRes, TKInner, TV>(
            this MultiParamEnumerableKeyFunctionCacheConfigurationManagerSyncCanx<TKOuter1, TKOuter2, TKInnerEnumerable, TRes, TKInner, TV> configManager,
            Action<RedisCacheFactoryConfig> redisConfigAction,
            Func<IDistributedCacheFactory, IDistributedCacheFactory> cacheConfigAction = null)
            where TKInnerEnumerable : IEnumerable<TKInner>
            where TRes : IDictionary<TKInner, TV>
        {
            return WithRedisImpl<MultiParamEnumerableKeyFunctionCacheConfigurationManagerSyncCanx<TKOuter1, TKOuter2, TKInnerEnumerable, TRes, TKInner, TV>, ((TKOuter1, TKOuter2), TKInner), TV>(
                configManager, redisConfigAction, cacheConfigAction);
        }
        
        public static MultiParamEnumerableKeyFunctionCacheConfigurationManagerNoCanx<TKOuter1, TKOuter2, TKOuter3, TKInnerEnumerable, TRes, TKInner, TV> WithMetrics<TKOuter1, TKOuter2, TKOuter3, TKInnerEnumerable, TRes, TKInner, TV>(
            this MultiParamEnumerableKeyFunctionCacheConfigurationManagerNoCanx<TKOuter1, TKOuter2, TKOuter3, TKInnerEnumerable, TRes, TKInner, TV> configManager,
            Action<RedisCacheFactoryConfig> redisConfigAction,
            Func<IDistributedCacheFactory, IDistributedCacheFactory> cacheConfigAction = null)
            where TKInnerEnumerable : IEnumerable<TKInner>
            where TRes : IDictionary<TKInner, TV>
        {
            return WithRedisImpl<MultiParamEnumerableKeyFunctionCacheConfigurationManagerNoCanx<TKOuter1, TKOuter2, TKOuter3, TKInnerEnumerable, TRes, TKInner, TV>, ((TKOuter1, TKOuter2, TKOuter3), TKInner), TV>(
                configManager, redisConfigAction, cacheConfigAction);
        }
        
        public static MultiParamEnumerableKeyFunctionCacheConfigurationManagerCanx<TKOuter1, TKOuter2, TKOuter3, TKInnerEnumerable, TRes, TKInner, TV> WithMetrics<TKOuter1, TKOuter2, TKOuter3, TKInnerEnumerable, TRes, TKInner, TV>(
            this MultiParamEnumerableKeyFunctionCacheConfigurationManagerCanx<TKOuter1, TKOuter2, TKOuter3, TKInnerEnumerable, TRes, TKInner, TV> configManager,
            Action<RedisCacheFactoryConfig> redisConfigAction,
            Func<IDistributedCacheFactory, IDistributedCacheFactory> cacheConfigAction = null)
            where TKInnerEnumerable : IEnumerable<TKInner>
            where TRes : IDictionary<TKInner, TV>
        {
            return WithRedisImpl<MultiParamEnumerableKeyFunctionCacheConfigurationManagerCanx<TKOuter1, TKOuter2, TKOuter3, TKInnerEnumerable, TRes, TKInner, TV>, ((TKOuter1, TKOuter2, TKOuter3), TKInner), TV>(
                configManager, redisConfigAction, cacheConfigAction);
        }
        
        public static MultiParamEnumerableKeyFunctionCacheConfigurationManagerSyncNoCanx<TKOuter1, TKOuter2, TKOuter3, TKInnerEnumerable, TRes, TKInner, TV> WithMetrics<TKOuter1, TKOuter2, TKOuter3, TKInnerEnumerable, TRes, TKInner, TV>(
            this MultiParamEnumerableKeyFunctionCacheConfigurationManagerSyncNoCanx<TKOuter1, TKOuter2, TKOuter3, TKInnerEnumerable, TRes, TKInner, TV> configManager,
            Action<RedisCacheFactoryConfig> redisConfigAction,
            Func<IDistributedCacheFactory, IDistributedCacheFactory> cacheConfigAction = null)
            where TKInnerEnumerable : IEnumerable<TKInner>
            where TRes : IDictionary<TKInner, TV>
        {
            return WithRedisImpl<MultiParamEnumerableKeyFunctionCacheConfigurationManagerSyncNoCanx<TKOuter1, TKOuter2, TKOuter3, TKInnerEnumerable, TRes, TKInner, TV>, ((TKOuter1, TKOuter2, TKOuter3), TKInner), TV>(
                configManager, redisConfigAction, cacheConfigAction);
        }
        
        public static MultiParamEnumerableKeyFunctionCacheConfigurationManagerSyncCanx<TKOuter1, TKOuter2, TKOuter3, TKInnerEnumerable, TRes, TKInner, TV> WithMetrics<TKOuter1, TKOuter2, TKOuter3, TKInnerEnumerable, TRes, TKInner, TV>(
            this MultiParamEnumerableKeyFunctionCacheConfigurationManagerSyncCanx<TKOuter1, TKOuter2, TKOuter3, TKInnerEnumerable, TRes, TKInner, TV> configManager,
            Action<RedisCacheFactoryConfig> redisConfigAction,
            Func<IDistributedCacheFactory, IDistributedCacheFactory> cacheConfigAction = null)
            where TKInnerEnumerable : IEnumerable<TKInner>
            where TRes : IDictionary<TKInner, TV>
        {
            return WithRedisImpl<MultiParamEnumerableKeyFunctionCacheConfigurationManagerSyncCanx<TKOuter1, TKOuter2, TKOuter3, TKInnerEnumerable, TRes, TKInner, TV>, ((TKOuter1, TKOuter2, TKOuter3), TKInner), TV>(
                configManager, redisConfigAction, cacheConfigAction);
        }
        
        private static TConfig WithRedisImpl<TConfig, TK, TV>(
            TConfig configManager,
            Action<RedisCacheFactoryConfig> redisConfigAction,
            Func<IDistributedCacheFactory, IDistributedCacheFactory> cacheConfigAction)
            where TConfig : FunctionCacheConfigurationManagerBase<TConfig, TK, TV>
        {
            var config = new RedisCacheFactoryConfig();

            redisConfigAction(config);
            
            IDistributedCacheFactory cacheFactory = new RedisCacheFactory(config);

            if (cacheConfigAction != null)
                cacheFactory = cacheConfigAction(cacheFactory);
            
            return configManager.WithDistributedCacheFactory(cacheFactory);
        }
    }
}