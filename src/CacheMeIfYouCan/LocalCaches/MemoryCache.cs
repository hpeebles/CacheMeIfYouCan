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
    
    public sealed class MemoryCache<TOuterKey, TInnerKey, TValue> : ILocalCache<TOuterKey, TInnerKey, TValue>
    {
        private readonly MemoryCache _memoryCache;
        private readonly Func<TOuterKey, string> _outerKeySerializer;
        private readonly Func<TInnerKey, string> _innerKeySerializer;

        public MemoryCache(Func<TOuterKey, string> outerKeySerializer, Func<TInnerKey, string> innerKeySerializer)
        {
            _memoryCache = new MemoryCache(Guid.NewGuid().ToString());
            _outerKeySerializer = outerKeySerializer;
            _innerKeySerializer = innerKeySerializer;
        }
        
        public IReadOnlyCollection<KeyValuePair<TInnerKey, TValue>> GetMany(TOuterKey outerKey, IReadOnlyCollection<TInnerKey> innerKeys)
        {
            var outerKeyString = _outerKeySerializer(outerKey);
            
            var values = new List<KeyValuePair<TInnerKey, TValue>>();

            foreach (var key in innerKeys)
            {
                var fromCache = _memoryCache.Get(outerKeyString + _innerKeySerializer(key));

                if (fromCache != null)
                    values.Add(new KeyValuePair<TInnerKey, TValue>(key, (TValue)fromCache)); 
            }

            return values;
        }

        public void SetMany(TOuterKey outerKey, IReadOnlyCollection<KeyValuePair<TInnerKey, TValue>> values, TimeSpan timeToLive)
        {
            var outerKeyString = _outerKeySerializer(outerKey);

            var expirationDate = DateTimeOffset.UtcNow.Add(timeToLive);
            
            foreach (var kv in values)
                _memoryCache.Set(outerKeyString + _innerKeySerializer(kv.Key), kv.Value, expirationDate);
        }

        public void SetMany(TOuterKey outerKey, IReadOnlyCollection<KeyValuePair<TInnerKey, ValueAndTimeToLive<TValue>>> values)
        {
            var outerKeyString = _outerKeySerializer(outerKey);

            foreach (var kv in values)
            {
                var expirationDate = DateTimeOffset.UtcNow.Add(kv.Value.TimeToLive);

                _memoryCache.Set(outerKeyString + _innerKeySerializer(kv.Key), kv.Value.Value, expirationDate);
            }
        }
    }
}