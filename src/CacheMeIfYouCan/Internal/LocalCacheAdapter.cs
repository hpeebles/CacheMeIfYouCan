using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CacheMeIfYouCan.Internal.CachedFunctions;

namespace CacheMeIfYouCan.Internal
{
    internal sealed class LocalCacheAdapter<TKey, TValue> : ICache<TKey, TValue>
    {
        private readonly ILocalCache<TKey, TValue> _innerCache;
        private readonly Func<TKey, bool> _skipCacheGetPredicate;
        private readonly Func<TKey, TValue, bool> _skipCacheSetPredicate;

        public LocalCacheAdapter(
            ILocalCache<TKey, TValue> innerCache,
            Func<TKey, bool> skipCacheGetPredicate,
            Func<TKey, TValue, bool> skipCacheSetPredicate)
        {
            _innerCache = innerCache;
            _skipCacheGetPredicate = skipCacheGetPredicate;
            _skipCacheSetPredicate = skipCacheSetPredicate;
        }
        
        public bool LocalCacheEnabled { get; } = true;
        public bool DistributedCacheEnabled { get; } = false;

        public ValueTask<(bool Success, TValue Value, CacheGetStats Stats)> TryGet(TKey key)
        {
            var flags = CacheGetFlags.LocalCache_Enabled;

            if (_skipCacheGetPredicate?.Invoke(key) == true)
            {
                flags |= CacheGetFlags.LocalCache_Skipped;
                return new ValueTask<(bool, TValue, CacheGetStats)>((false, default, flags.ToStats()));
            }

            flags |= CacheGetFlags.LocalCache_KeyRequested;
            
            var success = _innerCache.TryGet(key, out var value);

            if (!success)
                return new ValueTask<(bool, TValue, CacheGetStats)>((false, default, flags.ToStats()));

            flags |= CacheGetFlags.LocalCache_Hit;
            return new ValueTask<(bool, TValue, CacheGetStats)>((true, value, flags.ToStats()));
        }

        public ValueTask Set(TKey key, TValue value, TimeSpan timeToLive)
        {
            if (_skipCacheSetPredicate is null || !_skipCacheSetPredicate(key, value))
                _innerCache.Set(key, value, timeToLive);
            
            return default;
        }

        public ValueTask<CacheGetManyStats> GetMany(
            IReadOnlyCollection<TKey> keys,
            int cacheKeysSkipped,
            Memory<KeyValuePair<TKey, TValue>> destination)
        {
            int countFound;
            CacheGetManyStats stats;
            if (_skipCacheGetPredicate is null)
            {
                countFound = _innerCache.GetMany(keys, destination.Span);
                stats = new CacheGetManyStats(
                    cacheKeysRequested: keys.Count,
                    cacheKeysSkipped: cacheKeysSkipped,
                    localCacheEnabled: true,
                    localCacheKeysSkipped: 0,
                    localCacheHits: countFound);
                
                return new ValueTask<CacheGetManyStats>(stats);
            }

            var filteredKeys = CacheKeysFilter<TKey>.Filter(
                keys,
                _skipCacheGetPredicate,
                out var pooledKeyArray);

            try
            {
                countFound = filteredKeys.Count == 0
                    ? 0
                    : _innerCache.GetMany(filteredKeys, destination.Span);
            }
            finally
            {
                if (!(pooledKeyArray is null))
                    CacheKeysFilter<TKey>.ReturnPooledArray(pooledKeyArray);
            }
            
            stats = new CacheGetManyStats(
                cacheKeysRequested: keys.Count,
                cacheKeysSkipped: cacheKeysSkipped,
                localCacheEnabled: true,
                localCacheKeysSkipped: keys.Count - filteredKeys.Count,
                localCacheHits: countFound);
            
            return new ValueTask<CacheGetManyStats>(stats);
        }

        public ValueTask SetMany(IReadOnlyCollection<KeyValuePair<TKey, TValue>> values, TimeSpan timeToLive)
        {
            if (_skipCacheSetPredicate is null)
            {
                _innerCache.SetMany(values, timeToLive);
                return default;
            }

            var filteredValues = CacheValuesFilter<TKey, TValue>.Filter(
                values,
                _skipCacheSetPredicate,
                out var pooledArray);

            try
            {
                if (filteredValues.Count > 0)
                    _innerCache.SetMany(filteredValues, timeToLive);
            }
            finally
            {
                if (!(pooledArray is null))
                    CacheValuesFilter<TKey, TValue>.ReturnPooledArray(pooledArray);
            }

            return default;
        }
    }

    internal sealed class LocalCacheAdapter<TOuterKey, TInnerKey, TValue> : ICache<TOuterKey, TInnerKey, TValue>
    {
        private readonly ILocalCache<TOuterKey, TInnerKey, TValue> _innerCache;
        private readonly Func<TOuterKey, bool> _skipCacheGetOuterPredicate;
        private readonly Func<TOuterKey, TInnerKey, bool> _skipCacheGetInnerPredicate;
        private readonly Func<TOuterKey, bool> _skipCacheSetOuterPredicate;
        private readonly Func<TOuterKey, TInnerKey, TValue, bool> _skipCacheSetInnerPredicate;

        public LocalCacheAdapter(
            ILocalCache<TOuterKey, TInnerKey, TValue> innerCache,
            Func<TOuterKey, bool> skipCacheGetOuterPredicate,
            Func<TOuterKey, TInnerKey, bool> skipCacheGetInnerPredicate,
            Func<TOuterKey, bool> skipCacheSetOuterPredicate,
            Func<TOuterKey, TInnerKey, TValue, bool> skipCacheSetInnerPredicate)
        {
            _innerCache = innerCache;
            _skipCacheGetOuterPredicate = skipCacheGetOuterPredicate;
            _skipCacheGetInnerPredicate = skipCacheGetInnerPredicate;
            _skipCacheSetOuterPredicate = skipCacheSetOuterPredicate;
            _skipCacheSetInnerPredicate = skipCacheSetInnerPredicate;
        }

        public ValueTask<int> GetMany(
            TOuterKey outerKey,
            IReadOnlyCollection<TInnerKey> innerKeys,
            Memory<KeyValuePair<TInnerKey, TValue>> destination)
        {
            if (_skipCacheGetOuterPredicate?.Invoke(outerKey) == true)
                return default;

            if (_skipCacheGetInnerPredicate is null)
                return new ValueTask<int>(_innerCache.GetMany(outerKey, innerKeys, destination.Span));
            
            var filteredKeys = CacheKeysFilter<TOuterKey, TInnerKey>.Filter(
                outerKey,
                innerKeys,
                _skipCacheGetInnerPredicate,
                out var pooledKeyArray);

            try
            {
                return filteredKeys.Count == 0
                    ? default
                    : new ValueTask<int>(_innerCache.GetMany(outerKey, filteredKeys, destination.Span));
            }
            finally
            {
                if (!(pooledKeyArray is null))
                    CacheKeysFilter<TOuterKey, TInnerKey>.ReturnPooledArray(pooledKeyArray);
            }
        }

        public ValueTask SetMany(
            TOuterKey outerKey,
            IReadOnlyCollection<KeyValuePair<TInnerKey, TValue>> values,
            TimeSpan timeToLive)
        {
            if (_skipCacheSetOuterPredicate?.Invoke(outerKey) == true)
                return default;

            if (_skipCacheSetInnerPredicate is null)
            {
                _innerCache.SetMany(outerKey, values, timeToLive);
                return default;
            }
            
            var filteredValues = CacheValuesFilter<TOuterKey, TInnerKey, TValue>.Filter(
                outerKey,
                values,
                _skipCacheSetInnerPredicate,
                out var pooledArray);

            try
            {
                if (filteredValues.Count > 0)
                    _innerCache.SetMany(outerKey, filteredValues, timeToLive);
            }
            finally
            {
                if (!(pooledArray is null))
                    CacheValuesFilter<TOuterKey, TInnerKey, TValue>.ReturnPooledArray(pooledArray);
            }

            return default;
        }
    }
}