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
        private static readonly IReadOnlyCollection<KeyValuePair<TKey, TValue>> EmptyResults = new List<KeyValuePair<TKey, TValue>>();

        public TwoTierCache(
            ILocalCache<TKey, TValue> localCache,
            IDistributedCache<TKey, TValue> distributedCache,
            IEqualityComparer<TKey> keyComparer,
            Func<TKey, bool> skipLocalCacheGetPredicate = null,
            Func<TKey, TValue, bool> skipLocalCacheSetPredicate = null,
            Func<TKey, bool> skipDistributedCacheGetPredicate = null,
            Func<TKey, TValue, bool> skipDistributedCacheSetPredicate = null)
        {
            _localCache = localCache ?? throw new ArgumentNullException(nameof(localCache));
            _distributedCache = distributedCache ?? throw new ArgumentNullException(nameof(distributedCache));
            _keyComparer = keyComparer;
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

            var resultsDictionary = fromLocalCache is null
                ? new Dictionary<TKey, TValue>(_keyComparer)
                : fromLocalCache.ToDictionary(kv => kv.Key, kv => kv.Value, _keyComparer);

            var missingKeys = GetMissingKeys();

            if (missingKeys == null)
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
                        ? EmptyResults
                        : _localCache.GetMany(filteredKeys);
                }
                finally
                {
                    CacheKeysFilter<TKey>.ReturnPooledArray(pooledArray);
                }
            }

            IReadOnlyCollection<TKey> GetMissingKeys()
            {
                if (resultsDictionary.Count == 0)
                    return keys;
                
                List<TKey> missingKeysList;
                if (resultsDictionary.Count < keys.Count)
                {
                    missingKeysList = new List<TKey>(keys.Count - resultsDictionary.Count);
                    foreach (var key in keys)
                    {
                        if (resultsDictionary.ContainsKey(key))
                            continue;
             
                        missingKeysList.Add(key);
                    }
                
                    return missingKeysList;
                }

                missingKeysList = null;
                foreach (var key in keys)
                {
                    if (resultsDictionary.ContainsKey(key))
                        continue;
                
                    if (missingKeysList == null)
                        missingKeysList = new List<TKey>();

                    missingKeysList.Add(key);
                }
            
                return missingKeysList;
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
                            return default;

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
                    var filteredValues = CacheValuesFilter<TKey, TValue>.Filter(values, _skipDistributedCacheSetPredicate, out var pooledArray);

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
}