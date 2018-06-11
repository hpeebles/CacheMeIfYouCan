using System;

namespace CacheMeIfYouCan.Internal
{
    internal class CacheConfig
    {
        public readonly TimeSpan TimeToLive;
        public readonly int MemoryCacheMaxSizeMB;
        public readonly int MaxConcurrentFetches;
        public readonly bool EarlyFetchEnabled;
        public readonly Action<FunctionCacheErrorEvent> OnError;
        
        internal CacheConfig(CacheConfigOverrides config)
        {
            TimeToLive = config.TimeToLive ?? DefaultCacheSettings.TimeToLive;
            MemoryCacheMaxSizeMB = config.MemoryCacheMaxSizeMB ?? DefaultCacheSettings.MemoryCacheMaxSizeMB;
            MaxConcurrentFetches = config.MaxConcurrentFetches ?? DefaultCacheSettings.MaxConcurrentFetches;
            EarlyFetchEnabled = config.EarlyFetchEnabled ?? DefaultCacheSettings.EarlyFetchEnabled;
            OnError = config.OnError ?? DefaultCacheSettings.OnError;
        }
    }
}