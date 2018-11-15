using System;

namespace CacheMeIfYouCan
{
    public struct GetFromCacheResult<TK, TV>
    {
        public readonly Key<TK> Key;
        public readonly TV Value;
        public readonly TimeSpan TimeToLive;
        public readonly string CacheType;

        public bool Success => Key.AsObject != null;

        public GetFromCacheResult(Key<TK> key, TV value, TimeSpan timeToLive, string cacheType)
        {
            Key = key;
            Value = value;
            TimeToLive = timeToLive;
            CacheType = cacheType;
        }

        public static implicit operator TV(GetFromCacheResult<TK, TV> result)
        {
            return result.Value;
        }
    }
}