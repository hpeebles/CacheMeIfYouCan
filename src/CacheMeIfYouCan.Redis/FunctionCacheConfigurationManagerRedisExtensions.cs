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
            var config = new RedisCacheFactoryConfig();

            configAction(config);
            
            configManager.WithRemoteCacheFactory(new RedisCacheFactory(config));
            
            return configManager;
        }
    }
}