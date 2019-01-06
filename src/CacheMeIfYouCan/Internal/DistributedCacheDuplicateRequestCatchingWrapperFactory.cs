namespace CacheMeIfYouCan.Internal
{
    internal class DistributedCacheDuplicateRequestCatchingWrapperFactory : IDistributedCacheWrapperFactory
    {
        public IDistributedCache<TK, TV> Wrap<TK, TV>(IDistributedCache<TK, TV> cache)
        {
            var keyComparer = KeyComparerResolver.Get<TK>();
            
            return new DistributedCacheDuplicateRequestCatchingWrapper<TK, TV>(cache, keyComparer);
        }
    }
}