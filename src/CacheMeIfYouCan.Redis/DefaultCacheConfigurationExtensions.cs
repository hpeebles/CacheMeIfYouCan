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
        
            return config.WithRemoteCacheFactory(new RedisCacheFactory(redisConfig));
        }
    }
}