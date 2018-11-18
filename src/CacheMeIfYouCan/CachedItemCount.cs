namespace CacheMeIfYouCan
{
    public readonly struct CachedItemCount
    {
        public string CacheName { get; }
        public string CacheType { get; }
        public long Count { get; }

        internal CachedItemCount(string cacheName, string cacheType, long count)
        {
            CacheName = cacheName;
            CacheType = cacheType;
            Count = count;
        }
    }
}