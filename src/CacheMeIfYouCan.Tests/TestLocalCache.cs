using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using CacheMeIfYouCan.Caches;

namespace CacheMeIfYouCan.Tests
{
    public class TestLocalCache<TK, TV> : ILocalCache<TK, TV>
    {
        public readonly ConcurrentDictionary<TK, Tuple<TV, DateTimeOffset>> Values = new ConcurrentDictionary<TK, Tuple<TV, DateTimeOffset>>();

        public string CacheName { get; } = "test-local-name";
        public string CacheType { get; } = "test-local";
        
        public GetFromCacheResult<TK, TV> Get(Key<TK> key)
        {
            return Get(new[] { key }).SingleOrDefault();
        }

        public void Set(Key<TK> key, TV value, TimeSpan timeToLive)
        {
            Set(new[] { new KeyValuePair<Key<TK>, TV>(key, value) }, timeToLive);
        }

        public IList<GetFromCacheResult<TK, TV>> Get(ICollection<Key<TK>> keys)
        {
            var results = new List<GetFromCacheResult<TK, TV>>();

            foreach (var key in keys)
            {
                if (!Values.TryGetValue(key, out var item))
                    continue;
                
                var timeToLive = item.Item2 - DateTimeOffset.UtcNow;

                if (timeToLive < TimeSpan.Zero)
                    Values.TryRemove(key, out _);
                else
                    results.Add(new GetFromCacheResult<TK, TV>(key, item.Item1, timeToLive, CacheType));
            }

            return results;
        }

        public void Set(ICollection<KeyValuePair<Key<TK>, TV>> values, TimeSpan timeToLive)
        {
            if (timeToLive <= TimeSpan.Zero)
                return;
            
            var expiry = DateTimeOffset.UtcNow + timeToLive;

            foreach (var kv in values)
                Values[kv.Key] = Tuple.Create(kv.Value, expiry);
        }

        public void Remove(Key<TK> key)
        {
            Values.TryRemove(key, out _);
        }
    }
}