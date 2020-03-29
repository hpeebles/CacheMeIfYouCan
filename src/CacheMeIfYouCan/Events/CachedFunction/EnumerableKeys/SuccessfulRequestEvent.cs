﻿using System;
using System.Collections.Generic;

namespace CacheMeIfYouCan.Events.CachedFunction.EnumerableKeys
{
    public readonly struct SuccessfulRequestEvent<TParams, TKey, TValue>
    {
        internal SuccessfulRequestEvent(
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