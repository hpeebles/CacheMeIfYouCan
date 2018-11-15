using System;
using CacheMeIfYouCan.Configuration;

namespace CacheMeIfYouCan
{
    public static class CacheFactoryExtensions
    {
        public static ICacheFactory Configure(
            this ICacheFactory cacheFactory,
            Action<CacheConfigurationManager> configAction)
        {
            var configManager = new CacheConfigurationManager(cacheFactory);

            configAction(configManager);

            return configManager;
        }
        
        public static ICacheFactory<TK, TV> Configure<TK, TV>(
            this ICacheFactory<TK ,TV> cacheFactory,
            Action<CacheConfigurationManager<TK ,TV>> configAction)
        {
            var configManager = new CacheConfigurationManager<TK, TV>(cacheFactory);

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