using CacheMeIfYouCan.Configuration;

namespace CacheMeIfYouCan.Internal
{
    internal class NullLocalCacheFactory : ILocalCacheFactory
    {
        public ILocalCache<TK, TV> Build<TK, TV>(ILocalCacheConfig<TK> config)
        {
            return null;
        }
    }
    
    internal class NullLocalCacheFactory<TK, TV> : ILocalCacheFactory<TK, TV>
    {
        public ILocalCache<TK, TV> Build(ILocalCacheConfig<TK> config)
        {
            return null;
        }
    }
}