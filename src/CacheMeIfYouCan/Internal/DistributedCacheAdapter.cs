using System;
using System.Buffers;
using System.Collections.Generic;
using System.Threading.Tasks;

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

        public async ValueTask<(bool Success, TValue Value)> TryGet(TKey key)
        {
            if (_skipCacheGetPredicate?.Invoke(key) == true)
                return default;
            
            var (success, valueAndTimeToLive) = await _innerCache
                .TryGet(key)
                .ConfigureAwait(false);

            return success
                ? (true, valueAndTimeToLive.Value)
                : default;
        }

        public ValueTask Set(TKey key, TValue value, TimeSpan timeToLive)
        {
            return _skipCacheSetPredicate?.Invoke(key, value) == true
                ? default
                : new ValueTask(_innerCache.Set(key, value, timeToLive));
        }

        public ValueTask<int> GetMany(IReadOnlyCollection<TKey> keys, Memory<KeyValuePair<TKey, TValue>> destination)
        {
            if (_skipCacheGetPredicate is null)
                return GetManyImpl(keys, destination);

            var filteredKeys = CacheKeysFilter<TKey>.Filter(keys, _skipCacheGetPredicate, out var pooledKeyArray);

            try
            {
                return filteredKeys.Count == 0
                    ? default
                    : GetManyImpl(filteredKeys, destination);
            }
            finally
            {
                if (!(pooledKeyArray is null))
                    CacheKeysFilter<TKey>.ReturnPooledArray(pooledKeyArray);
            }
        }

        private async ValueTask<int> GetManyImpl(
            IReadOnlyCollection<TKey> keys,
            Memory<KeyValuePair<TKey, TValue>> destination)
        {
            var pooledArray = ArrayPool<KeyValuePair<TKey, ValueAndTimeToLive<TValue>>>.Shared.Rent(keys.Count);

            try
            {
                var countFound = await _innerCache
                    .GetMany(keys, pooledArray)
                    .ConfigureAwait(false);

                if (countFound > 0)
                    CopyResultsToDestination(countFound);

                return countFound;
            }
            finally
            {
                ArrayPool<KeyValuePair<TKey, ValueAndTimeToLive<TValue>>>.Shared.Return(pooledArray);
            }

            void CopyResultsToDestination(int countFound)
            {
                var span = destination.Span;
                for (var i = 0; i <= countFound; i++)
                {
                    var kv = pooledArray[i];
                    span[i] = new KeyValuePair<TKey, TValue>(kv.Key, kv.Value);
                }
            }
        }

        public ValueTask SetMany(IReadOnlyCollection<KeyValuePair<TKey, TValue>> values, TimeSpan timeToLive)
        {
            if (_skipCacheSetPredicate is null)
                return SetManyImpl(values, timeToLive);
            
            var filteredValues = CacheValuesFilter<TKey, TValue>.Filter(
                values,
                _skipCacheSetPredicate,
                out var pooledArray);

            try
            {
                return filteredValues.Count == 0
                    ? default
                    : SetManyImpl(filteredValues, timeToLive);
            }
            finally
            {
                if (!(pooledArray is null))
                    CacheValuesFilter<TKey, TValue>.ReturnPooledArray(pooledArray);
            }
        }

        private async ValueTask SetManyImpl(IReadOnlyCollection<KeyValuePair<TKey, TValue>> values, TimeSpan timeToLive)
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

        public ValueTask<int> GetMany(
            TOuterKey outerKey,
            IReadOnlyCollection<TInnerKey> innerKeys,
            Memory<KeyValuePair<TInnerKey, TValue>> destination)
        {
            if (_skipCacheGetOuterPredicate?.Invoke(outerKey) == true)
                return default;

            if (_skipCacheGetInnerPredicate is null)
                return GetManyImpl(outerKey, innerKeys, destination);
            
            var filteredKeys = CacheKeysFilter<TOuterKey, TInnerKey>.Filter(
                outerKey,
                innerKeys,
                _skipCacheGetInnerPredicate,
                out var pooledKeyArray);

            try
            {
                return filteredKeys.Count == 0
                    ? default
                    : GetManyImpl(outerKey, filteredKeys, destination);
            }
            finally
            {
                if (!(pooledKeyArray is null))
                    CacheKeysFilter<TOuterKey, TInnerKey>.ReturnPooledArray(pooledKeyArray);
            }
        }

        private async ValueTask<int> GetManyImpl(
            TOuterKey outerKey,
            IReadOnlyCollection<TInnerKey> innerKeys,
            Memory<KeyValuePair<TInnerKey, TValue>> destination)
        {
            var pooledArray = ArrayPool<KeyValuePair<TInnerKey, ValueAndTimeToLive<TValue>>>.Shared.Rent(innerKeys.Count);

            try
            {
                var countFound = await _innerCache
                    .GetMany(outerKey, innerKeys, pooledArray)
                    .ConfigureAwait(false);

                if (countFound > 0)
                    CopyResultsToDestination(countFound);
                
                return countFound;
            }
            finally
            {
                ArrayPool<KeyValuePair<TInnerKey, ValueAndTimeToLive<TValue>>>.Shared.Return(pooledArray);
            }

            void CopyResultsToDestination(int countFound)
            {
                var span = destination.Span;
                for (var i = 0; i <= countFound; i++)
                {
                    var kv = pooledArray[i];
                    span[i] = new KeyValuePair<TInnerKey, TValue>(kv.Key, kv.Value);
                }
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
                return SetManyImpl(outerKey, values, timeToLive);
            
            var filteredValues = CacheValuesFilter<TOuterKey, TInnerKey, TValue>.Filter(
                outerKey,
                values,
                _skipCacheSetInnerPredicate,
                out var pooledArray);

            try
            {
                return filteredValues.Count == 0
                    ? default
                    : SetManyImpl(outerKey, filteredValues, timeToLive);
            }
            finally
            {
                if (!(pooledArray is null))
                    CacheValuesFilter<TOuterKey, TInnerKey, TValue>.ReturnPooledArray(pooledArray);
            }
        }

        private async ValueTask SetManyImpl(
            TOuterKey outerKey,
            IReadOnlyCollection<KeyValuePair<TInnerKey, TValue>> values,
            TimeSpan timeToLive)
        {
            var task = _innerCache.SetMany(outerKey, values, timeToLive);

            if (!task.IsCompleted)
                await task.ConfigureAwait(false);
        }
    }
}