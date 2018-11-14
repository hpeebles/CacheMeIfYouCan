namespace CacheMeIfYouCan.Caches
{
    public class MemoryCacheFactory : ILocalCacheFactory
    {
        public bool RequiresStringKeys => true;

        public ILocalCache<TK, TV> Build<TK, TV>(string cacheName)
        {
            return new MemoryCache<TK, TV>(cacheName);
        }
    }
}
