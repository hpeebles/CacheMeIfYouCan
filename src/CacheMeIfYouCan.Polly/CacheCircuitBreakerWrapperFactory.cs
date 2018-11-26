using Polly;

namespace CacheMeIfYouCan.Polly
{
    internal class CachePollyWrapperFactory : IDistributedCacheWrapperFactory
    {
        private readonly Policy _policy;

        public CachePollyWrapperFactory(Policy policy)
        {
            _policy = policy;
        }

        public IDistributedCache<TK, TV> Wrap<TK, TV>(IDistributedCache<TK, TV> cache)
        {
            return new CachePollyWrapper<TK, TV>(cache, _policy);
        }
    }
    
    internal class CachePollyWrapperFactory<TK, TV> : IDistributedCacheWrapperFactory<TK, TV>
    {
        private readonly Policy _policy;

        public CachePollyWrapperFactory(Policy policy)
        {
            _policy = policy;
        }

        public IDistributedCache<TK, TV> Wrap(IDistributedCache<TK, TV> cache)
        {
            return new CachePollyWrapper<TK, TV>(cache, _policy);
        }
    }
}