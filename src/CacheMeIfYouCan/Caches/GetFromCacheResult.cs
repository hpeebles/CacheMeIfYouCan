using System;

namespace CacheMeIfYouCan.Caches
{
    public struct GetFromCacheResult<TK, TV>
    {
        public readonly Key<TK> Key;
        public readonly TV Value;
        public readonly TimeSpan TimeToLive;
        public readonly string CacheType;

        public GetFromCacheResult(Key<TK> key, TV value, TimeSpan timeToLive, string cacheType)
        {
            Key = key;
            Value = value;
            TimeToLive = timeToLive;
            CacheType = cacheType;
        }
    }
}