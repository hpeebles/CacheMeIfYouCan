using Polly;

namespace CacheMeIfYouCan.Polly
{
    public static class CacheConfigurationManagerExtensions
    {
        public static IDistributedCacheFactory WithPolicy(
            this IDistributedCacheFactory configurationManager,
            Policy policy,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return configurationManager.WithWrapper(new CachePollyWrapperFactory(policy), behaviour);
        }
        
        public static IDistributedCacheFactory<TK, TV> WithPolicy<TK, TV>(
            this IDistributedCacheFactory<TK, TV> configurationManager,
            Policy policy,
            AdditionBehaviour behaviour)
        {
            return configurationManager.WithWrapper(new CachePollyWrapperFactory<TK, TV>(policy), behaviour);
        }
    }
}