namespace CacheMeIfYouCan
{
    public readonly struct CachedItemCount
    {
        internal CachedItemCount(string cacheName, string cacheType, long count)
        {
            CacheName = cacheName;
            CacheType = cacheType;
            Count = count;
        }
        
        public string CacheName { get; }
        public string CacheType { get; }
        public long Count { get; }
    }
}