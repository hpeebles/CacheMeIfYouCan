using System;

namespace CacheMeIfYouCan.Internal
{
    internal class CacheConfig
    {
        public readonly TimeSpan TimeToLive;
        public readonly long MaxItemsInMemoryCache;
        public readonly int MaxConcurrentFetches;
        public readonly bool EarlyFetchEnabled;
        public readonly Action<FunctionCacheErrorEvent> OnError;
        
        internal CacheConfig(CacheConfigOverrides config)
        {
            TimeToLive = config.TimeToLive ?? DefaultCacheSettings.TimeToLive;
            MaxItemsInMemoryCache = config.MaxItemsInMemoryCache ?? DefaultCacheSettings.MaxItemsInMemoryCache;
            MaxConcurrentFetches = config.MaxConcurrentFetches ?? DefaultCacheSettings.MaxConcurrentFetches;
            EarlyFetchEnabled = config.EarlyFetchEnabled ?? DefaultCacheSettings.EarlyFetchEnabled;
            OnError = config.OnError ?? DefaultCacheSettings.OnError;
        }
    }
}