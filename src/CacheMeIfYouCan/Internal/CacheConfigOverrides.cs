using System;

namespace CacheMeIfYouCan.Internal
{
    internal class CacheConfigOverrides
    {
        public TimeSpan? TimeToLive;
        public int? MemoryCacheMaxSizeMB;
        public int? MaxConcurrentFetches;
        public bool? EarlyFetchEnabled;
    }
}