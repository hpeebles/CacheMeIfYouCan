using System;
using System.Collections.Generic;

namespace CacheMeIfYouCan.Events.CachedFunction.OuterKeyAndInnerEnumerableKeys
{
    public readonly struct SuccessfulRequestEvent<TParams, TOuterKey, TInnerKey, TValue>
    {
        internal SuccessfulRequestEvent(
            TParams parameters,
            TOuterKey outerKey,
            ReadOnlyMemory<TInnerKey> innerKeys,
            Dictionary<TInnerKey, TValue> values,
            DateTime start,
            TimeSpan duration,
            CacheGetManyStats cacheStats)
        {
            Parameters = parameters;
            OuterKey = outerKey;
            InnerKeys = innerKeys;
            Values = values;
            Start = start;
            Duration = duration;
            CacheStats = cacheStats;
        }
        
        public TParams Parameters { get; }
        public TOuterKey OuterKey { get; }
        public ReadOnlyMemory<TInnerKey> InnerKeys { get; }
        public Dictionary<TInnerKey, TValue> Values { get; }
        public DateTime Start { get; }
        public TimeSpan Duration { get; }
        public CacheGetManyStats CacheStats { get; }
    }
}