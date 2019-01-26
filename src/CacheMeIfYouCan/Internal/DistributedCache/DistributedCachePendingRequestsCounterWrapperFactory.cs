using CacheMeIfYouCan.Configuration;

namespace CacheMeIfYouCan.Internal.DistributedCache
{
    internal class DistributedCachePendingRequestsCounterWrapperFactory : IDistributedCacheWrapperFactory
    {
        public IDistributedCache<TK, TV> Wrap<TK, TV>(
            IDistributedCache<TK, TV> cache,
            DistributedCacheConfig<TK, TV> config)
        {
            return new DistributedCachePendingRequestsCounterWrapper<TK, TV>(cache);
        }
    }
}