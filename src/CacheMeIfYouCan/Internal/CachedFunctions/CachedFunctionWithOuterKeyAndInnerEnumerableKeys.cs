using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CacheMeIfYouCan.Internal.CachedFunctions
{
    internal sealed class CachedFunctionWithOuterKeyAndInnerEnumerableKeys<TOuterKey, TInnerKey, TValue>
    {
        private readonly Func<TOuterKey, IReadOnlyCollection<TInnerKey>, CancellationToken, Task<IEnumerable<KeyValuePair<TInnerKey, TValue>>>> _originalFunction;
        private readonly Func<TOuterKey, IReadOnlyCollection<TInnerKey>, TimeSpan> _timeToLiveFactory;
        private readonly IEqualityComparer<TInnerKey> _keyComparer;
        private readonly Func<TOuterKey, bool> _skipCacheGetPredicateOuterKeyOnly;
        private readonly Func<TOuterKey, TInnerKey, bool> _skipCacheGetPredicate;
        private readonly Func<TOuterKey, bool> _skipCacheSetPredicateOuterKeyOnly;
        private readonly Func<TOuterKey, TInnerKey, TValue, bool> _skipCacheSetPredicate;
        private readonly ICache<TOuterKey, TInnerKey, TValue> _cache;
        private static readonly IReadOnlyCollection<KeyValuePair<TInnerKey, TValue>> EmptyValuesCollection = new List<KeyValuePair<TInnerKey, TValue>>();

        public CachedFunctionWithOuterKeyAndInnerEnumerableKeys(
            Func<TOuterKey, IReadOnlyCollection<TInnerKey>, CancellationToken, Task<IEnumerable<KeyValuePair<TInnerKey, TValue>>>> originalFunction,
            CachedFunctionWithOuterKeyAndInnerEnumerableKeysConfiguration<TOuterKey, TInnerKey, TValue> config)
        {
            _originalFunction = originalFunction;
            
            if (config.DisableCaching)
            {
                _timeToLiveFactory = (_, __) => TimeSpan.Zero;
                _cache = NullCache<TOuterKey, TInnerKey, TValue>.Instance;
            }
            else
            {
                _timeToLiveFactory = config.TimeToLiveFactory;
                _keyComparer = config.KeyComparer ?? EqualityComparer<TInnerKey>.Default;
                _skipCacheGetPredicateOuterKeyOnly = config.SkipCacheGetPredicateOuterKeyOnly;
                _skipCacheGetPredicate = config.SkipCacheGetPredicate;
                _skipCacheSetPredicateOuterKeyOnly = config.SkipCacheSetPredicateOuterKeyOnly;
                _skipCacheSetPredicate = config.SkipCacheSetPredicate;
                _cache = CacheBuilder.Build(config);
            }
        }

        public async Task<Dictionary<TInnerKey, TValue>> GetMany(
            TOuterKey outerKey,
            IEnumerable<TInnerKey> innerKeys,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            var innerKeysCollection = innerKeys.ToReadOnlyCollection();

            var getFromCacheTask = GetFromCache();

            if (!getFromCacheTask.IsCompleted)
                await getFromCacheTask.ConfigureAwait(false);

            var fromCache = getFromCacheTask.Result;
            
            var resultsDictionary = fromCache is null || fromCache.Count == 0
                ? new Dictionary<TInnerKey, TValue>(_keyComparer)
                : fromCache.ToDictionary(kv => kv.Key, kv => kv.Value, _keyComparer);

            var missingKeys = MissingKeysResolver<TInnerKey, TValue>.GetMissingKeys(innerKeysCollection, resultsDictionary);

            if (missingKeys is null)
                return resultsDictionary;
            
            var getValuesFromFuncTask = _originalFunction(outerKey, missingKeys, cancellationToken);

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
            
            ValueTask<IReadOnlyCollection<KeyValuePair<TInnerKey, TValue>>> GetFromCache()
            {
                if (!(_skipCacheGetPredicateOuterKeyOnly is null) && _skipCacheGetPredicateOuterKeyOnly(outerKey))
                    return new ValueTask<IReadOnlyCollection<KeyValuePair<TInnerKey, TValue>>>(EmptyValuesCollection);
                
                if (_skipCacheGetPredicate is null)
                    return _cache.GetMany(outerKey, innerKeysCollection);
                    
                var filteredKeys = CacheKeysFilter<TOuterKey, TInnerKey>.Filter(
                    outerKey,
                    innerKeysCollection,
                    _skipCacheGetPredicate,
                    out var pooledArray);

                try
                {
                    return filteredKeys.Count > 0
                        ? _cache.GetMany(outerKey, filteredKeys)
                        : new ValueTask<IReadOnlyCollection<KeyValuePair<TInnerKey, TValue>>>(EmptyValuesCollection);
                }
                finally
                {
                    CacheKeysFilter<TInnerKey>.ReturnPooledArray(pooledArray);
                }
            }

            ValueTask SetInCache()
            {
                if (!(_skipCacheSetPredicateOuterKeyOnly is null) && _skipCacheSetPredicateOuterKeyOnly(outerKey))
                    return default;
                
                var timeToLive = _timeToLiveFactory(outerKey, innerKeysCollection);

                if (timeToLive == TimeSpan.Zero)
                    return default;
                
                if (_skipCacheSetPredicate is null)
                    return _cache.SetMany(outerKey, valuesFromFuncCollection, timeToLive);
                
                var filteredValues = CacheValuesFilter<TOuterKey, TInnerKey, TValue>.Filter(
                    outerKey,
                    valuesFromFuncCollection,
                    _skipCacheSetPredicate,
                    out var pooledArray);

                try
                {
                    if (filteredValues.Count > 0)
                        _cache.SetMany(outerKey, filteredValues, timeToLive);
                }
                finally
                {
                    CacheValuesFilter<TInnerKey, TValue>.ReturnPooledArray(pooledArray);
                }

                return default;
            }
        }
    }
}