using System;
using CacheMeIfYouCan.Configuration;

namespace CacheMeIfYouCan.Redis
{
    public static class DefaultCacheConfigurationExtensions
    {
        public static DefaultCacheConfiguration WithRedis(this DefaultCacheConfiguration config, Action<RedisCacheFactoryConfig> configAction)
        {
            var redisConfig = new RedisCacheFactoryConfig();

            configAction(redisConfig);
        
            config.RemoteCacheFactory = new RedisCacheFactory(redisConfig);
        
            return config;
        }
    }
}