using System;
using System.Collections.Generic;

namespace CacheMeIfYouCan.Events.CachedFunction.EnumerableKeys
{
    public sealed class SuccessfulRequestEvent<TParams, TKey, TValue>
    {
        internal SuccessfulRequestEvent(
            TParams parameters,
            ReadOnlyMemory<TKey> keys,
            Dictionary<TKey, TValue> values,
            DateTime start,
            TimeSpan duration,
            CacheGetManyStats cacheStats,
            int countExcluded)
        {
            Parameters = parameters;
            Keys = keys;
            Values = values;
            Start = start;
            Duration = duration;
            CacheStats = cacheStats;
            CountExcluded = countExcluded;
        }
        
        public TParams Parameters { get; }
        public ReadOnlyMemory<TKey> Keys { get; }
        public Dictionary<TKey, TValue> Values { get; }
        public DateTime Start { get; }
        public TimeSpan Duration { get; }
        public CacheGetManyStats CacheStats { get; }
        public int CountExcluded { get; }
    }
}