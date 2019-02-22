using System;

namespace CacheMeIfYouCan.ApplicationInsights
{
    public static class LocalCacheFactoryExtensions
    {
        public static ILocalCacheFactory WithApplicationInsights(
            this ILocalCacheFactory cacheFactory,
            string host,
            int? keyCountLimit,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            var config = new CacheApplicationInsightsConfig
            {
                Host = host,
                KeyCountLimit = keyCountLimit
            };

            return WithApplicationInsights(cacheFactory, config, behaviour);
        }
        
        public static ILocalCacheFactory WithApplicationInsights(
            this ILocalCacheFactory cacheFactory,
            CacheApplicationInsightsConfig config,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            if (config.Host == null)
                throw new ArgumentNullException(config.Host);

            return cacheFactory.WithWrapper(new LocalCacheApplicationInsightsWrapperFactory(config), behaviour);
        }
        
        public static ILocalCacheFactory<TK, TV> WithApplicationInsights<TK, TV>(
            this ILocalCacheFactory<TK, TV> cacheFactory,
            string host,
            int? keyCountLimit,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            var config = new CacheApplicationInsightsConfig
            {
                Host = host,
                KeyCountLimit = keyCountLimit
            };

            return WithApplicationInsights(cacheFactory, config, behaviour);
        }
        
        public static ILocalCacheFactory<TK, TV> WithApplicationInsights<TK, TV>(
            this ILocalCacheFactory<TK, TV> cacheFactory,
            CacheApplicationInsightsConfig config,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            if (config.Host == null)
                throw new ArgumentNullException(config.Host);

            return cacheFactory.WithWrapper(new LocalCacheApplicationInsightsWrapperFactory(config), behaviour);
        }
    }
}