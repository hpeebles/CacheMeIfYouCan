using System;

namespace CacheMeIfYouCan.Caches
{
    public struct GetFromCacheResult<TK, TV>
    {
        public readonly bool Success;
        public readonly Key<TK> Key;
        public readonly TV Value;
        public readonly TimeSpan TimeToLive;
        public readonly string CacheType;

        public GetFromCacheResult(Key<TK> key, TV value, TimeSpan timeToLive, string cacheType)
        {
            Success = true;
            Key = key;
            Value = value;
            TimeToLive = timeToLive;
            CacheType = cacheType;
        }

        public static GetFromCacheResult<TK, TV> NotFound(Key<TK> key)
        {
            return new GetFromCacheResult<TK, TV>(key);
        }

        private GetFromCacheResult(Key<TK> key)
        {
            Success = false;
            Key = key;
            Value = default(TV);
            CacheType = null;
        }
    }
}