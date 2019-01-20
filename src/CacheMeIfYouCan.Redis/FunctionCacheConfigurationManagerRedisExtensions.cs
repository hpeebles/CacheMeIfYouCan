using System;
using System.Collections.Generic;
using CacheMeIfYouCan.Configuration;

namespace CacheMeIfYouCan.Redis
{
    public static class FunctionCacheConfigurationManagerRedisExtensions
    {
        public static SingleKeyFunctionCacheConfigurationManager<TK, TV> WithRedis<TK, TV>(
            this SingleKeyFunctionCacheConfigurationManager<TK, TV> configManager,
            Action<RedisCacheFactoryConfig> redisConfigAction,
            Func<IDistributedCacheFactory, IDistributedCacheFactory> cacheConfigAction = null)
        {
            return WithRedis<SingleKeyFunctionCacheConfigurationManager<TK, TV>, TK, TV>(
                configManager, redisConfigAction, cacheConfigAction);
        }
        
        public static SingleKeyFunctionCacheConfigurationManagerSync<TK, TV> WithRedis<TK, TV>(
            this SingleKeyFunctionCacheConfigurationManagerSync<TK, TV> configManager,
            Action<RedisCacheFactoryConfig> redisConfigAction,
            Func<IDistributedCacheFactory, IDistributedCacheFactory> cacheConfigAction = null)
        {
            return WithRedis<SingleKeyFunctionCacheConfigurationManagerSync<TK, TV>, TK, TV>(
                configManager, redisConfigAction, cacheConfigAction);
        }
        
        public static EnumerableKeyFunctionCacheConfigurationManager<TReq, TRes, TK, TV> WithRedis<TReq, TRes, TK, TV>(
            this EnumerableKeyFunctionCacheConfigurationManager<TReq, TRes, TK, TV> configManager,
            Action<RedisCacheFactoryConfig> redisConfigAction,
            Func<IDistributedCacheFactory, IDistributedCacheFactory> cacheConfigAction = null)
            where TReq : IEnumerable<TK>
            where TRes : IDictionary<TK, TV>
        {
            return WithRedis<EnumerableKeyFunctionCacheConfigurationManager<TReq, TRes, TK, TV>, TK, TV>(
                configManager, redisConfigAction, cacheConfigAction);
        }
        
        public static EnumerableKeyFunctionCacheConfigurationManagerSync<TReq, TRes, TK, TV> WithRedis<TReq, TRes, TK, TV>(
            this EnumerableKeyFunctionCacheConfigurationManagerSync<TReq, TRes, TK, TV> configManager,
            Action<RedisCacheFactoryConfig> redisConfigAction,
            Func<IDistributedCacheFactory, IDistributedCacheFactory> cacheConfigAction = null)
            where TReq : IEnumerable<TK>
            where TRes : IDictionary<TK, TV>
        {
            return WithRedis<EnumerableKeyFunctionCacheConfigurationManagerSync<TReq, TRes, TK, TV>, TK, TV>(
                configManager, redisConfigAction, cacheConfigAction);
        }
        
        public static MultiParamFunctionCacheConfigurationManager<TK1, TK2, TV> WithRedis<TK1, TK2, TV>(
            this MultiParamFunctionCacheConfigurationManager<TK1, TK2, TV> configManager,
            Action<RedisCacheFactoryConfig> redisConfigAction,
            Func<IDistributedCacheFactory, IDistributedCacheFactory> cacheConfigAction = null)
        {
            return WithRedis<MultiParamFunctionCacheConfigurationManager<TK1, TK2, TV>, (TK1, TK2), TV>(
                configManager, redisConfigAction, cacheConfigAction);
        }
        
        public static MultiParamFunctionCacheConfigurationManagerSync<TK1, TK2, TV> WithRedis<TK1, TK2, TV>(
            this MultiParamFunctionCacheConfigurationManagerSync<TK1, TK2, TV> configManager,
            Action<RedisCacheFactoryConfig> redisConfigAction,
            Func<IDistributedCacheFactory, IDistributedCacheFactory> cacheConfigAction = null)
        {
            return WithRedis<MultiParamFunctionCacheConfigurationManagerSync<TK1, TK2, TV>, (TK1, TK2), TV>(
                configManager, redisConfigAction, cacheConfigAction);
        }
        
        public static MultiParamFunctionCacheConfigurationManager<TK1, TK2, TK3, TV> WithRedis<TK1, TK2, TK3, TV>(
            this MultiParamFunctionCacheConfigurationManager<TK1, TK2, TK3, TV> configManager,
            Action<RedisCacheFactoryConfig> redisConfigAction,
            Func<IDistributedCacheFactory, IDistributedCacheFactory> cacheConfigAction = null)
        {
            return WithRedis<MultiParamFunctionCacheConfigurationManager<TK1, TK2, TK3, TV>, (TK1, TK2, TK3), TV>(
                configManager, redisConfigAction, cacheConfigAction);
        }
        
        public static MultiParamFunctionCacheConfigurationManagerSync<TK1, TK2, TK3, TV> WithRedis<TK1, TK2, TK3, TV>(
            this MultiParamFunctionCacheConfigurationManagerSync<TK1, TK2, TK3, TV> configManager,
            Action<RedisCacheFactoryConfig> redisConfigAction,
            Func<IDistributedCacheFactory, IDistributedCacheFactory> cacheConfigAction = null)
        {
            return WithRedis<MultiParamFunctionCacheConfigurationManagerSync<TK1, TK2, TK3, TV>, (TK1, TK2, TK3), TV>(
                configManager, redisConfigAction, cacheConfigAction);
        }
        
        public static MultiParamFunctionCacheConfigurationManager<TK1, TK2, TK3, TK4, TV> WithRedis<TK1, TK2, TK3, TK4, TV>(
            this MultiParamFunctionCacheConfigurationManager<TK1, TK2, TK3, TK4, TV> configManager,
            Action<RedisCacheFactoryConfig> redisConfigAction,
            Func<IDistributedCacheFactory, IDistributedCacheFactory> cacheConfigAction = null)
        {
            return WithRedis<MultiParamFunctionCacheConfigurationManager<TK1, TK2, TK3, TK4, TV>, (TK1, TK2, TK3, TK4), TV>(
                configManager, redisConfigAction, cacheConfigAction);
        }
        
        public static MultiParamFunctionCacheConfigurationManagerSync<TK1, TK2, TK3, TK4, TV> WithRedis<TK1, TK2, TK3, TK4, TV>(
            this MultiParamFunctionCacheConfigurationManagerSync<TK1, TK2, TK3, TK4, TV> configManager,
            Action<RedisCacheFactoryConfig> redisConfigAction,
            Func<IDistributedCacheFactory, IDistributedCacheFactory> cacheConfigAction = null)
        {
            return WithRedis<MultiParamFunctionCacheConfigurationManagerSync<TK1, TK2, TK3, TK4, TV>, (TK1, TK2, TK3, TK4), TV>(
                configManager, redisConfigAction, cacheConfigAction);
        }
        
        private static TConfig WithRedis<TConfig, TK, TV>(
            TConfig configManager,
            Action<RedisCacheFactoryConfig> redisConfigAction,
            Func<IDistributedCacheFactory, IDistributedCacheFactory> cacheConfigAction)
            where TConfig : FunctionCacheConfigurationManagerBase<TConfig, TK, TV>
        {
            var config = new RedisCacheFactoryConfig();

            redisConfigAction(config);
            
            var keyspacePrefix = config.KeySpacePrefixFunc?.Invoke(null);

            IDistributedCacheFactory cacheFactory = new RedisCacheFactory(config);

            if (cacheConfigAction != null)
                cacheFactory = cacheConfigAction(cacheFactory);
            
            return configManager.WithDistributedCacheFactory(cacheFactory, keyspacePrefix);
        }
    }
}