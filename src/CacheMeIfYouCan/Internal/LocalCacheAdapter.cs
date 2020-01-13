using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CacheMeIfYouCan.Internal
{
    internal sealed class LocalCacheAdapter<TKey, TValue> : ICache<TKey, TValue>
    {
        private readonly ILocalCache<TKey, TValue> _innerCache;
        private static readonly IReadOnlyCollection<KeyValuePair<TKey, TValue>> EmptyResults = new List<KeyValuePair<TKey, TValue>>();

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

        public ValueTask<IReadOnlyCollection<KeyValuePair<TKey, TValue>>> GetMany(IReadOnlyCollection<TKey> keys)
        {
            var results = _innerCache.GetMany(keys) ?? EmptyResults;
            
            return new ValueTask<IReadOnlyCollection<KeyValuePair<TKey, TValue>>>(results);
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
        private static readonly IReadOnlyCollection<KeyValuePair<TInnerKey, TValue>> EmptyResults =
            new List<KeyValuePair<TInnerKey, TValue>>();

        public LocalCacheAdapter(ILocalCache<TOuterKey, TInnerKey, TValue> innerCache)
        {
            _innerCache = innerCache;
        }

        public ValueTask<IReadOnlyCollection<KeyValuePair<TInnerKey, TValue>>> GetMany(
            TOuterKey outerKey,
            IReadOnlyCollection<TInnerKey> innerKeys)
        {
            var results = _innerCache.GetMany(outerKey, innerKeys) ?? EmptyResults;
            
            return new ValueTask<IReadOnlyCollection<KeyValuePair<TInnerKey, TValue>>>(results);
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