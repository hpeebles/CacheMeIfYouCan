using System;
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
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> WithRedis<TK, TV>(
            this MultiKeyFunctionCacheConfigurationManager<TK, TV> configManager,
            Action<RedisCacheFactoryConfig> configAction)
        {
            return WithRedis<MultiKeyFunctionCacheConfigurationManager<TK, TV>, TK, TV>(configManager, configAction);
        }
        
        private static TConfig WithRedis<TConfig, TK, TV>(
            TConfig configManager,
            Action<RedisCacheFactoryConfig> configAction) 
            where TConfig : FunctionCacheConfigurationManagerBase<TConfig, TK, TV>
        {
            var config = new RedisCacheFactoryConfig();

            configAction(config);
            
            var keyspacePrefix = config.KeySpacePrefixFunc?.Invoke(null);

            configManager.WithRemoteCacheFactory(new RedisCacheFactory(config), keyspacePrefix);
            
            return configManager;
        }
    }
}