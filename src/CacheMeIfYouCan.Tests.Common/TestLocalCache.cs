using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CacheMeIfYouCan.Tests.Common
{
    public class TestLocalCache<TK, TV> : ILocalCache<TK, TV>
    {
        public readonly ConcurrentDictionary<TK, Tuple<TV, DateTimeOffset>> Values = new ConcurrentDictionary<TK, Tuple<TV, DateTimeOffset>>();
        private readonly TimeSpan? _delay;
        private readonly Func<bool> _error;

        public TestLocalCache(TimeSpan? delay = null, Func<bool> error = null, string cacheName = "test-local-name")
        {
            _delay = delay;
            _error = error;

            CacheName = cacheName;
        }

        public string CacheName { get; }
        public string CacheType { get; } = "test-local";
        public bool RequiresKeySerializer { get; } = false;
        public bool RequiresKeyComparer { get; } = true;
        
        public void Dispose() { }

        public GetFromCacheResult<TK, TV> Get(Key<TK> key)
        {
            return Get(new[] { key }).SingleOrDefault();
        }

        public void Set(Key<TK> key, TV value, TimeSpan timeToLive)
        {
            Set(new[] { new KeyValuePair<Key<TK>, TV>(key, value) }, timeToLive);
        }

        public IList<GetFromCacheResult<TK, TV>> Get(IReadOnlyCollection<Key<TK>> keys)
        {
            if (_delay.HasValue)
                Task.Delay(_delay.Value).Wait();

            if (_error?.Invoke() ?? false)
                throw new Exception();
            
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

        public void Set(IReadOnlyCollection<KeyValuePair<Key<TK>, TV>> values, TimeSpan timeToLive)
        {
            if (_delay.HasValue)
                Task.Delay(_delay.Value).Wait();

            if (_error?.Invoke() ?? false)
                throw new Exception();
            
            if (timeToLive <= TimeSpan.Zero)
                return;
            
            var expiry = DateTimeOffset.UtcNow + timeToLive;

            foreach (var kv in values)
                Values[kv.Key] = Tuple.Create(kv.Value, expiry);
        }

        public bool Remove(Key<TK> key)
        {
            if (_delay.HasValue)
                Task.Delay(_delay.Value).Wait();

            if (_error?.Invoke() ?? false)
                throw new Exception();
            
            return Values.TryRemove(key, out _);
        }
    }
}