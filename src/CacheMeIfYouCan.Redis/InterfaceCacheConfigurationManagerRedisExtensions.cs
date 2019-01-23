using System;
using CacheMeIfYouCan.Configuration;

namespace CacheMeIfYouCan.Redis
{
    public static class InterfaceCacheConfigurationManagerRedisExtensions
    {
        public static CachedProxyConfigurationManager<T> WithRedis<T>(
            this CachedProxyConfigurationManager<T> configManager,
            Action<RedisCacheFactoryConfig> redisConfigAction,
            Func<IDistributedCacheFactory, IDistributedCacheFactory> cacheConfigAction = null,
            Func<CachedProxyFunctionInfo, string> keyspacePrefixFunc = null)
        {
            var config = new RedisCacheFactoryConfig();

            redisConfigAction(config);
            
            IDistributedCacheFactory cacheFactory = new RedisCacheFactory(config);

            if (cacheConfigAction != null)
                cacheFactory = cacheConfigAction(cacheFactory);
            
            return configManager.WithDistributedCacheFactory(cacheFactory, keyspacePrefixFunc);
        }
    }
}