using CacheMeIfYouCan.Internal.CachedFunctions;

namespace CacheMeIfYouCan
{
    public readonly struct CacheGetStats
    {
        private readonly CacheGetFlags _flags;

        internal CacheGetStats(CacheGetFlags flags)
        {
            _flags = flags;
        }

        public bool CacheEnabled => LocalCacheEnabled || DistributedCacheEnabled;
        public bool CacheKeyRequested => LocalCacheKeyRequested || DistributedCacheKeyRequested;
        public bool CacheSkipped => CacheEnabled && !CacheKeyRequested;
        public bool CacheHit => LocalCacheHit || DistributedCacheHit;
        public bool CacheMiss => CacheKeyRequested && !CacheHit;
        
        public bool LocalCacheEnabled => (_flags & CacheGetFlags.LocalCache_Enabled) != 0;
        public bool LocalCacheKeyRequested => (_flags & CacheGetFlags.LocalCache_KeyRequested) != 0;
        public bool LocalCacheSkipped => (_flags & CacheGetFlags.LocalCache_Skipped) != 0;
        public bool LocalCacheHit => (_flags & CacheGetFlags.LocalCache_Hit) != 0;
        public bool LocalCacheMiss => LocalCacheKeyRequested & !LocalCacheHit;
        
        public bool DistributedCacheEnabled => (_flags & CacheGetFlags.DistributedCache_Enabled) != 0;
        public bool DistributedCacheKeyRequested => (_flags & CacheGetFlags.DistributedCache_KeyRequested) != 0;
        public bool DistributedCacheSkipped => (_flags & CacheGetFlags.DistributedCache_Skipped) != 0;
        public bool DistributedCacheHit => (_flags & CacheGetFlags.DistributedCache_Hit) != 0;
        public bool DistributedCacheMiss => DistributedCacheKeyRequested & !DistributedCacheHit;
    }
}