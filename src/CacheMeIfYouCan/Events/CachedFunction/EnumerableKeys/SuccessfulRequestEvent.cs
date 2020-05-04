using System;
using System.Collections.Generic;

namespace CacheMeIfYouCan.Events.CachedFunction.EnumerableKeys
{
    public readonly struct SuccessfulRequestEvent<TParams, TKey, TValue>
    {
        internal SuccessfulRequestEvent(
            TParams parameters,
            ReadOnlyMemory<TKey> keys,
            Dictionary<TKey, TValue> values,
            DateTime start,
            TimeSpan duration,
            CacheGetManyStats cacheStats)
        {
            Parameters = parameters;
            Keys = keys;
            Values = values;
            Start = start;
            Duration = duration;
            CacheStats = cacheStats;
        }
        
        public TParams Parameters { get; }
        public ReadOnlyMemory<TKey> Keys { get; }
        public Dictionary<TKey, TValue> Values { get; }
        public DateTime Start { get; }
        public TimeSpan Duration { get; }
        public CacheGetManyStats CacheStats { get; }
    }
}