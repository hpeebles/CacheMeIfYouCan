using System;
using CacheMeIfYouCan.Configuration;

namespace CacheMeIfYouCan.Redis
{
    public static class DefaultCacheConfigurationExtensions
    {
        public static DefaultCacheConfiguration WithRedis(
            this DefaultCacheConfiguration config,
            Action<RedisCacheFactoryConfig> redisConfigAction,
            Func<IDistributedCacheFactory, IDistributedCacheFactory> cacheConfigAction = null)
        {
            var redisConfig = new RedisCacheFactoryConfig();

            redisConfigAction(redisConfig);
            
            IDistributedCacheFactory cacheFactory = new RedisCacheFactory(redisConfig);

            if (cacheConfigAction != null)
                cacheFactory = cacheConfigAction(cacheFactory);

            return config.WithDistributedCacheFactory(cacheFactory);
        }
    }
}