using System;
using CacheMeIfYouCan.Configuration;

namespace CacheMeIfYouCan
{
    public static class CacheFactoryExtensions
    {
        public static IDistributedCacheFactory Configure(
            this IDistributedCacheFactory cacheFactory,
            Action<DistributedCacheFactoryConfigurationManager> configAction)
        {
            var configManager = new DistributedCacheFactoryConfigurationManager(cacheFactory);

            configAction(configManager);

            return configManager;
        }
        
        public static IDistributedCacheFactory<TK, TV> Configure<TK, TV>(
            this IDistributedCacheFactory<TK, TV> cacheFactory,
            Action<DistributedCacheFactoryConfigurationManager<TK ,TV>> configAction)
        {
            var configManager = new DistributedCacheFactoryConfigurationManager<TK, TV>(cacheFactory);

            configAction(configManager);

            return configManager;
        }
        
        public static ILocalCacheFactory Configure(
            this ILocalCacheFactory cacheFactory,
            Action<LocalCacheFactoryConfigurationManager> configAction)
        {
            var configManager = new LocalCacheFactoryConfigurationManager(cacheFactory);

            configAction(configManager);

            return configManager;
        }
        
        public static ILocalCacheFactory<TK, TV> Configure<TK, TV>(
            this ILocalCacheFactory<TK ,TV> cacheFactory,
            Action<LocalCacheFactoryConfigurationManager<TK ,TV>> configAction)
        {
            var configManager = new LocalCacheFactoryConfigurationManager<TK, TV>(cacheFactory);

            configAction(configManager);

            return configManager;
        }
    }
}