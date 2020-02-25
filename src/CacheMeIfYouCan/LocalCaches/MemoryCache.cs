using System;
using System.Collections.Generic;
using System.Runtime.Caching;
using CacheMeIfYouCan.Internal;

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

            if (fromCache is null)
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

        public int GetMany(IReadOnlyCollection<TKey> keys, Memory<KeyValuePair<TKey, TValue>> destination)
        {
            if (destination.Length < keys.Count) 
                throw Errors.LocalCache_DestinationArrayTooSmall(nameof(destination));

            var span = destination.Span;
            
            var countFound = 0;
            foreach (var key in keys)
            {
                var fromCache = _memoryCache.Get(_keySerializer(key));

                if (fromCache != null)
                    span[countFound++] = new KeyValuePair<TKey, TValue>(key, (TValue)fromCache); 
            }

            return countFound;
        }

        public void SetMany(IReadOnlyCollection<KeyValuePair<TKey, TValue>> values, TimeSpan timeToLive)
        {
            var expirationDate = DateTimeOffset.UtcNow.Add(timeToLive);
            
            foreach (var kv in values)
                _memoryCache.Set(_keySerializer(kv.Key), kv.Value, expirationDate);
        }

        public bool TryRemove(TKey key, out TValue value)
        {
            var valueRemoved = _memoryCache.Remove(_keySerializer(key));

            if (valueRemoved is TValue returnValue)
            {
                value = returnValue;
                return true;
            }

            value = default;
            return false;
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
        
        public int GetMany(TOuterKey outerKey, IReadOnlyCollection<TInnerKey> innerKeys, Memory<KeyValuePair<TInnerKey, TValue>> destination)
        {
            if (destination.Length < innerKeys.Count)
                throw Errors.LocalCache_DestinationArrayTooSmall(nameof(destination));

            var outerKeyString = _outerKeySerializer(outerKey);

            var span = destination.Span;
            
            var countFound = 0;
            foreach (var key in innerKeys)
            {
                var fromCache = _memoryCache.Get(outerKeyString + _innerKeySerializer(key));

                if (fromCache != null)
                    span[countFound++] = new KeyValuePair<TInnerKey, TValue>(key, (TValue)fromCache); 
            }

            return countFound;
        }

        public void SetMany(TOuterKey outerKey, IReadOnlyCollection<KeyValuePair<TInnerKey, TValue>> values, TimeSpan timeToLive)
        {
            var outerKeyString = _outerKeySerializer(outerKey);

            var expirationDate = DateTimeOffset.UtcNow.Add(timeToLive);
            
            foreach (var kv in values)
                _memoryCache.Set(outerKeyString + _innerKeySerializer(kv.Key), kv.Value, expirationDate);
        }

        public void SetManyWithVaryingTimesToLive(TOuterKey outerKey, Memory<KeyValuePair<TInnerKey, ValueAndTimeToLive<TValue>>> values)
        {
            var outerKeyString = _outerKeySerializer(outerKey);

            foreach (var kv in values.Span)
            {
                var expirationDate = DateTimeOffset.UtcNow.Add(kv.Value.TimeToLive);

                _memoryCache.Set(outerKeyString + _innerKeySerializer(kv.Key), kv.Value.Value, expirationDate);
            }
        }

        public bool TryRemove(TOuterKey outerKey, TInnerKey innerKey, out TValue value)
        {
            var outerKeyString = _outerKeySerializer(outerKey);
            var innerKeyString = _innerKeySerializer(innerKey);

            var valueRemoved = _memoryCache.Remove(outerKeyString + innerKeyString);

            if (valueRemoved is TValue returnValue)
            {
                value = returnValue;
                return true;
            }

            value = default;
            return false;
        }
    }
}