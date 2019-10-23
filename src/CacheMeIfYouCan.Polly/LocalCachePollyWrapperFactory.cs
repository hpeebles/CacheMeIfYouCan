using Polly;

namespace CacheMeIfYouCan.Polly
{
    internal class LocalCachePollyWrapperFactory : ILocalCacheWrapperFactory
    {
        private readonly ISyncPolicy _policy;

        public LocalCachePollyWrapperFactory(ISyncPolicy policy)
        {
            _policy = policy;
        }

        public ILocalCache<TK, TV> Wrap<TK, TV>(ILocalCache<TK, TV> cache)
        {
            return new LocalCachePollyWrapper<TK, TV>(cache, _policy);
        }
    }
    
    internal class LocalCachePollyWrapperFactory<TK, TV> : ILocalCacheWrapperFactory<TK, TV>
    {
        private readonly ISyncPolicy _policy;

        public LocalCachePollyWrapperFactory(ISyncPolicy policy)
        {
            _policy = policy;
        }

        public ILocalCache<TK, TV> Wrap(ILocalCache<TK, TV> cache)
        {
            return new LocalCachePollyWrapper<TK, TV>(cache, _policy);
        }
    }
}