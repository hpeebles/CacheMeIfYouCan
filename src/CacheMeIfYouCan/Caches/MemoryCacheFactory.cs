namespace CacheMeIfYouCan.Caches
{
    public class MemoryCacheFactory : ILocalCacheFactory
    {
        public ILocalCache<TK, TV> Build<TK, TV>(string cacheName)
        {
            return new MemoryCache<TK, TV>(cacheName);
        }
    }
}
