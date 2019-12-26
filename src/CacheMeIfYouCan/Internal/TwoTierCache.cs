using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CacheMeIfYouCan.Internal
{
    internal sealed class TwoTierCache<TKey, TValue> : ICache<TKey, TValue>
    {
        private readonly ILocalCache<TKey, TValue> _localCache;
        private readonly IDistributedCache<TKey, TValue> _distributedCache;
        private readonly IEqualityComparer<TKey> _keyComparer;
        private readonly Func<TKey, bool> _skipLocalCacheGetPredicate;
        private readonly Func<TKey, TValue, bool> _skipLocalCacheSetPredicate;
        private readonly Func<TKey, bool> _skipDistributedCacheGetPredicate;
        private readonly Func<TKey, TValue, bool> _skipDistributedCacheSetPredicate;

        public TwoTierCache(
            ILocalCache<TKey, TValue> localCache,
            IDistributedCache<TKey, TValue> distributedCache,
            IEqualityComparer<TKey> keyComparer = null,
            Func<TKey, bool> skipLocalCacheGetPredicate = null,
            Func<TKey, TValue, bool> skipLocalCacheSetPredicate = null,
            Func<TKey, bool> skipDistributedCacheGetPredicate = null,
            Func<TKey, TValue, bool> skipDistributedCacheSetPredicate = null)
        {
            _localCache = localCache ?? throw new ArgumentNullException(nameof(localCache));
            _distributedCache = distributedCache ?? throw new ArgumentNullException(nameof(distributedCache));
            _keyComparer = keyComparer ?? EqualityComparer<TKey>.Default;
            _skipLocalCacheGetPredicate = skipLocalCacheGetPredicate;
            _skipLocalCacheSetPredicate = skipLocalCacheSetPredicate;
            _skipDistributedCacheGetPredicate = skipDistributedCacheGetPredicate;
            _skipDistributedCacheSetPredicate = skipDistributedCacheSetPredicate;
        }

        public ValueTask<(bool Success, TValue Value)> TryGet(TKey key)
        {
            if (_skipLocalCacheGetPredicate is null || !_skipLocalCacheGetPredicate(key))
            {
                if (_localCache.TryGet(key, out var value))
                    return new ValueTask<(bool Success, TValue Value)>((true, value));
            }

            if (_skipDistributedCacheGetPredicate is null || !_skipDistributedCacheGetPredicate(key))
                return new ValueTask<(bool Success, TValue Value)>(GetFromDistributedCache());

            return default;

            async Task<(bool, TValue)> GetFromDistributedCache()
            {
                var (success, valueAndTimeToLive) = await _distributedCache
                    .TryGet(key)
                    .ConfigureAwait(false);

                if (success)
                {
                    if (_skipLocalCacheSetPredicate is null || !_skipLocalCacheSetPredicate(key, valueAndTimeToLive.Value))
                        _localCache.Set(key, valueAndTimeToLive.Value, valueAndTimeToLive.TimeToLive);
                }

                return (success, valueAndTimeToLive);
            }
        }

        public ValueTask Set(TKey key, TValue value, TimeSpan timeToLive)
        {
            if (_skipLocalCacheSetPredicate is null || !_skipLocalCacheSetPredicate(key, value))
                _localCache.Set(key, value, timeToLive);
            
            if (_skipDistributedCacheSetPredicate is null || !_skipDistributedCacheSetPredicate(key, value))
                return new ValueTask(_distributedCache.Set(key, value, timeToLive));

            return default;
        }

        public ValueTask<IReadOnlyCollection<KeyValuePair<TKey, TValue>>> GetMany(IReadOnlyCollection<TKey> keys)
        {
            var fromLocalCache = GetFromLocalCache();

            var resultsDictionary = fromLocalCache is null || fromLocalCache.Count == 0
                ? new Dictionary<TKey, TValue>(_keyComparer)
                : fromLocalCache.ToDictionary(kv => kv.Key, kv => kv.Value, _keyComparer);

            var missingKeys = MissingKeysResolver<TKey, TValue>.GetMissingKeys(keys, resultsDictionary);

            if (missingKeys is null)
                return new ValueTask<IReadOnlyCollection<KeyValuePair<TKey, TValue>>>(resultsDictionary);
            
            return GetFromDistributedCache();

            IReadOnlyCollection<KeyValuePair<TKey, TValue>> GetFromLocalCache()
            {
                if (_skipLocalCacheGetPredicate is null)
                    return _localCache.GetMany(keys);
                
                var filteredKeys = CacheKeysFilter<TKey>.Filter(keys, _skipLocalCacheGetPredicate, out var pooledArray);

                try
                {
                    return filteredKeys.Count == 0
                        ? null
                        : _localCache.GetMany(filteredKeys);
                }
                finally
                {
                    CacheKeysFilter<TKey>.ReturnPooledArray(pooledArray);
                }
            }
            
            async ValueTask<IReadOnlyCollection<KeyValuePair<TKey, TValue>>> GetFromDistributedCache()
            {
                IReadOnlyCollection<KeyValuePair<TKey, ValueAndTimeToLive<TValue>>> fromDistributedCache;
                if (_skipDistributedCacheGetPredicate is null)
                {
                    fromDistributedCache = await _distributedCache
                        .GetMany(missingKeys)
                        .ConfigureAwait(false);
                }
                else
                {
                    var filteredKeys = CacheKeysFilter<TKey>.Filter(missingKeys, _skipDistributedCacheGetPredicate, out var pooledArray);

                    try
                    {
                        if (filteredKeys.Count == 0)
                            return null;

                        fromDistributedCache = await _distributedCache
                            .GetMany(filteredKeys)
                            .ConfigureAwait(false);
                    }
                    finally
                    {
                        CacheKeysFilter<TKey>.ReturnPooledArray(pooledArray);
                    }
                }

                foreach (var kv in fromDistributedCache)
                {
                    _localCache.Set(kv.Key, kv.Value.Value, kv.Value.TimeToLive);
                    resultsDictionary[kv.Key] = kv.Value.Value;
                }

                return resultsDictionary;
            }
        }

        public ValueTask SetMany(IReadOnlyCollection<KeyValuePair<TKey, TValue>> values, TimeSpan timeToLive)
        {
            SetInLocalCache();

            return SetInDistributedCache();

            void SetInLocalCache()
            {
                if (_skipLocalCacheSetPredicate is null)
                {
                    _localCache.SetMany(values, timeToLive);
                }
                else
                {
                    var filteredValues = CacheValuesFilter<TKey, TValue>.Filter(values, _skipLocalCacheSetPredicate, out var pooledArray);

                    try
                    {
                        if (filteredValues.Count > 0)
                            _localCache.SetMany(filteredValues, timeToLive);
                    }
                    finally
                    {
                        CacheValuesFilter<TKey, TValue>.ReturnPooledArray(pooledArray);
                    }
                }
            }

            async ValueTask SetInDistributedCache()
            {
                if (_skipDistributedCacheSetPredicate is null)
                {
                    await _distributedCache
                        .SetMany(values, timeToLive)
                        .ConfigureAwait(false);
                }
                else
                {
                    var filteredValues = CacheValuesFilter<TKey, TValue>.Filter(values, _skipDistributedCacheSetPredicate, out var pooledArray);

                    try
                    {
                        if (filteredValues.Count > 0)
                        {
                            await _distributedCache
                                .SetMany(filteredValues, timeToLive)
                                .ConfigureAwait(false);
                        }
                    }
                    finally
                    {
                        CacheValuesFilter<TKey, TValue>.ReturnPooledArray(pooledArray);
                    }
                }
            }
        }
    }
    
    internal sealed class TwoTierCache<TOuterKey, TInnerKey, TValue> : ICache<TOuterKey, TInnerKey, TValue>
    {
        private readonly ILocalCache<TOuterKey, TInnerKey, TValue> _localCache;
        private readonly IDistributedCache<TOuterKey, TInnerKey, TValue> _distributedCache;
        private readonly IEqualityComparer<TInnerKey> _keyComparer;
        private readonly Func<TOuterKey, bool> _skipLocalCacheGetPredicateOuterKeyOnly;
        private readonly Func<TOuterKey, TInnerKey, bool> _skipLocalCacheGetPredicate;
        private readonly Func<TOuterKey, bool> _skipLocalCacheSetPredicateOuterKeyOnly;
        private readonly Func<TOuterKey, TInnerKey, TValue, bool> _skipLocalCacheSetPredicate;
        private readonly Func<TOuterKey, bool> _skipDistributedCacheGetPredicateOuterKeyOnly;
        private readonly Func<TOuterKey, TInnerKey, bool> _skipDistributedCacheGetPredicate;
        private readonly Func<TOuterKey, bool> _skipDistributedCacheSetPredicateOuterKeyOnly;
        private readonly Func<TOuterKey, TInnerKey, TValue, bool> _skipDistributedCacheSetPredicate;

        public TwoTierCache(
            ILocalCache<TOuterKey, TInnerKey, TValue> localCache,
            IDistributedCache<TOuterKey, TInnerKey, TValue> distributedCache,
            IEqualityComparer<TInnerKey> keyComparer = null,
            Func<TOuterKey, bool> skipLocalCacheGetPredicateOuterKeyOnly = null,
            Func<TOuterKey, TInnerKey, bool> skipLocalCacheGetPredicate = null,
            Func<TOuterKey, bool> skipLocalCacheSetPredicateOuterKeyOnly = null,
            Func<TOuterKey, TInnerKey, TValue, bool> skipLocalCacheSetPredicate = null,
            Func<TOuterKey, bool> skipDistributedCacheGetPredicateOuterKeyOnly = null,
            Func<TOuterKey, TInnerKey, bool> skipDistributedCacheGetPredicate = null,
            Func<TOuterKey, bool> skipDistributedCacheSetPredicateOuterKeyOnly = null,
            Func<TOuterKey, TInnerKey, TValue, bool> skipDistributedCacheSetPredicate = null)
        {
            _localCache = localCache ?? throw new ArgumentNullException(nameof(localCache));
            _distributedCache = distributedCache ?? throw new ArgumentNullException(nameof(distributedCache));
            _keyComparer = keyComparer ?? EqualityComparer<TInnerKey>.Default;
            _skipLocalCacheGetPredicateOuterKeyOnly = skipLocalCacheGetPredicateOuterKeyOnly;
            _skipLocalCacheGetPredicate = skipLocalCacheGetPredicate;
            _skipLocalCacheSetPredicateOuterKeyOnly = skipLocalCacheSetPredicateOuterKeyOnly;
            _skipLocalCacheSetPredicate = skipLocalCacheSetPredicate;
            _skipDistributedCacheGetPredicateOuterKeyOnly = skipDistributedCacheGetPredicateOuterKeyOnly;
            _skipDistributedCacheGetPredicate = skipDistributedCacheGetPredicate;
            _skipDistributedCacheSetPredicateOuterKeyOnly = skipDistributedCacheSetPredicateOuterKeyOnly;
            _skipDistributedCacheSetPredicate = skipDistributedCacheSetPredicate;
        }

        public ValueTask<IReadOnlyCollection<KeyValuePair<TInnerKey, TValue>>> GetMany(
            TOuterKey outerKey,
            IReadOnlyCollection<TInnerKey> innerKeys)
        {
            var fromLocalCache = GetFromLocalCache();

            var resultsDictionary = fromLocalCache is null || fromLocalCache.Count == 0
                ? new Dictionary<TInnerKey, TValue>(_keyComparer)
                : fromLocalCache.ToDictionary(kv => kv.Key, kv => kv.Value, _keyComparer);

            var missingKeys = MissingKeysResolver<TInnerKey, TValue>.GetMissingKeys(innerKeys, resultsDictionary);

            if (missingKeys is null)
                return new ValueTask<IReadOnlyCollection<KeyValuePair<TInnerKey, TValue>>>(resultsDictionary);
            
            return GetFromDistributedCache();

            IReadOnlyCollection<KeyValuePair<TInnerKey, TValue>> GetFromLocalCache()
            {
                if (!(_skipLocalCacheGetPredicateOuterKeyOnly is null) &&
                    _skipLocalCacheGetPredicateOuterKeyOnly(outerKey))
                {
                    return null;
                }

                if (_skipLocalCacheGetPredicate is null)
                    return _localCache.GetMany(outerKey, innerKeys);
                
                var filteredKeys = CacheKeysFilter<TOuterKey, TInnerKey>.Filter(
                    outerKey,
                    innerKeys,
                    _skipLocalCacheGetPredicate,
                    out var pooledArray);

                try
                {
                    return filteredKeys.Count == 0
                        ? null
                        : _localCache.GetMany(outerKey, filteredKeys);
                }
                finally
                {
                    CacheKeysFilter<TOuterKey, TInnerKey>.ReturnPooledArray(pooledArray);
                }
            }
            
            async ValueTask<IReadOnlyCollection<KeyValuePair<TInnerKey, TValue>>> GetFromDistributedCache()
            {
                if (!(_skipDistributedCacheGetPredicateOuterKeyOnly is null) &&
                    _skipDistributedCacheGetPredicateOuterKeyOnly(outerKey))
                {
                    return resultsDictionary;
                }

                IReadOnlyCollection<KeyValuePair<TInnerKey, ValueAndTimeToLive<TValue>>> fromDistributedCache;
                if (_skipDistributedCacheGetPredicate is null)
                {
                    fromDistributedCache = await _distributedCache
                        .GetMany(outerKey, missingKeys)
                        .ConfigureAwait(false);
                }
                else
                {
                    var filteredKeys = CacheKeysFilter<TOuterKey, TInnerKey>.Filter(
                        outerKey,
                        missingKeys,
                        _skipDistributedCacheGetPredicate,
                        out var pooledArray);

                    try
                    {
                        if (filteredKeys.Count == 0)
                            return null;

                        fromDistributedCache = await _distributedCache
                            .GetMany(outerKey, filteredKeys)
                            .ConfigureAwait(false);
                    }
                    finally
                    {
                        CacheKeysFilter<TOuterKey, TInnerKey>.ReturnPooledArray(pooledArray);
                    }
                }

                if (fromDistributedCache != null && fromDistributedCache.Count > 0)
                {
                    _localCache.SetMany(outerKey, fromDistributedCache);

                    foreach (var kv in fromDistributedCache)
                        resultsDictionary[kv.Key] = kv.Value.Value;
                }

                return resultsDictionary;
            }
        }

        public ValueTask SetMany(
            TOuterKey outerKey,
            IReadOnlyCollection<KeyValuePair<TInnerKey, TValue>> values,
            TimeSpan timeToLive)
        {
            SetInLocalCache();

            return SetInDistributedCache();

            void SetInLocalCache()
            {
                if (!(_skipLocalCacheSetPredicateOuterKeyOnly is null) &&
                    _skipLocalCacheSetPredicateOuterKeyOnly(outerKey))
                {
                    return;
                }
                
                if (_skipLocalCacheSetPredicate is null)
                {
                    _localCache.SetMany(outerKey, values, timeToLive);
                }
                else
                {
                    var filteredValues = CacheValuesFilter<TOuterKey, TInnerKey, TValue>.Filter(
                        outerKey,
                        values,
                        _skipLocalCacheSetPredicate,
                        out var pooledArray);

                    try
                    {
                        if (filteredValues.Count > 0)
                            _localCache.SetMany(outerKey, filteredValues, timeToLive);
                    }
                    finally
                    {
                        CacheValuesFilter<TOuterKey, TInnerKey, TValue>.ReturnPooledArray(pooledArray);
                    }
                }
            }

            async ValueTask SetInDistributedCache()
            {
                if (!(_skipDistributedCacheSetPredicateOuterKeyOnly is null) &&
                    _skipDistributedCacheSetPredicateOuterKeyOnly(outerKey))
                {
                    return;
                }
                
                if (_skipDistributedCacheSetPredicate is null)
                {
                    await _distributedCache
                        .SetMany(outerKey, values, timeToLive)
                        .ConfigureAwait(false);
                }
                else
                {
                    var filteredValues = CacheValuesFilter<TOuterKey, TInnerKey, TValue>.Filter(
                        outerKey,
                        values,
                        _skipDistributedCacheSetPredicate,
                        out var pooledArray);

                    try
                    {
                        if (filteredValues.Count > 0)
                        {
                            await _distributedCache
                                .SetMany(outerKey, filteredValues, timeToLive)
                                .ConfigureAwait(false);
                        }
                    }
                    finally
                    {
                        CacheValuesFilter<TOuterKey, TInnerKey, TValue>.ReturnPooledArray(pooledArray);
                    }
                }
            }
        }
    }
}