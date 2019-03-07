using CacheMeIfYouCan.Configuration;

namespace CacheMeIfYouCan.Internal.DistributedCache
{
    internal class DistributedCacheDuplicateRequestCatchingWrapperFactory : IDistributedCacheWrapperFactory
    {
        public IDistributedCache<TK, TV> Wrap<TK, TV>(
            IDistributedCache<TK, TV> cache,
            IDistributedCacheConfig<TK, TV> config)
        {
            return new DistributedCacheDuplicateRequestCatchingWrapper<TK, TV>(cache, config.KeyComparer);
        }
    }
}