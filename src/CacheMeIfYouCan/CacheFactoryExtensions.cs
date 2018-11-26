using System;
using CacheMeIfYouCan.Configuration;

namespace CacheMeIfYouCan
{
    public static class CacheFactoryExtensions
    {
        public static IDistributedCacheFactory Configure(
            this IDistributedCacheFactory cacheFactory,
            Action<DistributedCacheConfigurationManager> configAction)
        {
            var configManager = new DistributedCacheConfigurationManager(cacheFactory);

            configAction(configManager);

            return configManager;
        }
        
        public static IDistributedCacheFactory<TK, TV> Configure<TK, TV>(
            this IDistributedCacheFactory<TK, TV> cacheFactory,
            Action<DistributedCacheConfigurationManager<TK ,TV>> configAction)
        {
            var configManager = new DistributedCacheConfigurationManager<TK, TV>(cacheFactory);

            configAction(configManager);

            return configManager;
        }
        
        public static ILocalCacheFactory Configure(
            this ILocalCacheFactory cacheFactory,
            Action<LocalCacheConfigurationManager> configAction)
        {
            var configManager = new LocalCacheConfigurationManager(cacheFactory);

            configAction(configManager);

            return configManager;
        }
        
        public static ILocalCacheFactory<TK, TV> Configure<TK, TV>(
            this ILocalCacheFactory<TK ,TV> cacheFactory,
            Action<LocalCacheConfigurationManager<TK ,TV>> configAction)
        {
            var configManager = new LocalCacheConfigurationManager<TK, TV>(cacheFactory);

            configAction(configManager);

            return configManager;
        }
    }
}