using System;
using System.Buffers;
using System.Collections.Generic;
using System.Threading.Tasks;
using CacheMeIfYouCan.Internal.CachedFunctions;

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
        
        public bool LocalCacheEnabled { get; } = true;
        public bool DistributedCacheEnabled { get; } = true;

        public ValueTask<(bool Success, TValue Value, CacheGetStats Stats)> TryGet(TKey key)
        {
            var flags = CacheGetFlags.LocalCache_Enabled | CacheGetFlags.DistributedCache_Enabled;
            
            if (_skipLocalCacheGetPredicate is null || !_skipLocalCacheGetPredicate(key))
            {
                flags |= CacheGetFlags.LocalCache_KeyRequested;

                if (_localCache.TryGet(key, out var value))
                {
                    flags |= CacheGetFlags.LocalCache_Hit;
                    return new ValueTask<(bool, TValue, CacheGetStats)>((true, value, flags.ToStats()));
                }
            }
            else
            {
                flags |= CacheGetFlags.LocalCache_Skipped;
            }

            if (_skipDistributedCacheGetPredicate is null || !_skipDistributedCacheGetPredicate(key))
                return new ValueTask<(bool, TValue, CacheGetStats)>(GetFromDistributedCache());

            flags |= CacheGetFlags.DistributedCache_Skipped;
            return new ValueTask<(bool, TValue, CacheGetStats)>((false, default, flags.ToStats()));

            async Task<(bool, TValue, CacheGetStats)> GetFromDistributedCache()
            {
                flags |= CacheGetFlags.DistributedCache_KeyRequested;
                
                var (success, valueAndTimeToLive) = await _distributedCache
                    .TryGet(key)
                    .ConfigureAwait(false);

                if (!success)
                    return (false, default, flags.ToStats());
                
                flags |= CacheGetFlags.DistributedCache_Hit;
                    
                if (_skipLocalCacheSetPredicate is null || !_skipLocalCacheSetPredicate(key, valueAndTimeToLive.Value))
                    _localCache.Set(key, valueAndTimeToLive.Value, valueAndTimeToLive.TimeToLive);

                return (true, valueAndTimeToLive, flags.ToStats());
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

        public ValueTask<CacheGetManyStats> GetMany(
            IReadOnlyCollection<TKey> keys, int cacheKeysSkipped, Memory<KeyValuePair<TKey, TValue>> destination)
        {
            var localCacheKeysSkipped = 0;
            var countFromLocalCache = GetFromLocalCache();

            Dictionary<TKey, TValue> resultsDictionary;
            if (countFromLocalCache == 0)
            {
                resultsDictionary = new Dictionary<TKey, TValue>(_keyComparer);
            }
            else
            {
                resultsDictionary = new Dictionary<TKey, TValue>(countFromLocalCache, _keyComparer);
                foreach (var item in destination.Span.Slice(0, countFromLocalCache))
                    resultsDictionary[item.Key] = item.Value;
            }

            var missingKeys = MissingKeysResolver<TKey, TValue>.GetMissingKeys(
                keys,
                resultsDictionary,
                out var pooledMissingKeysArray);

            if (missingKeys is null)
            {
                // if missingKeys is null, then pooledMissingKeysArray will also be null, so no need to return it
                var cacheStats = new CacheGetManyStats(
                    cacheKeysRequested: keys.Count,
                    cacheKeysSkipped: cacheKeysSkipped,
                    localCacheEnabled: true,
                    distributedCacheEnabled: true,
                    localCacheKeysSkipped: localCacheKeysSkipped,
                    localCacheHits: countFromLocalCache);

                return new ValueTask<CacheGetManyStats>(cacheStats);
            }

            return GetFromDistributedCache();

            int GetFromLocalCache()
            {
                if (_skipLocalCacheGetPredicate is null)
                    return _localCache.GetMany(keys, destination.Span);

                var filteredKeys = CacheKeysFilter<TKey>.Filter(
                    keys,
                    _skipLocalCacheGetPredicate,
                    out var pooledKeyArray);

                localCacheKeysSkipped = keys.Count - filteredKeys.Count;
                
                try
                {
                    return filteredKeys.Count == 0
                        ? 0
                        : _localCache.GetMany(filteredKeys, destination.Span);
                }
                finally
                {
                    if (!(pooledKeyArray is null))
                        CacheKeysFilter<TKey>.ReturnPooledArray(pooledKeyArray);
                }
            }
            
            async ValueTask<CacheGetManyStats> GetFromDistributedCache()
            {
                int countFromDistributedCache;
                var distributedCacheKeysSkipped = 0;
                KeyValuePair<TKey, ValueAndTimeToLive<TValue>>[] pooledValueArray = null;
                try
                {
                    if (_skipDistributedCacheGetPredicate is null)
                    {
                        var countRemaining = keys.Count - countFromLocalCache;
                        pooledValueArray = ArrayPool<KeyValuePair<TKey, ValueAndTimeToLive<TValue>>>.Shared.Rent(countRemaining);
                        
                        countFromDistributedCache = await _distributedCache
                            .GetMany(missingKeys, pooledValueArray)
                            .ConfigureAwait(false);
                    }
                    else
                    {
                        var filteredKeys = CacheKeysFilter<TKey>.Filter(
                            missingKeys,
                            _skipDistributedCacheGetPredicate,
                            out var pooledFilteredKeysArray);

                        distributedCacheKeysSkipped = missingKeys.Count - filteredKeys.Count;
                        
                        try
                        {
                            if (filteredKeys.Count == 0)
                            {
                                return new CacheGetManyStats(
                                    cacheKeysRequested: keys.Count,
                                    cacheKeysSkipped: cacheKeysSkipped,
                                    localCacheEnabled: true,
                                    distributedCacheEnabled: true,
                                    localCacheKeysSkipped: localCacheKeysSkipped,
                                    localCacheHits: countFromLocalCache,
                                    distributedCacheKeysSkipped: distributedCacheKeysSkipped);
                            }

                            pooledValueArray = ArrayPool<KeyValuePair<TKey, ValueAndTimeToLive<TValue>>>.Shared.Rent(filteredKeys.Count);
                            
                            countFromDistributedCache = await _distributedCache
                                .GetMany(filteredKeys, pooledValueArray)
                                .ConfigureAwait(false);
                        }
                        finally
                        {
                            if (!(pooledFilteredKeysArray is null))
                                CacheKeysFilter<TKey>.ReturnPooledArray(pooledFilteredKeysArray);
                        }
                    }

                    if (countFromDistributedCache > 0)
                        ProcessValuesFromDistributedCache(pooledValueArray.AsSpan(0, countFromDistributedCache));
                }
                finally
                {
                    if (!(pooledValueArray is null))
                        ArrayPool<KeyValuePair<TKey, ValueAndTimeToLive<TValue>>>.Shared.Return(pooledValueArray);
                    
                    if (!(pooledMissingKeysArray is null))
                        ArrayPool<TKey>.Shared.Return(pooledMissingKeysArray);
                }

                return new CacheGetManyStats(
                    cacheKeysRequested: keys.Count,
                    cacheKeysSkipped: cacheKeysSkipped,
                    localCacheEnabled: true,
                    distributedCacheEnabled: true,
                    localCacheKeysSkipped: localCacheKeysSkipped,
                    localCacheHits: countFromLocalCache,
                    distributedCacheKeysSkipped: distributedCacheKeysSkipped,
                    distributedCacheHits: countFromDistributedCache);
            }

            void ProcessValuesFromDistributedCache(ReadOnlySpan<KeyValuePair<TKey, ValueAndTimeToLive<TValue>>> fromDistributedCache)
            {
                var destinationSpan = destination.Slice(countFromLocalCache).Span;
                for (var i = 0; i < fromDistributedCache.Length; i++)
                {
                    var kv = fromDistributedCache[i];
                    destinationSpan[i] = new KeyValuePair<TKey, TValue>(kv.Key, kv.Value);
                    
                    if (_skipLocalCacheSetPredicate is null || !_skipLocalCacheSetPredicate(kv.Key, kv.Value))
                        _localCache.Set(kv.Key, kv.Value.Value, kv.Value.TimeToLive);
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
                    var filteredValues = CacheValuesFilter<TKey, TValue>.Filter(
                        values,
                        _skipLocalCacheSetPredicate,
                        out var pooledArray);

                    try
                    {
                        if (filteredValues.Count > 0)
                            _localCache.SetMany(filteredValues, timeToLive);
                    }
                    finally
                    {
                        if (!(pooledArray is null))
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
                    var filteredValues = CacheValuesFilter<TKey, TValue>.Filter(
                        values,
                        _skipDistributedCacheSetPredicate,
                        out var pooledArray);

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
                        if (!(pooledArray is null))
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
        private readonly Func<TOuterKey, bool> _skipLocalCacheGetOuterPredicate;
        private readonly Func<TOuterKey, TInnerKey, bool> _skipLocalCacheGetInnerPredicate;
        private readonly Func<TOuterKey, bool> _skipLocalCacheSetOuterPredicate;
        private readonly Func<TOuterKey, TInnerKey, TValue, bool> _skipLocalCacheSetInnerPredicate;
        private readonly Func<TOuterKey, bool> _skipDistributedCacheGetOuterPredicate;
        private readonly Func<TOuterKey, TInnerKey, bool> _skipDistributedCacheGetInnerPredicate;
        private readonly Func<TOuterKey, bool> _skipDistributedCacheSetOuterPredicate;
        private readonly Func<TOuterKey, TInnerKey, TValue, bool> _skipDistributedCacheSetInnerPredicate;

        public TwoTierCache(
            ILocalCache<TOuterKey, TInnerKey, TValue> localCache,
            IDistributedCache<TOuterKey, TInnerKey, TValue> distributedCache,
            IEqualityComparer<TInnerKey> keyComparer = null,
            Func<TOuterKey, bool> skipLocalCacheGetOuterPredicate = null,
            Func<TOuterKey, TInnerKey, bool> skipLocalCacheGetInnerPredicate = null,
            Func<TOuterKey, bool> skipLocalCacheSetOuterPredicate = null,
            Func<TOuterKey, TInnerKey, TValue, bool> skipLocalCacheSetInnerPredicate = null,
            Func<TOuterKey, bool> skipDistributedCacheGetOuterPredicate = null,
            Func<TOuterKey, TInnerKey, bool> skipDistributedCacheGetInnerPredicate = null,
            Func<TOuterKey, bool> skipDistributedCacheSetOuterPredicate = null,
            Func<TOuterKey, TInnerKey, TValue, bool> skipDistributedCacheSetInnerPredicate = null)
        {
            _localCache = localCache ?? throw new ArgumentNullException(nameof(localCache));
            _distributedCache = distributedCache ?? throw new ArgumentNullException(nameof(distributedCache));
            _keyComparer = keyComparer ?? EqualityComparer<TInnerKey>.Default;
            _skipLocalCacheGetOuterPredicate = skipLocalCacheGetOuterPredicate;
            _skipLocalCacheGetInnerPredicate = skipLocalCacheGetInnerPredicate;
            _skipLocalCacheSetOuterPredicate = skipLocalCacheSetOuterPredicate;
            _skipLocalCacheSetInnerPredicate = skipLocalCacheSetInnerPredicate;
            _skipDistributedCacheGetOuterPredicate = skipDistributedCacheGetOuterPredicate;
            _skipDistributedCacheGetInnerPredicate = skipDistributedCacheGetInnerPredicate;
            _skipDistributedCacheSetOuterPredicate = skipDistributedCacheSetOuterPredicate;
            _skipDistributedCacheSetInnerPredicate = skipDistributedCacheSetInnerPredicate;
        }

        public bool LocalCacheEnabled => true;
        public bool DistributedCacheEnabled => true;
        
        public ValueTask<CacheGetManyStats> GetMany(
            TOuterKey outerKey,
            IReadOnlyCollection<TInnerKey> innerKeys,
            int cacheKeysSkipped,
            Memory<KeyValuePair<TInnerKey, TValue>> destination)
        {
            var (countFromLocalCache, localCacheKeysSkipped) = GetFromLocalCache();

            Dictionary<TInnerKey, TValue> resultsDictionary;
            if (countFromLocalCache == 0)
            {
                resultsDictionary = new Dictionary<TInnerKey, TValue>(_keyComparer);
            }
            else
            {
                resultsDictionary = new Dictionary<TInnerKey, TValue>(countFromLocalCache, _keyComparer);
                foreach (var item in destination.Span.Slice(0, countFromLocalCache))
                    resultsDictionary[item.Key] = item.Value;
            }
            
            if (_skipDistributedCacheGetOuterPredicate?.Invoke(outerKey) == true)
            {
                var cacheStats = new CacheGetManyStats(
                    cacheKeysRequested: innerKeys.Count,
                    cacheKeysSkipped: cacheKeysSkipped,
                    localCacheEnabled: true,
                    distributedCacheEnabled: true,
                    localCacheKeysSkipped: localCacheKeysSkipped,
                    localCacheHits: countFromLocalCache,
                    distributedCacheKeysSkipped: innerKeys.Count - countFromLocalCache);
                
                return new ValueTask<CacheGetManyStats>(cacheStats);
            }

            var missingKeys = MissingKeysResolver<TInnerKey, TValue>.GetMissingKeys(
                innerKeys,
                resultsDictionary,
                out var pooledMissingKeysArray);

            if (missingKeys is null)
            {
                // if missingKeys is null, then pooledMissingKeysArray will also be null, so no need to return it
                var cacheStats = new CacheGetManyStats(
                    cacheKeysRequested: innerKeys.Count,
                    cacheKeysSkipped: cacheKeysSkipped,
                    localCacheEnabled: true,
                    distributedCacheEnabled: true,
                    localCacheKeysSkipped: localCacheKeysSkipped,
                    localCacheHits: countFromLocalCache);
                
                return new ValueTask<CacheGetManyStats>(cacheStats);
            }
            
            return GetFromDistributedCache();

            (int CountFound, int CountSkipped) GetFromLocalCache()
            {
                if (_skipLocalCacheGetOuterPredicate?.Invoke(outerKey) == true)
                    return (0, innerKeys.Count);

                if (_skipLocalCacheGetInnerPredicate is null)
                    return (_localCache.GetMany(outerKey, innerKeys, destination.Span), 0);
                
                var filteredKeys = CacheKeysFilter<TOuterKey, TInnerKey>.Filter(
                    outerKey,
                    innerKeys,
                    _skipLocalCacheGetInnerPredicate,
                    out var pooledFilteredKeysArray);

                var countSkipped = innerKeys.Count - filteredKeys.Count;
                try
                {
                    return filteredKeys.Count == 0
                        ? (0, countSkipped)
                        : (_localCache.GetMany(outerKey, filteredKeys, destination.Span), countSkipped);
                }
                finally
                {
                    if (!(pooledFilteredKeysArray is null))
                        CacheKeysFilter<TOuterKey, TInnerKey>.ReturnPooledArray(pooledFilteredKeysArray);
                }
            }
            
            async ValueTask<CacheGetManyStats> GetFromDistributedCache()
            {
                int countFromDistributedCache;
                int countSkipped = 0;
                KeyValuePair<TInnerKey, ValueAndTimeToLive<TValue>>[] pooledValueArray = null;
                try
                {
                    if (_skipDistributedCacheGetInnerPredicate is null)
                    {
                        pooledValueArray = ArrayPool<KeyValuePair<TInnerKey, ValueAndTimeToLive<TValue>>>.Shared.Rent(missingKeys.Count);
                        
                        countFromDistributedCache = await _distributedCache
                            .GetMany(outerKey, missingKeys, pooledValueArray)
                            .ConfigureAwait(false);
                    }
                    else
                    {
                        var filteredKeys = CacheKeysFilter<TOuterKey, TInnerKey>.Filter(
                            outerKey,
                            missingKeys,
                            _skipDistributedCacheGetInnerPredicate,
                            out var pooledFilteredKeysArray);

                        countSkipped = missingKeys.Count - filteredKeys.Count;
                        try
                        {
                            if (filteredKeys.Count == 0)
                            {
                                return new CacheGetManyStats(
                                    cacheKeysRequested: innerKeys.Count,
                                    cacheKeysSkipped: cacheKeysSkipped,
                                    localCacheEnabled: true,
                                    distributedCacheEnabled: true,
                                    localCacheKeysSkipped: localCacheKeysSkipped,
                                    localCacheHits: countFromLocalCache,
                                    distributedCacheKeysSkipped: missingKeys.Count);
                            }
                            
                            pooledValueArray = ArrayPool<KeyValuePair<TInnerKey, ValueAndTimeToLive<TValue>>>.Shared.Rent(filteredKeys.Count);

                            countFromDistributedCache = await _distributedCache
                                .GetMany(outerKey, filteredKeys, pooledValueArray)
                                .ConfigureAwait(false);
                        }
                        finally
                        {
                            if (!(pooledFilteredKeysArray is null))
                                CacheKeysFilter<TOuterKey, TInnerKey>.ReturnPooledArray(pooledFilteredKeysArray);
                        }
                    }

                    if (countFromDistributedCache > 0)
                    {
                        ProcessValuesFromDistributedCache(
                            outerKey,
                            pooledValueArray.AsSpan().Slice(0, countFromDistributedCache),
                            destination.Span.Slice(countFromLocalCache));
                    }
                }
                finally
                {
                    if (!(pooledValueArray is null))
                        ArrayPool<KeyValuePair<TInnerKey, ValueAndTimeToLive<TValue>>>.Shared.Return(pooledValueArray);
                    
                    if (!(pooledMissingKeysArray is null))
                        ArrayPool<TInnerKey>.Shared.Return(pooledMissingKeysArray);
                }

                return new CacheGetManyStats(
                    cacheKeysRequested: innerKeys.Count,
                    cacheKeysSkipped: cacheKeysSkipped,
                    localCacheEnabled: true,
                    distributedCacheEnabled: true,
                    localCacheKeysSkipped: localCacheKeysSkipped,
                    localCacheHits: countFromLocalCache,
                    distributedCacheKeysSkipped: countSkipped,
                    distributedCacheHits: countFromDistributedCache);
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
                if (_skipLocalCacheSetOuterPredicate?.Invoke(outerKey) == true)
                    return;
                
                if (_skipLocalCacheSetInnerPredicate is null)
                {
                    _localCache.SetMany(outerKey, values, timeToLive);
                }
                else
                {
                    var filteredValues = CacheValuesFilter<TOuterKey, TInnerKey, TValue>.Filter(
                        outerKey,
                        values,
                        _skipLocalCacheSetInnerPredicate,
                        out var pooledArray);

                    try
                    {
                        if (filteredValues.Count > 0)
                            _localCache.SetMany(outerKey, filteredValues, timeToLive);
                    }
                    finally
                    {
                        if (!(pooledArray is null))
                            CacheValuesFilter<TOuterKey, TInnerKey, TValue>.ReturnPooledArray(pooledArray);
                    }
                }
            }

            async ValueTask SetInDistributedCache()
            {
                if (_skipDistributedCacheSetOuterPredicate?.Invoke(outerKey) == true)
                    return;
                
                if (_skipDistributedCacheSetInnerPredicate is null)
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
                        _skipDistributedCacheSetInnerPredicate,
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
                        if (!(pooledArray is null))
                            CacheValuesFilter<TOuterKey, TInnerKey, TValue>.ReturnPooledArray(pooledArray);
                    }
                }
            }
        }
        
        private void ProcessValuesFromDistributedCache(
            TOuterKey outerKey,
            ReadOnlySpan<KeyValuePair<TInnerKey, ValueAndTimeToLive<TValue>>> fromDistributedCache,
            Span<KeyValuePair<TInnerKey, TValue>> destination)
        {
            StoreValuesFromDistributedCacheInLocalCache(outerKey, fromDistributedCache);
                
            for (var i = 0; i < fromDistributedCache.Length; i++)
            {
                var kv = fromDistributedCache[i];
                destination[i] = new KeyValuePair<TInnerKey, TValue>(kv.Key, kv.Value);
            }
        }

        private void StoreValuesFromDistributedCacheInLocalCache(
            TOuterKey outerKey,
            ReadOnlySpan<KeyValuePair<TInnerKey, ValueAndTimeToLive<TValue>>> fromDistributedCache)
        {
            if (_skipLocalCacheSetOuterPredicate?.Invoke(outerKey) == true)
                return;
                
            if (_skipLocalCacheSetInnerPredicate is null)
            {
                _localCache.SetManyWithVaryingTimesToLive(outerKey, fromDistributedCache);
            }
            else
            {
                var filteredValues = CacheValuesFilter<TOuterKey, TInnerKey, TValue>.Filter(
                    outerKey,
                    fromDistributedCache,
                    _skipLocalCacheSetInnerPredicate,
                    out var pooledArray);

                try
                {
                    if (filteredValues.Count > 0)
                        _localCache.SetManyWithVaryingTimesToLive(outerKey, filteredValues);
                }
                finally
                {
                    if (!(pooledArray is null))
                        CacheValuesFilter<TOuterKey, TInnerKey, TValue>.ReturnPooledArray(pooledArray);
                }
            }
        }
    }
}