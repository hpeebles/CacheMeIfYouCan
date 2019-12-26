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

    internal sealed class LocalCacheAdapter<TOuterKey, TInnerKey, TValue> : ICache<TOuterKey, TInnerKey, TValue>
    {
        private readonly ILocalCache<TOuterKey, TInnerKey, TValue> _innerCache;
        private readonly Func<TOuterKey, bool> _skipCacheGetPredicateOuterKeyOnly;
        private readonly Func<TOuterKey, TInnerKey, bool> _skipCacheGetPredicate;
        private readonly Func<TOuterKey, bool> _skipCacheSetPredicateOuterKeyOnly;
        private readonly Func<TOuterKey, TInnerKey, TValue, bool> _skipCacheSetPredicate;

        private static readonly IReadOnlyCollection<KeyValuePair<TInnerKey, TValue>> EmptyResults =
            new List<KeyValuePair<TInnerKey, TValue>>();

        public LocalCacheAdapter(
            ILocalCache<TOuterKey, TInnerKey, TValue> innerCache,
            Func<TOuterKey, bool> skipCacheGetPredicateOuterKeyOnly,
            Func<TOuterKey, TInnerKey, bool> skipCacheGetPredicate,
            Func<TOuterKey, bool> skipCacheSetPredicateOuterKeyOnly,
            Func<TOuterKey, TInnerKey, TValue, bool> skipCacheSetPredicate)
        {
            _innerCache = innerCache;
            _skipCacheGetPredicateOuterKeyOnly = skipCacheGetPredicateOuterKeyOnly;
            _skipCacheGetPredicate = skipCacheGetPredicate;
            _skipCacheSetPredicateOuterKeyOnly = skipCacheSetPredicateOuterKeyOnly;
            _skipCacheSetPredicate = skipCacheSetPredicate;
        }

        public ValueTask<IReadOnlyCollection<KeyValuePair<TInnerKey, TValue>>> GetMany(
            TOuterKey outerKey,
            IReadOnlyCollection<TInnerKey> innerKeys)
        {
            return new ValueTask<IReadOnlyCollection<KeyValuePair<TInnerKey, TValue>>>(GetManyImpl());

            IReadOnlyCollection<KeyValuePair<TInnerKey, TValue>> GetManyImpl()
            {
                if (!(_skipCacheGetPredicateOuterKeyOnly is null) && _skipCacheGetPredicateOuterKeyOnly(outerKey))
                    return EmptyResults;
                
                if (_skipCacheGetPredicate is null)
                    return _innerCache.GetMany(outerKey, innerKeys);

                var filteredKeys = CacheKeysFilter<TOuterKey, TInnerKey>.Filter(
                    outerKey,
                    innerKeys,
                    _skipCacheGetPredicate,
                    out var pooledArray);

                try
                {
                    return filteredKeys.Count == 0
                        ? EmptyResults
                        : _innerCache.GetMany(outerKey, filteredKeys);
                }
                finally
                {
                    CacheKeysFilter<TOuterKey, TInnerKey>.ReturnPooledArray(pooledArray);
                }
            }
        }

        public ValueTask SetMany(
            TOuterKey outerKey,
            IReadOnlyCollection<KeyValuePair<TInnerKey, TValue>> values,
            TimeSpan timeToLive)
        {
            if (!(_skipCacheSetPredicateOuterKeyOnly is null) && _skipCacheSetPredicateOuterKeyOnly(outerKey))
                return default;
            
            if (_skipCacheSetPredicate is null)
            {
                _innerCache.SetMany(outerKey, values, timeToLive);
                return default;
            }

            var filteredValues = CacheValuesFilter<TOuterKey, TInnerKey, TValue>.Filter(
                outerKey,
                values,
                _skipCacheSetPredicate,
                out var pooledArray);

            try
            {
                if (filteredValues.Count > 0)
                    _innerCache.SetMany(outerKey, filteredValues, timeToLive);
            }
            finally
            {
                CacheValuesFilter<TOuterKey, TInnerKey, TValue>.ReturnPooledArray(pooledArray);
            }

            return default;
        }
    }
}