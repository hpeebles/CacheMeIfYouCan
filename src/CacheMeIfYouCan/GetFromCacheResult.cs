using System;

namespace CacheMeIfYouCan
{
    public readonly struct GetFromCacheResult<TK, TV>
    {
        public Key<TK> Key { get; }
        public TV Value { get; }
        public TimeSpan TimeToLive { get; }
        public string CacheType { get; }

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