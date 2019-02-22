using System;
using System.Linq.Expressions;

namespace CacheMeIfYouCan.ApplicationInsights
{
    public static class DistributedCacheFactoryExtensions
    {
        public static IDistributedCacheFactory WithApplicationInsights(
            this IDistributedCacheFactory cacheFactory,
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
        
        public static IDistributedCacheFactory WithApplicationInsights(
            this IDistributedCacheFactory cacheFactory,
            CacheApplicationInsightsConfig config,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            if (config.Host == null)
                throw new ArgumentNullException(config.Host);
            
            return cacheFactory.WithWrapper(new DistributedCacheApplicationInsightsWrapperFactory(config), behaviour);
        }
        
        public static IDistributedCacheFactory<TK, TV> WithApplicationInsights<TK, TV>(
            this IDistributedCacheFactory<TK, TV> cacheFactory,
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
        
        public static IDistributedCacheFactory<TK, TV> WithApplicationInsights<TK, TV>(
            this IDistributedCacheFactory<TK, TV> cacheFactory,
            CacheApplicationInsightsConfig config,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            if (config.Host == null)
                throw new ArgumentNullException(config.Host);

            return cacheFactory.WithWrapper(new DistributedCacheApplicationInsightsWrapperFactory(config), behaviour);
        }
    }
}