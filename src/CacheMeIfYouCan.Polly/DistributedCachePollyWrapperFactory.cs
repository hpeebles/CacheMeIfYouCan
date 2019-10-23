using CacheMeIfYouCan.Configuration;
using Polly;

namespace CacheMeIfYouCan.Polly
{
    internal class DistributedCachePollyWrapperFactory : IDistributedCacheWrapperFactory
    {
        private readonly IAsyncPolicy _policy;

        public DistributedCachePollyWrapperFactory(IAsyncPolicy policy)
        {
            _policy = policy;
        }

        public IDistributedCache<TK, TV> Wrap<TK, TV>(
            IDistributedCache<TK, TV> cache,
            IDistributedCacheConfig<TK, TV> config)
        {
            return new DistributedCachePollyWrapper<TK, TV>(cache, _policy);
        }
    }
    
    internal class DistributedCachePollyWrapperFactory<TK, TV> : IDistributedCacheWrapperFactory<TK, TV>
    {
        private readonly IAsyncPolicy _policy;

        public DistributedCachePollyWrapperFactory(IAsyncPolicy policy)
        {
            _policy = policy;
        }

        public IDistributedCache<TK, TV> Wrap(
            IDistributedCache<TK, TV> cache,
            IDistributedCacheConfig<TK, TV> config)
        {
            return new DistributedCachePollyWrapper<TK, TV>(cache, _policy);
        }
    }
}