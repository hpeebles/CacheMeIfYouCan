using System;
using System.Buffers;
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

        public ValueTask<int> GetMany(IReadOnlyCollection<TKey> keys, Memory<KeyValuePair<TKey, TValue>> destination)
        {
            var countFromLocalCache = GetFromLocalCache();

            Dictionary<TKey, TValue> resultsDictionary;
            if (countFromLocalCache == 0)
            {
                resultsDictionary = new Dictionary<TKey, TValue>(_keyComparer);
            }
            else
            {
                resultsDictionary = new Dictionary<TKey, TValue>(countFromLocalCache, _keyComparer);
                foreach (var item in destination.Slice(0, countFromLocalCache).Span)
                    resultsDictionary[item.Key] = item.Value;
            }

            var missingKeys = MissingKeysResolver<TKey, TValue>.GetMissingKeys(keys, resultsDictionary);

            if (missingKeys is null)
                return new ValueTask<int>(resultsDictionary.Count);
            
            return GetFromDistributedCache();

            int GetFromLocalCache()
            {
                if (_skipLocalCacheGetPredicate is null)
                    return _localCache.GetMany(keys, destination);

                var filteredKeys = CacheKeysFilter<TKey>.Filter(keys, _skipLocalCacheGetPredicate, out var pooledKeyArray);

                try
                {
                    return filteredKeys.Count == 0
                        ? 0
                        : _localCache.GetMany(filteredKeys, destination);
                }
                finally
                {
                    CacheKeysFilter<TKey>.ReturnPooledArray(pooledKeyArray);
                }
            }
            
            async ValueTask<int> GetFromDistributedCache()
            {
                int countFromDistributedCache;
                var countRemaining = keys.Count - countFromLocalCache;
                var pooledValueArray = ArrayPool<KeyValuePair<TKey, ValueAndTimeToLive<TValue>>>.Shared.Rent(countRemaining);
                var valuesMemory = new Memory<KeyValuePair<TKey, ValueAndTimeToLive<TValue>>>(pooledValueArray);
                try
                {
                    if (_skipDistributedCacheGetPredicate is null)
                    {
                        countFromDistributedCache = await _distributedCache
                            .GetMany(missingKeys, valuesMemory)
                            .ConfigureAwait(false);
                    }
                    else
                    {
                        var filteredKeys = CacheKeysFilter<TKey>.Filter(
                            missingKeys,
                            _skipDistributedCacheGetPredicate,
                            out var pooledKeyArray);

                        try
                        {
                            if (filteredKeys.Count == 0)
                                return 0;

                            countFromDistributedCache = await _distributedCache
                                .GetMany(filteredKeys, valuesMemory)
                                .ConfigureAwait(false);
                        }
                        finally
                        {
                            CacheKeysFilter<TKey>.ReturnPooledArray(pooledKeyArray);
                        }
                    }

                    if (countFromDistributedCache > 0)
                        ProcessValuesFromDistributedCache(valuesMemory.Slice(0, countFromDistributedCache));
                }
                finally
                {
                    ArrayPool<KeyValuePair<TKey, ValueAndTimeToLive<TValue>>>.Shared.Return(pooledValueArray);
                }

                return countFromLocalCache + countFromDistributedCache;
            }

            void ProcessValuesFromDistributedCache(Memory<KeyValuePair<TKey, ValueAndTimeToLive<TValue>>> fromDistributedCache)
            {
                var fromDistributedCacheSpan = fromDistributedCache.Span;
                var destinationSpan = destination.Slice(countFromLocalCache).Span;
                for (var i = 0; i < fromDistributedCache.Length; i++)
                {
                    var kv = fromDistributedCacheSpan[i];
                    _localCache.Set(kv.Key, kv.Value.Value, kv.Value.TimeToLive);
                    destinationSpan[i] = new KeyValuePair<TKey, TValue>(kv.Key, kv.Value);
                }
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
        
        public ValueTask<int> GetMany(
            TOuterKey outerKey,
            IReadOnlyCollection<TInnerKey> innerKeys,
            Memory<KeyValuePair<TInnerKey, TValue>> destination)
        {
            var countFromLocalCache = GetFromLocalCache();

            Dictionary<TInnerKey, TValue> resultsDictionary;
            if (countFromLocalCache == 0)
            {
                resultsDictionary = new Dictionary<TInnerKey, TValue>(_keyComparer);
            }
            else
            {
                resultsDictionary = new Dictionary<TInnerKey, TValue>(countFromLocalCache, _keyComparer);
                foreach (var item in destination.Slice(0, countFromLocalCache).Span)
                    resultsDictionary[item.Key] = item.Value;
            }

            var missingKeys = MissingKeysResolver<TInnerKey, TValue>.GetMissingKeys(innerKeys, resultsDictionary);

            if (missingKeys is null)
                return new ValueTask<int>(resultsDictionary.Count);
            
            return GetFromDistributedCache();

            int GetFromLocalCache()
            {
                if (!(_skipLocalCacheGetPredicateOuterKeyOnly is null) &&
                    _skipLocalCacheGetPredicateOuterKeyOnly(outerKey))
                {
                    return 0;
                }

                if (_skipLocalCacheGetPredicate is null)
                    return _localCache.GetMany(outerKey, innerKeys, destination);
                
                var filteredKeys = CacheKeysFilter<TOuterKey, TInnerKey>.Filter(
                    outerKey,
                    innerKeys,
                    _skipLocalCacheGetPredicate,
                    out var pooledKeyArray);

                try
                {
                    return filteredKeys.Count == 0
                        ? 0
                        : _localCache.GetMany(outerKey, filteredKeys, destination);
                }
                finally
                {
                    CacheKeysFilter<TOuterKey, TInnerKey>.ReturnPooledArray(pooledKeyArray);
                }
            }
            
            async ValueTask<int> GetFromDistributedCache()
            {
                if (!(_skipDistributedCacheGetPredicateOuterKeyOnly is null) &&
                    _skipDistributedCacheGetPredicateOuterKeyOnly(outerKey))
                {
                    return countFromLocalCache;
                }

                int countFromDistributedCache;
                var countRemaining = innerKeys.Count - countFromLocalCache;
                var pooledValueArray = ArrayPool<KeyValuePair<TInnerKey, ValueAndTimeToLive<TValue>>>.Shared.Rent(countRemaining);
                var valuesMemory = new Memory<KeyValuePair<TInnerKey, ValueAndTimeToLive<TValue>>>(pooledValueArray);
                try
                {
                    if (_skipDistributedCacheGetPredicate is null)
                    {
                        countFromDistributedCache = await _distributedCache
                            .GetMany(outerKey, missingKeys, valuesMemory)
                            .ConfigureAwait(false);
                    }
                    else
                    {
                        var filteredKeys = CacheKeysFilter<TOuterKey, TInnerKey>.Filter(
                            outerKey,
                            missingKeys,
                            _skipDistributedCacheGetPredicate,
                            out var pooledKeyArray);

                        try
                        {
                            if (filteredKeys.Count == 0)
                                return countFromLocalCache;

                            countFromDistributedCache = await _distributedCache
                                .GetMany(outerKey, filteredKeys, valuesMemory)
                                .ConfigureAwait(false);
                        }
                        finally
                        {
                            CacheKeysFilter<TOuterKey, TInnerKey>.ReturnPooledArray(pooledKeyArray);
                        }
                    }

                    if (countFromDistributedCache > 0)
                        ProcessValuesFromDistributedCache(valuesMemory.Slice(0, countFromDistributedCache));
                }
                finally
                {
                    ArrayPool<KeyValuePair<TInnerKey, ValueAndTimeToLive<TValue>>>.Shared.Return(pooledValueArray);
                }

                return countFromLocalCache + countFromDistributedCache;
            }

            void ProcessValuesFromDistributedCache(Memory<KeyValuePair<TInnerKey, ValueAndTimeToLive<TValue>>> fromDistributedCache)
            {
                var fromDistributedCacheSpan = fromDistributedCache.Span;
                var destinationSpan = destination.Slice(countFromLocalCache).Span;
                
                _localCache.SetManyWithVaryingTimesToLive(outerKey, fromDistributedCache);
                
                for (var i = 0; i < fromDistributedCache.Length; i++)
                {
                    var kv = fromDistributedCacheSpan[i];
                    destinationSpan[i] = new KeyValuePair<TInnerKey, TValue>(kv.Key, kv.Value);
                }
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