using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CacheMeIfYouCan.Internal.CachedFunctions
{
    internal sealed class CachedFunctionWithEnumerableKeys<TKey, TValue>
    {
        private readonly Func<IReadOnlyCollection<TKey>, CancellationToken, Task<IEnumerable<KeyValuePair<TKey, TValue>>>> _originalFunction;
        private readonly Func<IReadOnlyCollection<TKey>, TimeSpan> _timeToLiveFactory;
        private readonly IEqualityComparer<TKey> _keyComparer;
        private readonly Func<TKey, bool> _skipCacheGetPredicate;
        private readonly Func<TKey, TValue, bool> _skipCacheSetPredicate;
        private readonly ICache<TKey, TValue> _cache;
        private static readonly IReadOnlyCollection<KeyValuePair<TKey, TValue>> EmptyValuesCollection = new List<KeyValuePair<TKey, TValue>>();

        public CachedFunctionWithEnumerableKeys(
            Func<IReadOnlyCollection<TKey>, CancellationToken, Task<IEnumerable<KeyValuePair<TKey, TValue>>>> originalFunction,
            CachedFunctionWithEnumerableKeysConfiguration<TKey, TValue> config)
        {
            _originalFunction = originalFunction;

            if (config.DisableCaching)
            {
                _timeToLiveFactory = keys => TimeSpan.Zero;
                _cache = NullCache<TKey, TValue>.Instance;
            }
            else
            {
                _timeToLiveFactory = config.TimeToLiveFactory;
                _keyComparer = config.KeyComparer ?? EqualityComparer<TKey>.Default;
                _skipCacheGetPredicate = config.SkipCacheGetPredicate;
                _skipCacheSetPredicate = config.SkipCacheSetPredicate;
                _cache = CacheBuilder.Build(config);
            }
        }

        public async Task<Dictionary<TKey, TValue>> GetMany(
            IEnumerable<TKey> keys,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            var keysCollection = keys.ToReadOnlyCollection();

            var getFromCacheTask = GetFromCache();

            if (!getFromCacheTask.IsCompleted)
                await getFromCacheTask.ConfigureAwait(false);

            var fromCache = getFromCacheTask.Result;
            
            var resultsDictionary = fromCache is null || fromCache.Count == 0
                ? new Dictionary<TKey, TValue>(_keyComparer)
                : fromCache.ToDictionary(kv => kv.Key, kv => kv.Value, _keyComparer);

            var missingKeys = MissingKeysResolver<TKey, TValue>.GetMissingKeys(keysCollection, resultsDictionary);

            if (missingKeys is null)
                return resultsDictionary;
            
            var getValuesFromFuncTask = _originalFunction(missingKeys, cancellationToken);

            if (!getValuesFromFuncTask.IsCompleted)
                await getValuesFromFuncTask.ConfigureAwait(false);

            var valuesFromFunc = getValuesFromFuncTask.Result;

            if (valuesFromFunc is null)
                return resultsDictionary;

            var valuesFromFuncCollection = valuesFromFunc.ToReadOnlyCollection();
            if (valuesFromFuncCollection.Count == 0)
                return default;

            var setInCacheTask = SetInCache();

            foreach (var kv in valuesFromFuncCollection)
                resultsDictionary[kv.Key] = kv.Value;
            
            if (!setInCacheTask.IsCompleted)
                await setInCacheTask.ConfigureAwait(false);

            return resultsDictionary;

            ValueTask<IReadOnlyCollection<KeyValuePair<TKey, TValue>>> GetFromCache()
            {
                if (_skipCacheGetPredicate is null)
                    return _cache.GetMany(keysCollection);
                    
                var filteredKeys = CacheKeysFilter<TKey>.Filter(keysCollection, _skipCacheGetPredicate, out var pooledArray);

                try
                {
                    return filteredKeys.Count > 0
                        ? _cache.GetMany(filteredKeys)
                        : new ValueTask<IReadOnlyCollection<KeyValuePair<TKey, TValue>>>(EmptyValuesCollection);
                }
                finally
                {
                    CacheKeysFilter<TKey>.ReturnPooledArray(pooledArray);
                }
            }

            ValueTask SetInCache()
            {
                var timeToLive = _timeToLiveFactory(keysCollection);

                if (timeToLive == TimeSpan.Zero)
                    return default;
                
                if (_skipCacheSetPredicate is null)
                    return _cache.SetMany(valuesFromFuncCollection, timeToLive);
                
                var filteredValues = CacheValuesFilter<TKey, TValue>.Filter(valuesFromFuncCollection, _skipCacheSetPredicate, out var pooledArray);

                try
                {
                    if (filteredValues.Count > 0)
                        _cache.SetMany(filteredValues, timeToLive);
                }
                finally
                {
                    CacheValuesFilter<TKey, TValue>.ReturnPooledArray(pooledArray);
                }

                return default;
            }
        }
    }
}