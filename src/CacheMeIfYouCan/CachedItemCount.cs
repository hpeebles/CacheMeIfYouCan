namespace CacheMeIfYouCan
{
    /// <summary>
    /// Represents the count of items in a specific cache
    /// </summary>
    public readonly struct CachedItemCount
    {
        internal CachedItemCount(string cacheName, string cacheType, long count)
        {
            CacheName = cacheName;
            CacheType = cacheType;
            Count = count;
        }

        /// <summary>
        /// The name used to identify the cache
        /// </summary>
        public string CacheName { get; }
        
        /// <summary>
        /// The name used to identify the type of the cache
        /// </summary>
        public string CacheType { get; }
        
        /// <summary>
        /// The number of items in the cache
        /// </summary>
        public long Count { get; }
    }
}