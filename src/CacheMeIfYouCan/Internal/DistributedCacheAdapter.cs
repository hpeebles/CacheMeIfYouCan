using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CacheMeIfYouCan.Internal
{
    internal sealed class DistributedCacheAdapter<TKey, TValue> : ICache<TKey, TValue>
    {
        private readonly IDistributedCache<TKey, TValue> _innerCache;
        private readonly Func<TKey, bool> _skipCacheGetPredicate;
        private readonly Func<TKey, TValue, bool> _skipCacheSetPredicate;
        private static readonly IReadOnlyCollection<KeyValuePair<TKey, TValue>> EmptyResults = new List<KeyValuePair<TKey, TValue>>();

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
            if (_skipCacheGetPredicate is null || !_skipCacheGetPredicate(key))
            {
                var (success, valueAndTimeToLive) = await _innerCache
                    .TryGet(key)
                    .ConfigureAwait(false);

                if (success)
                    return (true, valueAndTimeToLive.Value);
            }
            
            return default;
        }

        public ValueTask Set(TKey key, TValue value, TimeSpan timeToLive)
        {
            if (_skipCacheSetPredicate is null || !_skipCacheSetPredicate(key, value))
                return new ValueTask(_innerCache.Set(key, value, timeToLive));

            return default;
        }

        public async ValueTask<IReadOnlyCollection<KeyValuePair<TKey, TValue>>> GetMany(IReadOnlyCollection<TKey> keys)
        {
            IReadOnlyCollection<KeyValuePair<TKey, ValueAndTimeToLive<TValue>>> resultsWithTimeToLive;
            if (_skipCacheGetPredicate is null)
            {
                resultsWithTimeToLive = await _innerCache
                    .GetMany(keys)
                    .ConfigureAwait(false);
            }
            else
            {
                var filteredKeys = CacheKeysFilter<TKey>.Filter(keys, _skipCacheGetPredicate, out var pooledArray);

                try
                {
                    if (filteredKeys.Count == 0)
                        return EmptyResults;

                    resultsWithTimeToLive = await _innerCache
                        .GetMany(filteredKeys)
                        .ConfigureAwait(false);
                }
                finally
                {
                    CacheKeysFilter<TKey>.ReturnPooledArray(pooledArray);
                }
            }
            
            var results = new List<KeyValuePair<TKey, TValue>>(resultsWithTimeToLive.Count);
            foreach (var kv in resultsWithTimeToLive)
                results.Add(new KeyValuePair<TKey, TValue>(kv.Key, kv.Value));

            return results;
        }

        public async ValueTask SetMany(IReadOnlyCollection<KeyValuePair<TKey, TValue>> values, TimeSpan timeToLive)
        {
            if (_skipCacheSetPredicate is null)
            {
                var task = _innerCache.SetMany(values, timeToLive);

                if (!task.IsCompleted)
                    await task.ConfigureAwait(false);

                return;
            }
            
            var filteredValues = CacheValuesFilter<TKey, TValue>.Filter(values, _skipCacheSetPredicate, out var pooledArray);

            try
            {
                if (filteredValues.Count == 0)
                    return;
                
                var task = _innerCache.SetMany(filteredValues, timeToLive);

                if (!task.IsCompleted)
                    await task.ConfigureAwait(false);
            }
            finally
            {
                CacheValuesFilter<TKey, TValue>.ReturnPooledArray(pooledArray);
            }
        }
    }
    
    internal sealed class DistributedCacheAdapter<TOuterKey, TInnerKey, TValue> : ICache<TOuterKey, TInnerKey, TValue>
    {
        private readonly IDistributedCache<TOuterKey, TInnerKey, TValue> _innerCache;
        private readonly Func<TOuterKey, bool> _skipCacheGetPredicateOuterKeyOnly;
        private readonly Func<TOuterKey, TInnerKey, bool> _skipCacheGetPredicate;
        private readonly Func<TOuterKey, bool> _skipCacheSetPredicateOuterKeyOnly;
        private readonly Func<TOuterKey, TInnerKey, TValue, bool> _skipCacheSetPredicate;
        private static readonly IReadOnlyCollection<KeyValuePair<TInnerKey, TValue>> EmptyResults = new List<KeyValuePair<TInnerKey, TValue>>();

        public DistributedCacheAdapter(
            IDistributedCache<TOuterKey, TInnerKey, TValue> innerCache,
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
        
        public async ValueTask<IReadOnlyCollection<KeyValuePair<TInnerKey, TValue>>> GetMany(
            TOuterKey outerKey,
            IReadOnlyCollection<TInnerKey> innerKeys)
        {
            if (!(_skipCacheGetPredicateOuterKeyOnly is null) && _skipCacheGetPredicateOuterKeyOnly(outerKey))
                return default;
            
            IReadOnlyCollection<KeyValuePair<TInnerKey, ValueAndTimeToLive<TValue>>> resultsWithTimeToLive;
            if (_skipCacheGetPredicate is null)
            {
                resultsWithTimeToLive = await _innerCache
                    .GetMany(outerKey, innerKeys)
                    .ConfigureAwait(false);
            }
            else
            {
                var filteredKeys = CacheKeysFilter<TOuterKey, TInnerKey>.Filter(outerKey, innerKeys, _skipCacheGetPredicate, out var pooledArray);

                try
                {
                    if (filteredKeys.Count == 0)
                        return EmptyResults;

                    resultsWithTimeToLive = await _innerCache
                        .GetMany(outerKey, filteredKeys)
                        .ConfigureAwait(false);
                }
                finally
                {
                    CacheKeysFilter<TOuterKey, TInnerKey>.ReturnPooledArray(pooledArray);
                }
            }
            
            var results = new List<KeyValuePair<TInnerKey, TValue>>(resultsWithTimeToLive.Count);
            foreach (var kv in resultsWithTimeToLive)
                results.Add(new KeyValuePair<TInnerKey, TValue>(kv.Key, kv.Value));

            return results;
        }

        public async ValueTask SetMany(
            TOuterKey outerKey,
            IReadOnlyCollection<KeyValuePair<TInnerKey, TValue>> values,
            TimeSpan timeToLive)
        {
            if (!(_skipCacheSetPredicateOuterKeyOnly is null) && _skipCacheSetPredicateOuterKeyOnly(outerKey))
                return;
            
            if (_skipCacheSetPredicate is null)
            {
                var task = _innerCache.SetMany(outerKey, values, timeToLive);

                if (!task.IsCompleted)
                    await task.ConfigureAwait(false);

                return;
            }
            
            var filteredValues = CacheValuesFilter<TOuterKey, TInnerKey, TValue>.Filter(
                outerKey,
                values,
                _skipCacheSetPredicate,
                out var pooledArray);

            try
            {
                if (filteredValues.Count == 0)
                    return;
                
                var task = _innerCache.SetMany(outerKey, filteredValues, timeToLive);

                if (!task.IsCompleted)
                    await task.ConfigureAwait(false);
            }
            finally
            {
                CacheValuesFilter<TOuterKey, TInnerKey, TValue>.ReturnPooledArray(pooledArray);
            }
        }
    }
}