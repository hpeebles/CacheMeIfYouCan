namespace CacheMeIfYouCan.Internal.DistributedCache
{
    internal class DistributedCachePendingRequestsCounterWrapperFactory : IDistributedCacheWrapperFactory
    {
        public IDistributedCache<TK, TV> Wrap<TK, TV>(IDistributedCache<TK, TV> cache)
        {
            return new DistributedCachePendingRequestsCounterWrapper<TK, TV>(cache);
        }
    }
}