using System;

namespace CacheMeIfYouCan.Redis
{
    public static class InterfaceCacheConfigurationManagerRedisExtensions
    {
        public static CachedProxyConfigurationManager<T> WithRedis<T>(
            this CachedProxyConfigurationManager<T> configManager,
            Action<RedisConfig> configAction)
        {
            var config = new RedisConfig();

            configAction(config);
            
            configManager.WithCacheFactory(new RedisCacheFactory(config));
            
            return configManager;
        }
    }
}