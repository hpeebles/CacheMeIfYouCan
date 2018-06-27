using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace CacheMeIfYouCan.Tests
{
    public class TestCache<T> : ICache<T>
    {
        private readonly Func<T, string> _serializer;
        private readonly Func<string, T> _deserializer;
        public readonly ConcurrentDictionary<string, Tuple<string, DateTimeOffset>> Values = new ConcurrentDictionary<string, Tuple<string, DateTimeOffset>>();

        public TestCache(Func<T, string> serializer, Func<string, T> deserializer)
        {
            _serializer = serializer;
            _deserializer = deserializer;
        }

        public Task<GetFromCacheResult<T>> Get(string key)
        {
            var result = GetFromCacheResult<T>.NotFound;

            if (Values.TryGetValue(key, out var item))
            {
                var timeToLive = item.Item2 - DateTimeOffset.UtcNow;

                if (timeToLive < TimeSpan.Zero)
                    Values.TryRemove(key, out _);
                else
                    result = new GetFromCacheResult<T>(_deserializer(item.Item1), timeToLive, "test");
            }

            return Task.FromResult(result);
        }

        public Task Set(string key, T value, TimeSpan timeToLive)
        {
            if (timeToLive > TimeSpan.Zero)
            {
                var expiry = DateTimeOffset.UtcNow + timeToLive;

                Values[key] = Tuple.Create(_serializer(value), expiry);
            }

            return Task.CompletedTask;
        }
    }
}
