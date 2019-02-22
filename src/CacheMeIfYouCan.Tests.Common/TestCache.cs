using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace CacheMeIfYouCan.Tests.Common
{
    public class TestCache<TK, TV> : IDistributedCache<TK, TV>, INotifyKeyChanges<TK>
    {
        private readonly Func<TV, object> _serializer;
        private readonly Func<object, TV> _deserializer;
        private readonly TimeSpan? _delay;
        private readonly Func<bool> _error;
        private readonly Subject<Key<TK>> _keyChanges;
        public readonly ConcurrentDictionary<string, Tuple<object, DateTimeOffset>> Values = new ConcurrentDictionary<string, Tuple<object, DateTimeOffset>>();
        
        public TestCache(
            Func<TV, string> serializer = null,
            Func<string, TV> deserializer = null,
            Func<TV, byte[]> byteSerializer = null,
            Func<byte[], TV> byteDeserializer = null,
            TimeSpan? delay = null,
            Func<bool> error = null,
            string cacheName = "test-name")
        {
            _serializer = v => serializer == null ? (object)byteSerializer(v) : serializer(v);
            _deserializer = o => serializer == null ? byteDeserializer((byte[])o) : deserializer((string)o);
            _delay = delay;
            _error = error;
            _keyChanges = new Subject<Key<TK>>();

            CacheName = cacheName;
        }

        public string CacheName { get; }
        public string CacheType { get; } = "test";
        
        public void Dispose() { }
        
        public async Task<GetFromCacheResult<TK, TV>> Get(Key<TK> key)
        {
            var result = await Get(new[] { key });

            return result.SingleOrDefault();
        }

        public async Task Set(Key<TK> key, TV value, TimeSpan timeToLive)
        {
            await Set(new[] { new KeyValuePair<Key<TK>, TV>(key, value) }, timeToLive);
        }

        public async Task<IList<GetFromCacheResult<TK, TV>>> Get(ICollection<Key<TK>> keys)
        {
            if (_delay.HasValue)
                await Task.Delay(_delay.Value);

            if (_error?.Invoke() ?? false)
                throw new Exception();
            
            var results = new List<GetFromCacheResult<TK, TV>>();

            foreach (var key in keys)
            {
                if (!Values.TryGetValue(key.AsString, out var item))
                    continue;
                
                var timeToLive = item.Item2 - DateTimeOffset.UtcNow;

                if (timeToLive < TimeSpan.Zero)
                    Values.TryRemove(key.AsString, out _);
                else
                    results.Add(new GetFromCacheResult<TK, TV>(key, _deserializer(item.Item1), timeToLive, CacheType));
            }

            return results;
        }

        public async Task Set(ICollection<KeyValuePair<Key<TK>, TV>> values, TimeSpan timeToLive)
        {
            if (_delay.HasValue)
                await Task.Delay(_delay.Value);

            if (_error?.Invoke() ?? false)
                throw new Exception();

            if (timeToLive <= TimeSpan.Zero)
                return;
            
            var expiry = DateTimeOffset.UtcNow + timeToLive;
            
            foreach (var kv in values)
                Values[kv.Key.AsString] = Tuple.Create(_serializer(kv.Value), expiry);
        }

        public async Task<bool> Remove(Key<TK> key)
        {
            if (_delay.HasValue)
                await Task.Delay(_delay.Value);

            if (_error?.Invoke() ?? false)
                throw new Exception();

            return Values.TryRemove(key.AsString, out _);
        }

        public void NotifyChanged(Key<TK> key)
        {
            Remove(key).Wait();
            
            _keyChanges.OnNext(key);
        }

        public bool NotifyKeyChangesEnabled { get; } = true;
        public IObservable<Key<TK>> KeyChanges => _keyChanges.AsObservable();
    }
}
