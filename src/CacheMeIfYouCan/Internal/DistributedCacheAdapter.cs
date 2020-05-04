using System;
using System.Buffers;
using System.Collections.Generic;
using System.Threading.Tasks;
using CacheMeIfYouCan.Internal.CachedFunctions;

namespace CacheMeIfYouCan.Internal
{
    internal sealed class DistributedCacheAdapter<TKey, TValue> : ICache<TKey, TValue>
    {
        private readonly IDistributedCache<TKey, TValue> _innerCache;
        private readonly Func<TKey, bool> _skipCacheGetPredicate;
        private readonly Func<TKey, TValue, bool> _skipCacheSetPredicate;

        public DistributedCacheAdapter(
            IDistributedCache<TKey, TValue> innerCache,
            Func<TKey, bool> skipCacheGetPredicate,
            Func<TKey, TValue, bool> skipCacheSetPredicate)
        {
            _innerCache = innerCache;
            _skipCacheGetPredicate = skipCacheGetPredicate;
            _skipCacheSetPredicate = skipCacheSetPredicate;
        }

        public bool LocalCacheEnabled { get; } = false;
        public bool DistributedCacheEnabled { get; } = true;

        public async ValueTask<(bool Success, TValue Value, CacheGetStats Stats)> TryGet(TKey key)
        {
            var flags = CacheGetFlags.DistributedCache_Enabled;

            if (_skipCacheGetPredicate?.Invoke(key) == true)
            {
                flags |= CacheGetFlags.DistributedCache_Skipped;
                return (false, default, flags.ToStats());
            }

            flags |= CacheGetFlags.DistributedCache_KeyRequested;
            
            var (success, valueAndTimeToLive) = await _innerCache
                .TryGet(key)
                .ConfigureAwait(false);

            if (!success)
                return (false, default, flags.ToStats());

            flags |= CacheGetFlags.DistributedCache_Hit;
            return (true, valueAndTimeToLive.Value, flags.ToStats());
        }

        public ValueTask Set(TKey key, TValue value, TimeSpan timeToLive)
        {
            return _skipCacheSetPredicate?.Invoke(key, value) == true
                ? default
                : new ValueTask(_innerCache.Set(key, value, timeToLive));
        }

        public ValueTask<CacheGetManyStats> GetMany(
            ReadOnlyMemory<TKey> keys,
            int cacheKeysSkipped,
            Memory<KeyValuePair<TKey, TValue>> destination)
        {
            if (_skipCacheGetPredicate is null)
                return GetManyImpl(keys);

            var filteredKeys = CacheKeysFilter<TKey>.Filter(keys, _skipCacheGetPredicate, out var pooledKeyArray);

            try
            {
                if (filteredKeys.Length == 0)
                {
                    var cacheStats = new CacheGetManyStats(
                        cacheKeysRequested: keys.Length,
                        cacheKeysSkipped: cacheKeysSkipped,
                        distributedCacheEnabled: true,
                        distributedCacheKeysSkipped: keys.Length);

                    return new ValueTask<CacheGetManyStats>(cacheStats);
                }

                return GetManyImpl(filteredKeys);
            }
            finally
            {
                if (!(pooledKeyArray is null))
                    ArrayPool<TKey>.Shared.Return(pooledKeyArray);
            }

            async ValueTask<CacheGetManyStats> GetManyImpl(ReadOnlyMemory<TKey> keysToGetFromCache)
            {
                using var memoryOwner = MemoryPool<KeyValuePair<TKey, ValueAndTimeToLive<TValue>>>.Shared.Rent(keysToGetFromCache.Length);
                var memory = memoryOwner.Memory;

                var countFound = await _innerCache
                    .GetMany(keysToGetFromCache, memory)
                    .ConfigureAwait(false);

                if (countFound > 0)
                    CopyResultsToDestination(memory.Span.Slice(0, countFound), destination.Span);

                return new CacheGetManyStats(
                    cacheKeysRequested: keys.Length,
                    cacheKeysSkipped: cacheKeysSkipped,
                    distributedCacheEnabled: true,
                    distributedCacheKeysSkipped: keys.Length - keysToGetFromCache.Length,
                    distributedCacheHits: countFound);
                
                static void CopyResultsToDestination(
                    ReadOnlySpan<KeyValuePair<TKey, ValueAndTimeToLive<TValue>>> values,
                    Span<KeyValuePair<TKey, TValue>> destinationSpan)
                {
                    for (var i = 0; i < values.Length; i++)
                    {
                        var kv = values[i];
                        destinationSpan[i] = new KeyValuePair<TKey, TValue>(kv.Key, kv.Value);
                    }
                }
            }
        }

        public ValueTask SetMany(ReadOnlyMemory<KeyValuePair<TKey, TValue>> values, TimeSpan timeToLive)
        {
            if (_skipCacheSetPredicate is null)
                return SetManyImpl(values, timeToLive);
            
            var filteredValues = CacheValuesFilter<TKey, TValue>.Filter(
                values,
                _skipCacheSetPredicate,
                out var pooledArray);

            try
            {
                return filteredValues.Length == 0
                    ? default
                    : SetManyImpl(filteredValues, timeToLive);
            }
            finally
            {
                if (!(pooledArray is null))
                    ArrayPool<KeyValuePair<TKey, TValue>>.Shared.Return(pooledArray);
            }
        }

        private async ValueTask SetManyImpl(ReadOnlyMemory<KeyValuePair<TKey, TValue>> values, TimeSpan timeToLive)
        {
            var task = _innerCache.SetMany(values, timeToLive);

            if (!task.IsCompleted)
                await task.ConfigureAwait(false);
        }
    }
    
    internal sealed class DistributedCacheAdapter<TOuterKey, TInnerKey, TValue> : ICache<TOuterKey, TInnerKey, TValue>
    {
        private readonly IDistributedCache<TOuterKey, TInnerKey, TValue> _innerCache;
        private readonly Func<TOuterKey, bool> _skipCacheGetOuterPredicate;
        private readonly Func<TOuterKey, TInnerKey, bool> _skipCacheGetInnerPredicate;
        private readonly Func<TOuterKey, bool> _skipCacheSetOuterPredicate;
        private readonly Func<TOuterKey, TInnerKey, TValue, bool> _skipCacheSetInnerPredicate;

        public DistributedCacheAdapter(
            IDistributedCache<TOuterKey, TInnerKey, TValue> innerCache,
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

        public bool LocalCacheEnabled => false;
        public bool DistributedCacheEnabled => true;

        public ValueTask<CacheGetManyStats> GetMany(
            TOuterKey outerKey,
            ReadOnlyMemory<TInnerKey> innerKeys,
            int cacheKeysSkipped,
            Memory<KeyValuePair<TInnerKey, TValue>> destination)
        {
            if (_skipCacheGetOuterPredicate?.Invoke(outerKey) == true)
            {
                var cacheStats = new CacheGetManyStats(
                    cacheKeysRequested: innerKeys.Length,
                    cacheKeysSkipped: cacheKeysSkipped,
                    distributedCacheEnabled: true,
                    distributedCacheKeysSkipped: innerKeys.Length);

                return new ValueTask<CacheGetManyStats>(cacheStats);
            }

            if (_skipCacheGetInnerPredicate is null)
                return GetManyImpl(outerKey, innerKeys, cacheKeysSkipped, 0, destination);
            
            var filteredKeys = CacheKeysFilter<TOuterKey, TInnerKey>.Filter(
                outerKey,
                innerKeys,
                _skipCacheGetInnerPredicate,
                out var pooledKeyArray);

            try
            {
                if (filteredKeys.Length == 0)
                {
                    var cacheStats = new CacheGetManyStats(
                        cacheKeysRequested: innerKeys.Length,
                        cacheKeysSkipped: cacheKeysSkipped,
                        distributedCacheEnabled: true,
                        distributedCacheKeysSkipped: innerKeys.Length);

                    return new ValueTask<CacheGetManyStats>(cacheStats);
                }

                return GetManyImpl(outerKey, filteredKeys, cacheKeysSkipped, innerKeys.Length - filteredKeys.Length, destination);
            }
            finally
            {
                if (!(pooledKeyArray is null))
                    ArrayPool<TInnerKey>.Shared.Return(pooledKeyArray);
            }
        }

        private async ValueTask<CacheGetManyStats> GetManyImpl(
            TOuterKey outerKey,
            ReadOnlyMemory<TInnerKey> innerKeys,
            int cacheKeysSkipped,
            int distributedCacheKeysSkipped,
            Memory<KeyValuePair<TInnerKey, TValue>> destination)
        {
            using var memoryOwner = MemoryPool<KeyValuePair<TInnerKey, ValueAndTimeToLive<TValue>>>.Shared.Rent(innerKeys.Length);
            var memory = memoryOwner.Memory;

            var countFound = await _innerCache
                .GetMany(outerKey, innerKeys, memory)
                .ConfigureAwait(false);

            if (countFound > 0)
                CopyResultsToDestination(memory.Span.Slice(0, countFound), destination.Span);

            return new CacheGetManyStats(
                cacheKeysRequested: innerKeys.Length + distributedCacheKeysSkipped,
                cacheKeysSkipped: cacheKeysSkipped,
                distributedCacheEnabled: true,
                distributedCacheKeysSkipped: distributedCacheKeysSkipped,
                distributedCacheHits: countFound);

            static void CopyResultsToDestination(
                ReadOnlySpan<KeyValuePair<TInnerKey, ValueAndTimeToLive<TValue>>> values,
                Span<KeyValuePair<TInnerKey, TValue>> destinationSpan)
            {
                for (var i = 0; i < values.Length; i++)
                {
                    var kv = values[i];
                    destinationSpan[i] = new KeyValuePair<TInnerKey, TValue>(kv.Key, kv.Value);
                }
            }
        }

        public ValueTask SetMany(
            TOuterKey outerKey,
            ReadOnlyMemory<KeyValuePair<TInnerKey, TValue>> values,
            TimeSpan timeToLive)
        {
            if (_skipCacheSetOuterPredicate?.Invoke(outerKey) == true)
                return default;

            if (_skipCacheSetInnerPredicate is null)
                return SetManyImpl(outerKey, values, timeToLive);
            
            var filteredValues = CacheValuesFilter<TOuterKey, TInnerKey, TValue>.Filter(
                outerKey,
                values,
                _skipCacheSetInnerPredicate,
                out var pooledArray);

            try
            {
                return filteredValues.Length == 0
                    ? default
                    : SetManyImpl(outerKey, filteredValues, timeToLive);
            }
            finally
            {
                if (!(pooledArray is null))
                    ArrayPool<KeyValuePair<TInnerKey, TValue>>.Shared.Return(pooledArray);
            }
        }

        private async ValueTask SetManyImpl(
            TOuterKey outerKey,
            ReadOnlyMemory<KeyValuePair<TInnerKey, TValue>> values,
            TimeSpan timeToLive)
        {
            var task = _innerCache.SetMany(outerKey, values, timeToLive);

            if (!task.IsCompleted)
                await task.ConfigureAwait(false);
        }
    }
}