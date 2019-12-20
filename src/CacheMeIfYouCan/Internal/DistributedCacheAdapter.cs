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
}