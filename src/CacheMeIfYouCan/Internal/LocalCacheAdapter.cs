using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CacheMeIfYouCan.Internal
{
    internal sealed class LocalCacheAdapter<TKey, TValue> : ICache<TKey, TValue>
    {
        private readonly ILocalCache<TKey, TValue> _innerCache;

        public LocalCacheAdapter(ILocalCache<TKey, TValue> innerCache)
        {
            _innerCache = innerCache;
        }

        public ValueTask<(bool Success, TValue Value)> TryGet(TKey key)
        {
            var success = _innerCache.TryGet(key, out var value);

            return success
                ? new ValueTask<(bool, TValue)>((true, value))
                : default;
        }

        public ValueTask Set(TKey key, TValue value, TimeSpan timeToLive)
        {
            _innerCache.Set(key, value, timeToLive);
            
            return default;
        }

        public ValueTask<int> GetMany(IReadOnlyCollection<TKey> keys, Memory<KeyValuePair<TKey, TValue>> destination)
        {
            var countFound = _innerCache.GetMany(keys, destination);
            
            return new ValueTask<int>(countFound);
        }

        public ValueTask SetMany(IReadOnlyCollection<KeyValuePair<TKey, TValue>> values, TimeSpan timeToLive)
        {
            _innerCache.SetMany(values, timeToLive);
            
            return default;
        }
    }

    internal sealed class LocalCacheAdapter<TOuterKey, TInnerKey, TValue> : ICache<TOuterKey, TInnerKey, TValue>
    {
        private readonly ILocalCache<TOuterKey, TInnerKey, TValue> _innerCache;

        public LocalCacheAdapter(ILocalCache<TOuterKey, TInnerKey, TValue> innerCache)
        {
            _innerCache = innerCache;
        }

        public ValueTask<int> GetMany(
            TOuterKey outerKey,
            IReadOnlyCollection<TInnerKey> innerKeys,
            Memory<KeyValuePair<TInnerKey, TValue>> destination)
        {
            var countFound = _innerCache.GetMany(outerKey, innerKeys, destination);
            
            return new ValueTask<int>(countFound);
        }

        public ValueTask SetMany(
            TOuterKey outerKey,
            IReadOnlyCollection<KeyValuePair<TInnerKey, TValue>> values,
            TimeSpan timeToLive)
        {
            _innerCache.SetMany(outerKey, values, timeToLive);
            
            return default;
        }
    }
}