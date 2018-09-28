using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using CacheMeIfYouCan.Caches;

namespace CacheMeIfYouCan.Tests
{
    public class TestCache<TK, TV> : ICache<TK, TV>
    {
        private readonly Func<TV, string> _serializer;
        private readonly Func<string, TV> _deserializer;
        private readonly Action<Key<string>> _removeKeyFromLocalCacheAction;
        public readonly ConcurrentDictionary<string, Tuple<string, DateTimeOffset>> Values = new ConcurrentDictionary<string, Tuple<string, DateTimeOffset>>();
        public readonly string CacheType = "test";

        public TestCache(Func<TV, string> serializer, Func<string, TV> deserializer, Action<Key<string>> removeKeyFromLocalCacheAction = null)
        {
            _serializer = serializer;
            _deserializer = deserializer;
            _removeKeyFromLocalCacheAction = removeKeyFromLocalCacheAction;
        }

        public Task<GetFromCacheResult<TV>> Get(Key<TK> key)
        {
            var result = GetFromCacheResult<TV>.NotFound;

            if (Values.TryGetValue(key.AsString.Value, out var item))
            {
                var timeToLive = item.Item2 - DateTimeOffset.UtcNow;

                if (timeToLive < TimeSpan.Zero)
                    Values.TryRemove(key.AsString.Value, out _);
                else
                    result = new GetFromCacheResult<TV>(_deserializer(item.Item1), timeToLive, CacheType);
            }

            return Task.FromResult(result);
        }

        public Task Set(Key<TK> key, TV value, TimeSpan timeToLive)
        {
            if (timeToLive > TimeSpan.Zero)
            {
                var expiry = DateTimeOffset.UtcNow + timeToLive;

                Values[key.AsString.Value] = Tuple.Create(_serializer(value), expiry);
            }

            return Task.CompletedTask;
        }

        public void OnKeyChangedRemotely(string key)
        {
            Values.TryRemove(key, out _);
            _removeKeyFromLocalCacheAction?.Invoke(new Key<string>(key, new Lazy<string>(key)));
        }
    }
}
