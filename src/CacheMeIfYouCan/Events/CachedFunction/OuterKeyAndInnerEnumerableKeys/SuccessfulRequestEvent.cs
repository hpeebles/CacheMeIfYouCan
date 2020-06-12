using System;
using System.Collections.Generic;

namespace CacheMeIfYouCan.Events.CachedFunction.OuterKeyAndInnerEnumerableKeys
{
    public sealed class SuccessfulRequestEvent<TParams, TOuterKey, TInnerKey, TValue>
    {
        internal SuccessfulRequestEvent(
            TParams parameters,
            TOuterKey outerKey,
            ReadOnlyMemory<TInnerKey> innerKeys,
            Dictionary<TInnerKey, TValue> values,
            DateTime start,
            TimeSpan duration,
            CacheGetManyStats cacheStats,
            int countExcluded)
        {
            Parameters = parameters;
            OuterKey = outerKey;
            InnerKeys = innerKeys;
            Values = values;
            Start = start;
            Duration = duration;
            CacheStats = cacheStats;
            CountExcluded = countExcluded;
        }
        
        public TParams Parameters { get; }
        public TOuterKey OuterKey { get; }
        public ReadOnlyMemory<TInnerKey> InnerKeys { get; }
        public Dictionary<TInnerKey, TValue> Values { get; }
        public DateTime Start { get; }
        public TimeSpan Duration { get; }
        public CacheGetManyStats CacheStats { get; }
        public int CountExcluded { get; }
    }
}