using System;
using System.Buffers;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CacheMeIfYouCan.Internal.CachedFunctions.Configuration;

namespace CacheMeIfYouCan.Internal.CachedFunctions
{
    internal sealed class CachedFunctionWithEnumerableKeys<TKey, TValue>
    {
        private readonly Func<IReadOnlyCollection<TKey>, CancellationToken, Task<IEnumerable<KeyValuePair<TKey, TValue>>>> _originalFunction;
        private readonly Func<IReadOnlyCollection<TKey>, TimeSpan> _timeToLiveFactory;
        private readonly IEqualityComparer<TKey> _keyComparer;
        private readonly Func<TKey, bool> _skipCacheGetPredicate;
        private readonly Func<TKey, TValue, bool> _skipCacheSetPredicate;
        private readonly int _maxBatchSize;
        private readonly BatchBehaviour _batchBehaviour;
        private readonly bool _shouldFillMissingKeys;
        private readonly bool _shouldFillMissingKeysWithConstantValue;
        private readonly TValue _fillMissingKeysConstantValue;
        private readonly Func<TKey, TValue> _fillMissingKeysValueFactory;
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
                
                _cache = CacheBuilder.Build(
                    config,
                    out var additionalSkipCacheGetPredicate,
                    out var additionalSkipCacheSetPredicate);
                
                _skipCacheGetPredicate = config.SkipCacheGetPredicate.Or(additionalSkipCacheGetPredicate);
                _skipCacheSetPredicate = config.SkipCacheSetPredicate.Or(additionalSkipCacheSetPredicate);
            }
            
            _maxBatchSize = config.MaxBatchSize;
            _batchBehaviour = config.BatchBehaviour;

            if (config.FillMissingKeysConstantValue.IsSet)
            {
                _shouldFillMissingKeys = true;
                _shouldFillMissingKeysWithConstantValue = true;
                _fillMissingKeysConstantValue = config.FillMissingKeysConstantValue.Value;
            }
            else if (!(config.FillMissingKeysValueFactory is null))
            {
                _shouldFillMissingKeys = true;
                _fillMissingKeysValueFactory = config.FillMissingKeysValueFactory;
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

            var resultsDictionary = getFromCacheTask.Result;

            if (resultsDictionary.Count == keysCollection.Count)
                return resultsDictionary;
            
            var missingKeys = MissingKeysResolver<TKey, TValue>.GetMissingKeys(keysCollection, resultsDictionary);

            if (missingKeys.Count < _maxBatchSize)
            {
                await GetValuesFromFunc(missingKeys, cancellationToken, resultsDictionary).ConfigureAwait(false);
            }
            else
            {
                var batches = BatchingHelper.Batch(missingKeys, _maxBatchSize, _batchBehaviour);
                
                var tasks = new Task[batches.Count];
                var resultsDictionaryLock = new object();
                var index = 0;
                foreach (var batch in batches)
                {
                    tasks[index++] = GetValuesFromFunc(
                        batch,
                        cancellationToken,
                        resultsDictionary,
                        resultsDictionaryLock);
                }

                await Task.WhenAll(tasks).ConfigureAwait(false);
            }

            return resultsDictionary;

            async ValueTask<Dictionary<TKey, TValue>> GetFromCache()
            {
                var getFromCacheResultsArray = ArrayPool<KeyValuePair<TKey, TValue>>.Shared.Rent(keysCollection.Count);
                Dictionary<TKey, TValue> resultsFromCache;

                try
                {
                    int countFoundInCache;
                    if (_skipCacheGetPredicate is null)
                    {
                        var task = _cache.GetMany(keysCollection, getFromCacheResultsArray);

                        if (!task.IsCompleted)
                            await task.ConfigureAwait(false);

                        countFoundInCache = task.Result;
                    }
                    else
                    {
                        var filteredKeys = CacheKeysFilter<TKey>.Filter(
                            keysCollection,
                            _skipCacheGetPredicate,
                            out var pooledKeyArray);

                        try
                        {
                            var task = filteredKeys.Count > 0
                                ? _cache.GetMany(filteredKeys, getFromCacheResultsArray)
                                : new ValueTask<int>(0);

                            if (!task.IsCompleted)
                                await task.ConfigureAwait(false);

                            countFoundInCache = task.Result;
                        }
                        finally
                        {
                            CacheKeysFilter<TKey>.ReturnPooledArray(pooledKeyArray);
                        }
                    }

                    if (countFoundInCache == 0)
                    {
                        resultsFromCache = new Dictionary<TKey, TValue>(_keyComparer);
                    }
                    else
                    {
                        resultsFromCache = new Dictionary<TKey, TValue>(countFoundInCache, _keyComparer);
                        for (var i = 0; i < countFoundInCache; i++)
                        {
                            var kv = getFromCacheResultsArray[i];
                            resultsFromCache[kv.Key] = kv.Value;
                        }
                    }
                }
                finally
                {
                    ArrayPool<KeyValuePair<TKey, TValue>>.Shared.Return(getFromCacheResultsArray);
                }

                return resultsFromCache;
            }
        }

        private async Task GetValuesFromFunc(
            IReadOnlyCollection<TKey> keys,
            CancellationToken cancellationToken,
            Dictionary<TKey, TValue> resultsDictionary,
            object resultsDictionaryLock = null)
        {
            var getValuesFromFuncTask = _originalFunction(keys, cancellationToken);

            if (!getValuesFromFuncTask.IsCompleted)
                await getValuesFromFuncTask.ConfigureAwait(false);

            var valuesFromFunc = getValuesFromFuncTask.Result;

            if (_shouldFillMissingKeys)
                valuesFromFunc = FillMissingKeys(keys, valuesFromFunc);
            
            if (valuesFromFunc is null)
                return;

            var valuesFromFuncCollection = valuesFromFunc.ToReadOnlyCollection();
            if (valuesFromFuncCollection.Count == 0)
                return;

            var setInCacheTask = SetInCache();

            cancellationToken.ThrowIfCancellationRequested();
            
            var lockTaken = false;
            if (!(resultsDictionaryLock is null))
                Monitor.TryEnter(resultsDictionaryLock, ref lockTaken);
            
            foreach (var kv in valuesFromFuncCollection)
                resultsDictionary[kv.Key] = kv.Value;

            if (lockTaken)
                Monitor.Exit(resultsDictionaryLock);
            
            if (!setInCacheTask.IsCompleted)
                await setInCacheTask.ConfigureAwait(false);
            
            ValueTask SetInCache()
            {
                if (_skipCacheSetPredicate is null)
                {
                    var timeToLive = _timeToLiveFactory(keys);

                    return timeToLive == TimeSpan.Zero
                        ? default
                        : _cache.SetMany(valuesFromFuncCollection, timeToLive);
                }

                var filteredValues = CacheValuesFilter<TKey, TValue>.Filter(
                    valuesFromFuncCollection,
                    _skipCacheSetPredicate,
                    out var pooledArray);

                try
                {
                    if (filteredValues.Count > 0)
                    {
                        var filteredKeys = filteredValues.Count == keys.Count
                            ? keys
                            : new ReadOnlyCollectionKeyValuePairKeys<TKey, TValue>(filteredValues);
                        
                        var timeToLive = _timeToLiveFactory(filteredKeys);

                        return timeToLive == TimeSpan.Zero
                            ? default
                            : _cache.SetMany(filteredValues, timeToLive);
                    }
                }
                finally
                {
                    CacheValuesFilter<TKey, TValue>.ReturnPooledArray(pooledArray);
                }

                return default;
            }
        }

        private Dictionary<TKey, TValue> FillMissingKeys(
            IReadOnlyCollection<TKey> keys,
            IEnumerable<KeyValuePair<TKey, TValue>> valuesFromFunc)
        {
            if (!(valuesFromFunc is Dictionary<TKey, TValue> valuesDictionary && valuesDictionary.Comparer.Equals(_keyComparer)))
            {
                valuesDictionary = new Dictionary<TKey, TValue>(keys.Count, _keyComparer);

                if (!(valuesFromFunc is null))
                {
                    foreach (var kv in valuesFromFunc)
                        valuesDictionary[kv.Key] = kv.Value;
                }
            }
            
            foreach (var key in keys)
            {
                if (valuesDictionary.ContainsKey(key))
                    continue;
                
                var value = _shouldFillMissingKeysWithConstantValue
                    ? _fillMissingKeysConstantValue
                    : _fillMissingKeysValueFactory(key);
                
                valuesDictionary[key] = value;
            }

            return valuesDictionary;
        }
    }
}