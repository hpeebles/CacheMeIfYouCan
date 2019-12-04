using System;
using System.Collections.Generic;
using System.Runtime.Caching;

namespace CacheMeIfYouCan.LocalCaches
{
    public sealed class MemoryCache<TKey, TValue> : ILocalCache<TKey, TValue>
    {
        private readonly MemoryCache _memoryCache;
        private readonly Func<TKey, string> _keySerializer;

        public MemoryCache(Func<TKey, string> keySerializer)
        {
            _memoryCache = new MemoryCache(Guid.NewGuid().ToString());
            _keySerializer = keySerializer;
        }

        public bool TryGet(TKey key, out TValue value)
        {
            var fromCache = _memoryCache.Get(_keySerializer(key));

            if (fromCache == null)
            {
                value = default;
                return false;
            }

            value = (TValue)fromCache;
            return true;
        }

        public void Set(TKey key, TValue value, TimeSpan timeToLive)
        {
            _memoryCache.Set(_keySerializer(key), value, DateTimeOffset.UtcNow.Add(timeToLive));
        }

        public IReadOnlyCollection<KeyValuePair<TKey, TValue>> GetMany(IReadOnlyCollection<TKey> keys)
        {
            var values = new List<KeyValuePair<TKey, TValue>>();

            foreach (var key in keys)
            {
                var fromCache = _memoryCache.Get(_keySerializer(key));

                if (fromCache != null)
                    values.Add(new KeyValuePair<TKey, TValue>(key, (TValue)fromCache)); 
            }

            return values;
        }

        public void SetMany(IReadOnlyCollection<KeyValuePair<TKey, TValue>> values, TimeSpan timeToLive)
        {
            var expirationDate = DateTimeOffset.UtcNow.Add(timeToLive);
            
            foreach (var kv in values)
                _memoryCache.Set(_keySerializer(kv.Key), kv.Value, expirationDate);
        }
    }
}