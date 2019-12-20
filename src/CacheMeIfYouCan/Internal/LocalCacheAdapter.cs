using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CacheMeIfYouCan.Internal
{
    internal sealed class LocalCacheAdapter<TKey, TValue> : ICache<TKey, TValue>
    {
        private readonly ILocalCache<TKey, TValue> _innerCache;
        private readonly Func<TKey, bool> _skipCacheGetPredicate;
        private readonly Func<TKey, TValue, bool> _skipCacheSetPredicate;
        private static readonly IReadOnlyCollection<KeyValuePair<TKey, TValue>> EmptyResults = new List<KeyValuePair<TKey, TValue>>();

        public LocalCacheAdapter(
            ILocalCache<TKey, TValue> innerCache,
            Func<TKey, bool> skipCacheGetPredicate,
            Func<TKey, TValue, bool> skipCacheSetPredicate)
        {
            _innerCache = innerCache;
            _skipCacheGetPredicate = skipCacheGetPredicate;
            _skipCacheSetPredicate = skipCacheSetPredicate;
        }

        public ValueTask<(bool Success, TValue Value)> TryGet(TKey key)
        {
            if (_skipCacheGetPredicate is null || !_skipCacheGetPredicate(key))
            {
                var success = _innerCache.TryGet(key, out var value);
                
                if (success)
                    return new ValueTask<(bool, TValue)>((true, value)); 
            }

            return default;
        }

        public ValueTask Set(TKey key, TValue value, TimeSpan timeToLive)
        {
            if (_skipCacheSetPredicate is null || !_skipCacheSetPredicate(key, value))
                _innerCache.Set(key, value, timeToLive);
            
            return default;
        }

        public ValueTask<IReadOnlyCollection<KeyValuePair<TKey, TValue>>> GetMany(IReadOnlyCollection<TKey> keys)
        {
            return new ValueTask<IReadOnlyCollection<KeyValuePair<TKey, TValue>>>(GetManyImpl());

            IReadOnlyCollection<KeyValuePair<TKey, TValue>> GetManyImpl()
            {
                if (_skipCacheGetPredicate is null)
                    return _innerCache.GetMany(keys);
            
                var filteredKeys = CacheKeysFilter<TKey>.Filter(keys, _skipCacheGetPredicate, out var pooledArray);

                try
                {
                    return filteredKeys.Count == 0
                        ? EmptyResults
                        : _innerCache.GetMany(filteredKeys);
                }
                finally
                {
                    CacheKeysFilter<TKey>.ReturnPooledArray(pooledArray);
                }
            }
        }

        public ValueTask SetMany(IReadOnlyCollection<KeyValuePair<TKey, TValue>> values, TimeSpan timeToLive)
        {
            if (_skipCacheSetPredicate is null)
            {
                _innerCache.SetMany(values, timeToLive);
                return default;
            }

            var filteredValues = CacheValuesFilter<TKey, TValue>.Filter(values, _skipCacheSetPredicate, out var pooledArray);

            try
            {
                if (filteredValues.Count > 0)
                    _innerCache.SetMany(filteredValues, timeToLive);
            }
            finally
            {
                CacheValuesFilter<TKey, TValue>.ReturnPooledArray(pooledArray);
            }
            
            return default;
        }
    }
}