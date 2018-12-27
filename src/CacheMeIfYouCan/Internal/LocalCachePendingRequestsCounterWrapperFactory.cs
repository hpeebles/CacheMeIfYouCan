namespace CacheMeIfYouCan.Internal
{
    internal class LocalCachePendingRequestsCounterWrapperFactory : ILocalCacheWrapperFactory
    {
        public ILocalCache<TK, TV> Wrap<TK, TV>(ILocalCache<TK, TV> cache)
        {
            return new LocalCachePendingRequestsCounterWrapper<TK, TV>(cache);
        }
    }
}