using System;
using System.Collections.Concurrent;
using CacheMeIfYouCan.Caches;

namespace CacheMeIfYouCan.Tests
{
    public class TestLocalCache<TK, TV> : ILocalCache<TK, TV>
    {
        public readonly ConcurrentDictionary<TK, Tuple<TV, DateTimeOffset>> Values = new ConcurrentDictionary<TK, Tuple<TV, DateTimeOffset>>();
        public readonly string CacheType = "test-local";
        
        public GetFromCacheResult<TK, TV> Get(Key<TK> key)
        {
            var result = GetFromCacheResult<TK, TV>.NotFound(key);

            if (Values.TryGetValue(key, out var item))
            {
                var timeToLive = item.Item2 - DateTimeOffset.UtcNow;

                if (timeToLive < TimeSpan.Zero)
                    Values.TryRemove(key, out _);
                else
                    result = new GetFromCacheResult<TK, TV>(key, item.Item1, timeToLive, CacheType);
            }

            return result;
        }

        public void Set(Key<TK> key, TV value, TimeSpan timeToLive)
        {
            if (timeToLive <= TimeSpan.Zero)
                return;
            
            var expiry = DateTimeOffset.UtcNow + timeToLive;

            Values[key] = Tuple.Create(value, expiry);
        }

        public void Remove(Key<TK> key)
        {
            Values.TryRemove(key, out _);
        }
    }
}