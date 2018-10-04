using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CacheMeIfYouCan.Caches;

namespace CacheMeIfYouCan.Tests
{
    public class TestCache<TK, TV> : ICache<TK, TV>
    {
        private readonly Func<TV, string> _serializer;
        private readonly Func<string, TV> _deserializer;
        private readonly Action<Key<string>> _removeKeyFromLocalCacheAction;
        private readonly TimeSpan? _delay;
        public readonly ConcurrentDictionary<string, Tuple<string, DateTimeOffset>> Values = new ConcurrentDictionary<string, Tuple<string, DateTimeOffset>>();
        public readonly string CacheType = "test";

        public TestCache(Func<TV, string> serializer, Func<string, TV> deserializer, Action<Key<string>> removeKeyFromLocalCacheAction = null, TimeSpan? delay = null)
        {
            _serializer = serializer;
            _deserializer = deserializer;
            _removeKeyFromLocalCacheAction = removeKeyFromLocalCacheAction;
            _delay = delay;
        }

        public async Task<IList<GetFromCacheResult<TK, TV>>> Get(ICollection<Key<TK>> keys)
        {
            if (_delay.HasValue)
                await Task.Delay(_delay.Value);

            return keys
                .Select(GetSingle)
                .ToArray();
        }

        public async Task Set(ICollection<KeyValuePair<Key<TK>, TV>> values, TimeSpan timeToLive)
        {
            if (_delay.HasValue)
                await Task.Delay(_delay.Value);

            if (timeToLive <= TimeSpan.Zero)
                return;
            
            var expiry = DateTimeOffset.UtcNow + timeToLive;
            
            foreach (var kv in values)
                Values[kv.Key.AsString] = Tuple.Create(_serializer(kv.Value), expiry);
        }

        public void OnKeyChangedRemotely(string key)
        {
            Values.TryRemove(key, out _);
            _removeKeyFromLocalCacheAction?.Invoke(new Key<string>(key, new Lazy<string>(key)));
        }

        private GetFromCacheResult<TK, TV> GetSingle(Key<TK> key)
        {
            var result = GetFromCacheResult<TK, TV>.NotFound(key);

            if (Values.TryGetValue(key.AsString, out var item))
            {
                var timeToLive = item.Item2 - DateTimeOffset.UtcNow;

                if (timeToLive < TimeSpan.Zero)
                    Values.TryRemove(key.AsString, out _);
                else
                    result = new GetFromCacheResult<TK, TV>(key, _deserializer(item.Item1), timeToLive, CacheType);
            }

            return result;
        }
    }
}
