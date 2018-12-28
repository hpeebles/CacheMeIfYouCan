using Polly;

namespace CacheMeIfYouCan.Polly
{
    internal class DistributedCachePollyWrapperFactory : IDistributedCacheWrapperFactory
    {
        private readonly Policy _policy;

        public DistributedCachePollyWrapperFactory(Policy policy)
        {
            _policy = policy;
        }

        public IDistributedCache<TK, TV> Wrap<TK, TV>(IDistributedCache<TK, TV> cache)
        {
            return new DistributedCachePollyWrapper<TK, TV>(cache, _policy);
        }
    }
    
    internal class DistributedCachePollyWrapperFactory<TK, TV> : IDistributedCacheWrapperFactory<TK, TV>
    {
        private readonly Policy _policy;

        public DistributedCachePollyWrapperFactory(Policy policy)
        {
            _policy = policy;
        }

        public IDistributedCache<TK, TV> Wrap(IDistributedCache<TK, TV> cache)
        {
            return new DistributedCachePollyWrapper<TK, TV>(cache, _policy);
        }
    }
}