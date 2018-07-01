using System;

namespace CacheMeIfYouCan.Redis
{
    public static class InterfaceCacheConfigurationManagerRedisExtensions
    {
        public static CachedProxyConfigurationManager<T> WithRedis<T>(
            this CachedProxyConfigurationManager<T> configManager,
            Action<RedisCacheFactoryConfig> configAction)
        {
            var config = new RedisCacheFactoryConfig();

            configAction(config);
            
            configManager.WithCacheFactory(new RedisCacheFactory(config));
            
            return configManager;
        }
    }
}