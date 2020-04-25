namespace CacheMeIfYouCan
{
    public readonly struct CacheGetManyStats
    {
        internal CacheGetManyStats(
            int cacheKeysRequested,
            int cacheKeysSkipped,
            bool localCacheEnabled = false,
            bool distributedCacheEnabled = false,
            int localCacheKeysSkipped = 0,
            int localCacheHits = 0,
            int distributedCacheKeysSkipped = 0,
            int distributedCacheHits = 0)
        {
            CacheKeysRequested = cacheKeysRequested;
            CacheKeysSkipped = cacheKeysSkipped;
            LocalCacheEnabled = localCacheEnabled;
            LocalCacheKeysSkipped = localCacheKeysSkipped;
            LocalCacheHits = localCacheHits;
            DistributedCacheEnabled = distributedCacheEnabled;
            DistributedCacheKeysSkipped = distributedCacheKeysSkipped;
            DistributedCacheHits = distributedCacheHits;
        }
        
        public bool CacheEnabled => LocalCacheEnabled || DistributedCacheEnabled;
        public int CacheKeysRequested { get; }
        public int CacheKeysSkipped { get; }
        public int CacheHits => LocalCacheHits + DistributedCacheHits;
        public int CacheMisses => CacheKeysRequested - CacheHits;

        public bool LocalCacheEnabled { get; }
        public int LocalCacheKeysRequested => LocalCacheEnabled ? CacheKeysRequested - LocalCacheKeysSkipped : 0;
        public int LocalCacheKeysSkipped { get; }
        public int LocalCacheHits { get; }
        public int LocalCacheMisses => LocalCacheKeysRequested - LocalCacheHits;
        
        public bool DistributedCacheEnabled { get; }
        public int DistributedCacheKeysRequested => DistributedCacheEnabled ? CacheKeysRequested - LocalCacheHits - DistributedCacheKeysSkipped : 0;
        public int DistributedCacheKeysSkipped { get; }
        public int DistributedCacheHits { get; }
        public int DistributedCacheMisses => DistributedCacheKeysRequested - DistributedCacheHits;
    }
}