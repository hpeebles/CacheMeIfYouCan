using System;
using System.Collections.Generic;
using System.Runtime.Caching;
using CacheMeIfYouCan.Internal;

namespace CacheMeIfYouCan
{
    public sealed class MemoryCache<TKey, TValue> : ILocalCache<TKey, TValue>
    {
        private MemoryCache _memoryCache;
        private readonly Func<TKey, string> _keySerializer;

        public MemoryCache(Func<TKey, string> keySerializer = null)
        {
            _memoryCache = new MemoryCache(Guid.NewGuid().ToString());
            _keySerializer = keySerializer ?? (k => k.ToString());
        }

        public int Count => (int)_memoryCache.GetCount();

        public void Clear()
        {
            var memoryCache = _memoryCache;
            _memoryCache = new MemoryCache(Guid.NewGuid().ToString());
            memoryCache.Dispose();
        }

        public bool TryGet(TKey key, out TValue value)
        {
            var fromCache = _memoryCache.Get(_keySerializer(key));

            if (fromCache is null)
            {
                value = default;
                return false;
            }

            value = ConvertValue(fromCache);
            return true;
        }

        public void Set(TKey key, TValue value, TimeSpan timeToLive)
        {
            _memoryCache.Set(
                _keySerializer(key),
                (object)value ?? NullObj.Instance,
                DateTimeOffset.UtcNow.Add(timeToLive));
        }

        public int GetMany(ReadOnlySpan<TKey> keys, Span<KeyValuePair<TKey, TValue>> destination)
        {
            if (destination.Length < keys.Length) 
                throw Errors.LocalCache_DestinationArrayTooSmall(nameof(destination));
            
            var countFound = 0;
            foreach (var key in keys)
            {
                var fromCache = _memoryCache.Get(_keySerializer(key));

                if (fromCache != null)
                    destination[countFound++] = new KeyValuePair<TKey, TValue>(key, ConvertValue(fromCache)); 
            }

            return countFound;
        }

        public void SetMany(ReadOnlySpan<KeyValuePair<TKey, TValue>> values, TimeSpan timeToLive)
        {
            var expirationDate = DateTimeOffset.UtcNow.Add(timeToLive);
            
            foreach (var kv in values)
                _memoryCache.Set(_keySerializer(kv.Key), (object)kv.Value ?? NullObj.Instance, expirationDate);
        }

        public bool TryRemove(TKey key, out TValue value)
        {
            var valueRemoved = _memoryCache.Remove(_keySerializer(key));

            switch (valueRemoved)
            {
                case TValue returnValue:
                    value = returnValue;
                    return true;
                case NullObj _:
                    value = default;
                    return true;
                default:
                    value = default;
                    return false;
            }
        }
    
        private static TValue ConvertValue(object value)
        {
            return value is TValue v
                ? v
                : default;
        }
    }

    public sealed class MemoryCache<TOuterKey, TInnerKey, TValue> : ILocalCache<TOuterKey, TInnerKey, TValue>
    {
        private MemoryCache _memoryCache;
        private readonly Func<TOuterKey, string> _outerKeySerializer;
        private readonly Func<TInnerKey, string> _innerKeySerializer;
        
        public MemoryCache(
            Func<TOuterKey, string> outerKeySerializer = null,
            Func<TInnerKey, string> innerKeySerializer = null)
        {
            _memoryCache = new MemoryCache(Guid.NewGuid().ToString());
            _outerKeySerializer = outerKeySerializer ?? (k => k.ToString());
            _innerKeySerializer = innerKeySerializer ?? (k => k.ToString());
        }

        public int Count => (int)_memoryCache.GetCount();
        
        public void Clear()
        {
            var memoryCache = _memoryCache;
            _memoryCache = new MemoryCache(Guid.NewGuid().ToString());
            memoryCache.Dispose();
        }
        
        public int GetMany(TOuterKey outerKey, ReadOnlySpan<TInnerKey> innerKeys, Span<KeyValuePair<TInnerKey, TValue>> destination)
        {
            if (destination.Length < innerKeys.Length)
                throw Errors.LocalCache_DestinationArrayTooSmall(nameof(destination));

            var outerKeyString = _outerKeySerializer(outerKey);
            
            var countFound = 0;
            foreach (var key in innerKeys)
            {
                var fromCache = _memoryCache.Get(outerKeyString + _innerKeySerializer(key));

                if (fromCache != null)
                    destination[countFound++] = new KeyValuePair<TInnerKey, TValue>(key, ConvertValue(fromCache));
            }

            return countFound;
        }

        public void SetMany(TOuterKey outerKey, ReadOnlySpan<KeyValuePair<TInnerKey, TValue>> values, TimeSpan timeToLive)
        {
            var outerKeyString = _outerKeySerializer(outerKey);

            var expirationDate = DateTimeOffset.UtcNow.Add(timeToLive);
            
            foreach (var kv in values)
                _memoryCache.Set(outerKeyString + _innerKeySerializer(kv.Key), (object)kv.Value ?? NullObj.Instance, expirationDate);
        }

        public void SetManyWithVaryingTimesToLive(
            TOuterKey outerKey,
            ReadOnlySpan<KeyValuePair<TInnerKey, ValueAndTimeToLive<TValue>>> values)
        {
            var outerKeyString = _outerKeySerializer(outerKey);

            var now = DateTimeOffset.UtcNow;
            foreach (var kv in values)
            {
                var expirationDate = now.Add(kv.Value.TimeToLive);

                _memoryCache.Set(outerKeyString + _innerKeySerializer(kv.Key), (object)kv.Value.Value ?? NullObj.Instance, expirationDate);
            }
        }

        public bool TryRemove(TOuterKey outerKey, TInnerKey innerKey, out TValue value)
        {
            var outerKeyString = _outerKeySerializer(outerKey);
            var innerKeyString = _innerKeySerializer(innerKey);

            var valueRemoved = _memoryCache.Remove(outerKeyString + innerKeyString);
            switch (valueRemoved)
            {
                case TValue returnValue:
                    value = returnValue;
                    return true;
                case NullObj _:
                    value = default;
                    return true;
                default:
                    value = default;
                    return false;
            }
        }

        private static TValue ConvertValue(object value)
        {
            return value is TValue v
                ? v
                : default;
        }
    }
}
