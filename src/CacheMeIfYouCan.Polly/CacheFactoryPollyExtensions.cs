using Polly;

namespace CacheMeIfYouCan.Polly
{
    public static class CacheFactoryPollyExtensions
    {
        public static IDistributedCacheFactory WithPolicy(
            this IDistributedCacheFactory cacheFactory,
            IAsyncPolicy policy,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return cacheFactory.WithWrapper(new DistributedCachePollyWrapperFactory(policy), behaviour);
        }
        
        public static IDistributedCacheFactory<TK, TV> WithPolicy<TK, TV>(
            this IDistributedCacheFactory<TK, TV> cacheFactory,
            IAsyncPolicy policy,
            AdditionBehaviour behaviour)
        {
            return cacheFactory.WithWrapper(new DistributedCachePollyWrapperFactory<TK, TV>(policy), behaviour);
        }
        
        public static ILocalCacheFactory WithPolicy(
            this ILocalCacheFactory cacheFactory,
            ISyncPolicy policy,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return cacheFactory.WithWrapper(new LocalCachePollyWrapperFactory(policy), behaviour);
        }
        
        public static ILocalCacheFactory<TK, TV> WithPolicy<TK, TV>(
            this ILocalCacheFactory<TK, TV> cacheFactory,
            ISyncPolicy policy,
            AdditionBehaviour behaviour)
        {
            return cacheFactory.WithWrapper(new LocalCachePollyWrapperFactory<TK, TV>(policy), behaviour);
        }
    }
}