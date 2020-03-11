using System;
using System.Collections.Generic;

namespace CacheMeIfYouCan.Events.CachedFunction.EnumerableKeys
{
    public readonly struct SuccessfulRequestEvent<TKey, TValue>
    {
        internal SuccessfulRequestEvent(in SuccessfulRequestEvent_MultiParam<Unit, TKey, TValue> result)
        {
            Keys = result.Keys;
            Values = result.Values;
            Start = result.Start;
            Duration = result.Duration;
            CacheHits = result.CacheHits;
        }
        
        public IReadOnlyCollection<TKey> Keys { get; }
        public Dictionary<TKey, TValue> Values { get; }
        public DateTime Start { get; }
        public TimeSpan Duration { get; }
        public int CacheHits { get; }
        public int CacheMisses => Keys.Count - CacheHits;
    }
    
    public readonly struct SuccessfulRequestEvent_1ExtraParam<TParam, TKey, TValue>
    {
        internal SuccessfulRequestEvent_1ExtraParam(in SuccessfulRequestEvent_MultiParam<TParam, TKey, TValue> result)
        {
            Parameter = result.Parameters;
            Keys = result.Keys;
            Values = result.Values;
            Start = result.Start;
            Duration = result.Duration;
            CacheHits = result.CacheHits;
        }
        
        public TParam Parameter { get; }
        public IReadOnlyCollection<TKey> Keys { get; }
        public Dictionary<TKey, TValue> Values { get; }
        public DateTime Start { get; }
        public TimeSpan Duration { get; }
        public int CacheHits { get; }
        public int CacheMisses => Keys.Count - CacheHits;
    }

    public readonly struct SuccessfulRequestEvent_MultiParam<TParams, TKey, TValue>
    {
        internal SuccessfulRequestEvent_MultiParam(
            TParams parameters,
            IReadOnlyCollection<TKey> keys,
            Dictionary<TKey, TValue> values,
            DateTime start,
            TimeSpan duration,
            int cacheHits)
        {
            Parameters = parameters;
            Keys = keys;
            Values = values;
            Start = start;
            Duration = duration;
            CacheHits = cacheHits;
        }

        public TParams Parameters { get; }
        public IReadOnlyCollection<TKey> Keys { get; }
        public Dictionary<TKey, TValue> Values { get; }
        public DateTime Start { get; }
        public TimeSpan Duration { get; }
        public int CacheHits { get; }
        public int CacheMisses => Keys.Count - CacheHits;
    }
}