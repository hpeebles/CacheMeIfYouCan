using CacheMeIfYouCan.Configuration;

namespace CacheMeIfYouCan.Caches
{
    public class MemoryCacheFactory : ILocalCacheFactory
    {
        public ILocalCache<TK, TV> Build<TK, TV>(ILocalCacheConfig<TK> config)
        {
            return new MemoryCache<TK, TV>(config.CacheName);
        }
    }
}
