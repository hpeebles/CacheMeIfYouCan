using Polly;

namespace CacheMeIfYouCan.Polly
{
    public static class CacheFactoryPollyExtensions
    {
        public static IDistributedCacheFactory WithPolicy(
            this IDistributedCacheFactory configurationManager,
            Policy policy,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return configurationManager.WithWrapper(new DistributedCachePollyWrapperFactory(policy), behaviour);
        }
        
        public static IDistributedCacheFactory<TK, TV> WithPolicy<TK, TV>(
            this IDistributedCacheFactory<TK, TV> configurationManager,
            Policy policy,
            AdditionBehaviour behaviour)
        {
            return configurationManager.WithWrapper(new DistributedCachePollyWrapperFactory<TK, TV>(policy), behaviour);
        }
        
        public static ILocalCacheFactory WithPolicy(
            this ILocalCacheFactory configurationManager,
            Policy policy,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return configurationManager.WithWrapper(new LocalCachePollyWrapperFactory(policy), behaviour);
        }
        
        public static ILocalCacheFactory<TK, TV> WithPolicy<TK, TV>(
            this ILocalCacheFactory<TK, TV> configurationManager,
            Policy policy,
            AdditionBehaviour behaviour)
        {
            return configurationManager.WithWrapper(new LocalCachePollyWrapperFactory<TK, TV>(policy), behaviour);
        }
    }
}