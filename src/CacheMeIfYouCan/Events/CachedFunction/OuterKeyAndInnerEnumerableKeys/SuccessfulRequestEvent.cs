using System;
using System.Collections.Generic;

namespace CacheMeIfYouCan.Events.CachedFunction.OuterKeyAndInnerEnumerableKeys
{
    public readonly struct SuccessfulRequestEvent<TParams, TOuterKey, TInnerKey, TValue>
    {
        internal SuccessfulRequestEvent(
            TParams parameters,
            TOuterKey outerKey,
            IReadOnlyCollection<TInnerKey> innerKeys,
            Dictionary<TInnerKey, TValue> values,
            DateTime start,
            TimeSpan duration,
            int cacheHits)
        {
            Parameters = parameters;
            OuterKey = outerKey;
            InnerKeys = innerKeys;
            Values = values;
            Start = start;
            Duration = duration;
            CacheHits = cacheHits;
        }
        
        public TParams Parameters { get; }
        public TOuterKey OuterKey { get; }
        public IReadOnlyCollection<TInnerKey> InnerKeys { get; }
        public Dictionary<TInnerKey, TValue> Values { get; }
        public DateTime Start { get; }
        public TimeSpan Duration { get; }
        public int CacheHits { get; }
        public int CacheMisses => InnerKeys.Count - CacheHits;
    }
}