using CacheMeIfYouCan.Internal.CachedFunctions;

namespace CacheMeIfYouCan.Events.CachedFunction.SingleKey
{
    public readonly struct SingleKeyCacheGetStats
    {
        private readonly SingleKeyCacheGetFlags _flags;

        internal SingleKeyCacheGetStats(SingleKeyCacheGetFlags flags)
        {
            _flags = flags;
        }

        public bool CacheEnabled => LocalCacheEnabled || DistributedCacheEnabled;
        public bool CacheKeyRequested => LocalCacheKeyRequested || DistributedCacheKeyRequested;
        public bool CacheSkipped => CacheEnabled && !CacheKeyRequested;
        public bool CacheHit => LocalCacheHit || DistributedCacheHit;
        public bool CacheMiss => CacheKeyRequested && !CacheHit;
        
        public bool LocalCacheEnabled => (_flags & SingleKeyCacheGetFlags.LocalCache_Enabled) != 0;
        public bool LocalCacheKeyRequested => (_flags & SingleKeyCacheGetFlags.LocalCache_KeyRequested) != 0;
        public bool LocalCacheSkipped => (_flags & SingleKeyCacheGetFlags.LocalCache_Skipped) != 0;
        public bool LocalCacheHit => (_flags & SingleKeyCacheGetFlags.LocalCache_Hit) != 0;
        public bool LocalCacheMiss => LocalCacheKeyRequested & !LocalCacheHit;
        
        public bool DistributedCacheEnabled => (_flags & SingleKeyCacheGetFlags.DistributedCache_Enabled) != 0;
        public bool DistributedCacheKeyRequested => (_flags & SingleKeyCacheGetFlags.DistributedCache_KeyRequested) != 0;
        public bool DistributedCacheSkipped => (_flags & SingleKeyCacheGetFlags.DistributedCache_Skipped) != 0;
        public bool DistributedCacheHit => (_flags & SingleKeyCacheGetFlags.DistributedCache_Hit) != 0;
        public bool DistributedCacheMiss => DistributedCacheKeyRequested & !DistributedCacheHit;
    }
}