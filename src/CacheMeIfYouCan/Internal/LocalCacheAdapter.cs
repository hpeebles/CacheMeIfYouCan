using System;
using System.Buffers;
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
            ReadOnlyMemory<TKey> keys,
            int cacheKeysSkipped,
            Memory<KeyValuePair<TKey, TValue>> destination)
        {
            int countFound;
            CacheGetManyStats cacheStats;
            if (_skipCacheGetPredicate is null)
            {
                countFound = _innerCache.GetMany(keys.Span, destination.Span);
                cacheStats = new CacheGetManyStats(
                    cacheKeysRequested: keys.Length,
                    cacheKeysSkipped: cacheKeysSkipped,
                    localCacheEnabled: true,
                    localCacheKeysSkipped: 0,
                    localCacheHits: countFound);
                
                return new ValueTask<CacheGetManyStats>(cacheStats);
            }

            var filteredKeys = CacheKeysFilter<TKey>.Filter(
                keys,
                _skipCacheGetPredicate,
                out var pooledKeyArray);

            try
            {
                countFound = filteredKeys.Length == 0
                    ? 0
                    : _innerCache.GetMany(filteredKeys.Span, destination.Span);
            }
            finally
            {
                if (!(pooledKeyArray is null))
                    ArrayPool<TKey>.Shared.Return(pooledKeyArray);
            }
            
            cacheStats = new CacheGetManyStats(
                cacheKeysRequested: keys.Length,
                cacheKeysSkipped: cacheKeysSkipped,
                localCacheEnabled: true,
                localCacheKeysSkipped: keys.Length - filteredKeys.Length,
                localCacheHits: countFound);
            
            return new ValueTask<CacheGetManyStats>(cacheStats);
        }

        public ValueTask SetMany(ReadOnlyMemory<KeyValuePair<TKey, TValue>> values, TimeSpan timeToLive)
        {
            if (_skipCacheSetPredicate is null)
            {
                _innerCache.SetMany(values.Span, timeToLive);
                return default;
            }

            var filteredValues = CacheValuesFilter<TKey, TValue>.Filter(
                values,
                _skipCacheSetPredicate,
                out var pooledArray);

            try
            {
                if (filteredValues.Length > 0)
                    _innerCache.SetMany(filteredValues.Span, timeToLive);
            }
            finally
            {
                if (!(pooledArray is null))
                    ArrayPool<KeyValuePair<TKey, TValue>>.Shared.Return(pooledArray);
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

        public bool LocalCacheEnabled => true;
        public bool DistributedCacheEnabled => false;

        public ValueTask<CacheGetManyStats> GetMany(
            TOuterKey outerKey,
            ReadOnlyMemory<TInnerKey> innerKeys,
            int cacheKeysSkipped,
            Memory<KeyValuePair<TInnerKey, TValue>> destination)
        {
            CacheGetManyStats cacheStats;
            if (_skipCacheGetOuterPredicate?.Invoke(outerKey) == true)
            {
                cacheStats = new CacheGetManyStats(
                    cacheKeysRequested: innerKeys.Length,
                    cacheKeysSkipped: cacheKeysSkipped,
                    localCacheEnabled: true,
                    localCacheKeysSkipped: innerKeys.Length);

                return new ValueTask<CacheGetManyStats>(cacheStats);
            }

            int countFound;
            if (_skipCacheGetInnerPredicate is null)
            {
                countFound = _innerCache.GetMany(outerKey, innerKeys.Span, destination.Span);
                cacheStats = new CacheGetManyStats(
                    cacheKeysRequested: innerKeys.Length,
                    cacheKeysSkipped: cacheKeysSkipped,
                    localCacheEnabled: true,
                    localCacheKeysSkipped: 0,
                    localCacheHits: countFound);

                return new ValueTask<CacheGetManyStats>(cacheStats);
            }

            var filteredKeys = CacheKeysFilter<TOuterKey, TInnerKey>.Filter(
                outerKey,
                innerKeys,
                _skipCacheGetInnerPredicate,
                out var pooledKeyArray);

            try
            {
                countFound = filteredKeys.Length == 0
                    ? 0
                    : _innerCache.GetMany(outerKey, filteredKeys.Span, destination.Span);
            }
            finally
            {
                if (!(pooledKeyArray is null))
                    ArrayPool<TInnerKey>.Shared.Return(pooledKeyArray);
            }
            
            cacheStats = new CacheGetManyStats(
                cacheKeysRequested: innerKeys.Length,
                cacheKeysSkipped: cacheKeysSkipped,
                localCacheEnabled: true,
                localCacheKeysSkipped: innerKeys.Length - filteredKeys.Length,
                localCacheHits: countFound);
            
            return new ValueTask<CacheGetManyStats>(cacheStats);
        }

        public ValueTask SetMany(
            TOuterKey outerKey,
            ReadOnlyMemory<KeyValuePair<TInnerKey, TValue>> values,
            TimeSpan timeToLive)
        {
            if (_skipCacheSetOuterPredicate?.Invoke(outerKey) == true)
                return default;

            if (_skipCacheSetInnerPredicate is null)
            {
                _innerCache.SetMany(outerKey, values.Span, timeToLive);
                return default;
            }
            
            var filteredValues = CacheValuesFilter<TOuterKey, TInnerKey, TValue>.Filter(
                outerKey,
                values,
                _skipCacheSetInnerPredicate,
                out var pooledArray);

            try
            {
                if (filteredValues.Length > 0)
                    _innerCache.SetMany(outerKey, filteredValues.Span, timeToLive);
            }
            finally
            {
                if (!(pooledArray is null))
                    ArrayPool<KeyValuePair<TInnerKey, TValue>>.Shared.Return(pooledArray);
            }

            return default;
        }
    }
}