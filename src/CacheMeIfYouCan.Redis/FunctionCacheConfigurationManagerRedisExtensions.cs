using System;
using System.Collections.Generic;
using CacheMeIfYouCan.Configuration;

namespace CacheMeIfYouCan.Redis
{
    public static class FunctionCacheConfigurationManagerRedisExtensions
    {
        public static FunctionCacheConfigurationManager<TK, TV> WithRedis<TK, TV>(
            this FunctionCacheConfigurationManager<TK, TV> configManager,
            Action<RedisCacheFactoryConfig> redisConfigAction,
            Func<IDistributedCacheFactory, IDistributedCacheFactory> cacheConfigAction = null)
        {
            return WithRedis<FunctionCacheConfigurationManager<TK, TV>, TK, TV>(
                configManager, redisConfigAction, cacheConfigAction);
        }
        
        public static FunctionCacheConfigurationManagerSync<TK, TV> WithRedis<TK, TV>(
            this FunctionCacheConfigurationManagerSync<TK, TV> configManager,
            Action<RedisCacheFactoryConfig> redisConfigAction,
            Func<IDistributedCacheFactory, IDistributedCacheFactory> cacheConfigAction = null)
        {
            return WithRedis<FunctionCacheConfigurationManagerSync<TK, TV>, TK, TV>(
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