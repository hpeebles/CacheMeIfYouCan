using System;

namespace CacheMeIfYouCan.Internal
{
    internal class CacheConfig
    {
        public readonly TimeSpan TimeToLive;
        public readonly long MaxItemsInMemoryCache;
        public readonly int MaxConcurrentFetches;
        public readonly bool PreFetchEnabled;
        public readonly ILogger Logger;
        
        internal CacheConfig(CacheConfigOverrides config)
        {
            TimeToLive = config.TimeToLive ?? DefaultCacheSettings.TimeToLive;
            MaxItemsInMemoryCache = config.MaxItemsInMemoryCache ?? DefaultCacheSettings.MaxItemsInMemoryCache;
            MaxConcurrentFetches = config.MaxConcurrentFetches ?? DefaultCacheSettings.MaxConcurrentFetches;
            PreFetchEnabled = config.PreFetchEnabled ?? DefaultCacheSettings.PreFetchEnabled;
            Logger = config.Logger ?? DefaultCacheSettings.Logger;
        }
    }
}