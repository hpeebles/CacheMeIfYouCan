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
            
            return new ValueTask<(bool Success, TValue Value)>((success, value));
        }

        public ValueTask Set(TKey key, TValue value, TimeSpan timeToLive)
        {
            _innerCache.Set(key, value, timeToLive);
            
            return default;
        }

        public ValueTask<IReadOnlyCollection<KeyValuePair<TKey, TValue>>> GetMany(IReadOnlyCollection<TKey> keys)
        {
            var results = _innerCache.GetMany(keys);
            
            return new ValueTask<IReadOnlyCollection<KeyValuePair<TKey, TValue>>>(results);
        }

        public ValueTask SetMany(IReadOnlyCollection<KeyValuePair<TKey, TValue>> values, TimeSpan timeToLive)
        {
            _innerCache.SetMany(values, timeToLive);
            
            return default;
        }
    }
}