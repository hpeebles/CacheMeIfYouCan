using System;

namespace CacheMeIfYouCan.Internal
{
    internal class CacheConfigOverrides
    {
        public TimeSpan? TimeToLive;
        public long? MaxItemsInMemoryCache;
        public int? MaxConcurrentFetches;
        public bool? PreFetchEnabled;
        public ILogger Logger;
    }
}