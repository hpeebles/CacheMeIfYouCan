using Polly;

namespace CacheMeIfYouCan.Polly
{
    internal class CachePollyWrapperFactory : ICacheWrapperFactory
    {
        private readonly Policy _policy;

        public CachePollyWrapperFactory(Policy policy)
        {
            _policy = policy;
        }

        public ICache<TK, TV> Wrap<TK, TV>(ICache<TK, TV> cache)
        {
            return new CachePollyWrapper<TK, TV>(cache, _policy);
        }
    }
    
    internal class CachePollyWrapperFactory<TK, TV> : ICacheWrapperFactory<TK, TV>
    {
        private readonly Policy _policy;

        public CachePollyWrapperFactory(Policy policy)
        {
            _policy = policy;
        }

        public ICache<TK, TV> Wrap(ICache<TK, TV> cache)
        {
            return new CachePollyWrapper<TK, TV>(cache, _policy);
        }
    }
}