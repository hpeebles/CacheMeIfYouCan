using System;
using System.Collections.Generic;
using CacheMeIfYouCan.Configuration;

namespace CacheMeIfYouCan.Redis
{
    public static class FunctionCacheConfigurationManagerRedisExtensions
    {
        public static FunctionCacheConfigurationManager<TK, TV> WithRedis<TK, TV>(
            this FunctionCacheConfigurationManager<TK, TV> configManager,
            Action<RedisCacheFactoryConfig> configAction)
        {
            return WithRedis<FunctionCacheConfigurationManager<TK, TV>, TK, TV>(configManager, configAction);
        }
        
        public static FunctionCacheConfigurationManagerSync<TK, TV> WithRedis<TK, TV>(
            this FunctionCacheConfigurationManagerSync<TK, TV> configManager,
            Action<RedisCacheFactoryConfig> configAction)
        {
            return WithRedis<FunctionCacheConfigurationManagerSync<TK, TV>, TK, TV>(configManager, configAction);
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TReq, TRes, TK, TV> WithRedis<TReq, TRes, TK, TV>(
            this MultiKeyFunctionCacheConfigurationManager<TReq, TRes, TK, TV> configManager,
            Action<RedisCacheFactoryConfig> configAction)
            where TReq : IEnumerable<TK>
            where TRes : IDictionary<TK, TV>
        {
            return WithRedis<MultiKeyFunctionCacheConfigurationManager<TReq, TRes, TK, TV>, TK, TV>(
                configManager, configAction);
        }
        
        public static MultiKeyFunctionCacheConfigurationManagerSync<TReq, TRes, TK, TV> WithRedis<TReq, TRes, TK, TV>(
            this MultiKeyFunctionCacheConfigurationManagerSync<TReq, TRes, TK, TV> configManager,
            Action<RedisCacheFactoryConfig> configAction)
            where TReq : IEnumerable<TK>
            where TRes : IDictionary<TK, TV>
        {
            return WithRedis<MultiKeyFunctionCacheConfigurationManagerSync<TReq, TRes, TK, TV>, TK, TV>(
                configManager, configAction);
        }
        
        private static TConfig WithRedis<TConfig, TK, TV>(
            TConfig configManager,
            Action<RedisCacheFactoryConfig> configAction) 
            where TConfig : FunctionCacheConfigurationManagerBase<TConfig, TK, TV>
        {
            var config = new RedisCacheFactoryConfig();

            configAction(config);
            
            var keyspacePrefix = config.KeySpacePrefixFunc?.Invoke(null);

            configManager.WithDistributedCacheFactory(new RedisCacheFactory(config), keyspacePrefix);
            
            return configManager;
        }
    }
}