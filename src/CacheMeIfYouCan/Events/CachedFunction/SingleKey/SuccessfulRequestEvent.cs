using System;
using CacheMeIfYouCan.Internal.CachedFunctions;

namespace CacheMeIfYouCan.Events.CachedFunction.SingleKey
{
    public readonly struct SuccessfulRequestEvent<TParams, TKey, TValue>
    {
        internal SuccessfulRequestEvent(
            TParams parameters,
            TKey key,
            TValue value,
            DateTime start,
            TimeSpan duration,
            SingleKeyCacheGetStats cacheStats)
        {
            Parameters = parameters;
            Key = key;
            Value = value;
            Start = start;
            Duration = duration;
            CacheStats = cacheStats;
        }

        public TParams Parameters { get; }
        public TKey Key { get; }
        public TValue Value { get; }
        public DateTime Start { get; }
        public TimeSpan Duration { get; }
        public SingleKeyCacheGetStats CacheStats { get; }
    }
}