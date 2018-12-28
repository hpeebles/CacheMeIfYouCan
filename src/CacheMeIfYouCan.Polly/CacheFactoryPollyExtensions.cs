using Polly;

namespace CacheMeIfYouCan.Polly
{
    public static class CacheFactoryPollyExtensions
    {
        public static IDistributedCacheFactory WithPolicy(
            this IDistributedCacheFactory cacheFactory,
            Policy policy,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return cacheFactory.WithWrapper(new DistributedCachePollyWrapperFactory(policy), behaviour);
        }
        
        public static IDistributedCacheFactory<TK, TV> WithPolicy<TK, TV>(
            this IDistributedCacheFactory<TK, TV> cacheFactory,
            Policy policy,
            AdditionBehaviour behaviour)
        {
            return cacheFactory.WithWrapper(new DistributedCachePollyWrapperFactory<TK, TV>(policy), behaviour);
        }
        
        public static ILocalCacheFactory WithPolicy(
            this ILocalCacheFactory cacheFactory,
            Policy policy,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return cacheFactory.WithWrapper(new LocalCachePollyWrapperFactory(policy), behaviour);
        }
        
        public static ILocalCacheFactory<TK, TV> WithPolicy<TK, TV>(
            this ILocalCacheFactory<TK, TV> cacheFactory,
            Policy policy,
            AdditionBehaviour behaviour)
        {
            return cacheFactory.WithWrapper(new LocalCachePollyWrapperFactory<TK, TV>(policy), behaviour);
        }
    }
}