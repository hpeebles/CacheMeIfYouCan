namespace CacheMeIfYouCan.Caches
{
    public class CachedItemCount
    {
        public readonly string CacheName;
        public readonly string CacheType;
        public readonly long Count;

        internal CachedItemCount(string cacheName, string cacheType, long count)
        {
            CacheName = cacheName;
            CacheType = cacheType;
            Count = count;
        }
    }
}