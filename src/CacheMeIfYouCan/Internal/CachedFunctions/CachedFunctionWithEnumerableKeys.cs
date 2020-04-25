using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using CacheMeIfYouCan.Events.CachedFunction.EnumerableKeys;
using CacheMeIfYouCan.Internal.CachedFunctions.Configuration;

namespace CacheMeIfYouCan.Internal.CachedFunctions
{
    internal sealed class CachedFunctionWithEnumerableKeys<TParams, TKey, TValue>
    {
        private readonly Func<TParams, IReadOnlyCollection<TKey>, CancellationToken, ValueTask<IEnumerable<KeyValuePair<TKey, TValue>>>> _originalFunction;
        private readonly TimeSpan _timeToLive;
        private readonly Func<TParams, IReadOnlyCollection<TKey>, TimeSpan> _timeToLiveFactory;
        private readonly IEqualityComparer<TKey> _keyComparer;
        private readonly Func<TParams, bool> _skipCacheGetOuterPredicate;
        private readonly Func<TParams, TKey, bool> _skipCacheGetInnerPredicate;
        private readonly Func<TParams, bool> _skipCacheSetOuterPredicate;
        private readonly Func<TParams, TKey, TValue, bool> _skipCacheSetInnerPredicate;
        private readonly int _maxBatchSize;
        private readonly BatchBehaviour _batchBehaviour;
        private readonly bool _shouldFillMissingKeys;
        private readonly bool _shouldFillMissingKeysWithConstantValue;
        private readonly TValue _fillMissingKeysConstantValue;
        private readonly Func<TKey, TValue> _fillMissingKeysValueFactory;
        private readonly ICache<TKey, TValue> _cache;
        private readonly Action<SuccessfulRequestEvent<TParams, TKey, TValue>> _onSuccessAction;
        private readonly Action<ExceptionEvent<TParams, TKey>> _onExceptionAction;
        private readonly bool _cacheEnabled;

        public CachedFunctionWithEnumerableKeys(
            Func<TParams, IReadOnlyCollection<TKey>, CancellationToken, ValueTask<IEnumerable<KeyValuePair<TKey, TValue>>>> originalFunction,
            CachedFunctionWithEnumerableKeysConfiguration<TParams, TKey, TValue> config)
        {
            _originalFunction = originalFunction;
            _maxBatchSize = config.MaxBatchSize;
            _batchBehaviour = config.BatchBehaviour;
            _onSuccessAction = config.OnSuccessAction;
            _onExceptionAction = config.OnExceptionAction;

            if (config.DisableCaching)
            {
                _cache = NullCache<TKey, TValue>.Instance;
            }
            else
            {
                if (config.TimeToLive.HasValue)
                    _timeToLive = config.TimeToLive.Value;
                else
                    _timeToLiveFactory = config.TimeToLiveFactory;

                _cache = CacheBuilder.Build(config);
                _keyComparer = config.KeyComparer ?? EqualityComparer<TKey>.Default;
                _skipCacheGetOuterPredicate = config.SkipCacheGetOuterPredicate;
                _skipCacheGetInnerPredicate = config.SkipCacheGetInnerPredicate;
                _skipCacheSetOuterPredicate = config.SkipCacheSetOuterPredicate;
                _skipCacheSetInnerPredicate = config.SkipCacheSetInnerPredicate;
            }

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
            
            _cacheEnabled = _cache.LocalCacheEnabled || _cache.DistributedCacheEnabled;
        }

        public async ValueTask<Dictionary<TKey, TValue>> GetMany(
            TParams parameters,
            IEnumerable<TKey> keys,
            CancellationToken cancellationToken)
        {
            var start = DateTime.UtcNow;
            var timer = Stopwatch.StartNew();
            var keysCollection = keys.ToReadOnlyCollection();
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                var getFromCacheTask = GetFromCache(parameters, keysCollection);

                var (resultsDictionary, cacheStats) = getFromCacheTask.IsCompleted
                    ? getFromCacheTask.Result
                    : await getFromCacheTask.ConfigureAwait(false);

                var cacheHits = resultsDictionary.Count;

                if (cacheHits == keysCollection.Count)
                {
                    _onSuccessAction?.Invoke(new SuccessfulRequestEvent<TParams, TKey, TValue>(
                        parameters,
                        keysCollection,
                        resultsDictionary,
                        start,
                        timer.Elapsed,
                        cacheStats));

                    return resultsDictionary;
                }

                var missingKeys = MissingKeysResolver<TKey, TValue>.GetMissingKeys(keysCollection, resultsDictionary);

                if (missingKeys.Count < _maxBatchSize)
                {
                    var getValuesFromFuncTask = GetValuesFromFunc(
                        parameters,
                        missingKeys,
                        cancellationToken,
                        resultsDictionary);

                    if (!getValuesFromFuncTask.IsCompletedSuccessfully)
                        await getValuesFromFuncTask.ConfigureAwait(false);
                }
                else
                {
                    var batches = BatchingHelper.Batch(missingKeys, _maxBatchSize, _batchBehaviour);

                    Task[] tasks = null;
                    var resultsDictionaryLock = new object();
                    var tasksIndex = 0;
                    for (var batchIndex = 0; batchIndex < batches.Length; batchIndex++)
                    {
                        var getValuesFromFuncTask = GetValuesFromFunc(
                            parameters,
                            batches[batchIndex],
                            cancellationToken,
                            resultsDictionary,
                            resultsDictionaryLock);

                        if (getValuesFromFuncTask.IsCompletedSuccessfully)
                            continue;

                        if (tasks is null)
                            tasks = new Task[batches.Length - batchIndex];

                        tasks[tasksIndex++] = getValuesFromFuncTask.AsTask();
                    }

                    if (!(tasks is null))
                        await Task.WhenAll(new ArraySegment<Task>(tasks, 0, tasksIndex)).ConfigureAwait(false);
                }

                _onSuccessAction?.Invoke(new SuccessfulRequestEvent<TParams, TKey, TValue>(
                    parameters,
                    keysCollection,
                    resultsDictionary,
                    start,
                    timer.Elapsed,
                    cacheStats));

                return resultsDictionary;
            }
            catch (Exception ex) when (!(_onExceptionAction is null))
            {
                _onExceptionAction?.Invoke(new ExceptionEvent<TParams, TKey>(
                    parameters,
                    keysCollection,
                    start,
                    timer.Elapsed,
                    ex));

                throw;
            }
        }

        private async ValueTask<(Dictionary<TKey, TValue> Result, CacheGetManyStats CacheStats)> GetFromCache(TParams parameters, IReadOnlyCollection<TKey> keysCollection)
        {
            if (!_cacheEnabled)
                return (new Dictionary<TKey, TValue>(_keyComparer), default);
            
            if (_skipCacheGetOuterPredicate?.Invoke(parameters) == true)
                return GetResultIfAllKeysSkipped();

            if (_skipCacheGetInnerPredicate is null)
            {
                var task = GetFromCacheInner(keysCollection, 0);

                return task.IsCompleted
                    ? task.Result
                    : await task.ConfigureAwait(false);
            } 
            
            var filteredKeys = CacheKeysFilter<TParams, TKey>.Filter(
                parameters,
                keysCollection,
                _skipCacheGetInnerPredicate,
                out var pooledKeyArray);

            try
            {
                if (filteredKeys.Count == 0)
                    return GetResultIfAllKeysSkipped();

                var task = GetFromCacheInner(filteredKeys, keysCollection.Count - filteredKeys.Count);
                
                return task.IsCompleted
                    ? task.Result
                    : await task.ConfigureAwait(false);
            }
            finally
            {
                if (!(pooledKeyArray is null))
                    CacheKeysFilter<TKey>.ReturnPooledArray(pooledKeyArray);
            }

            (Dictionary<TKey, TValue>, CacheGetManyStats) GetResultIfAllKeysSkipped()
            {
                var cacheStats = new CacheGetManyStats(
                    cacheKeysRequested: 0,
                    cacheKeysSkipped: keysCollection.Count,
                    localCacheEnabled: _cache.LocalCacheEnabled,
                    distributedCacheEnabled: _cache.DistributedCacheEnabled);
                
                return (new Dictionary<TKey, TValue>(_keyComparer), cacheStats);
            }
            
            async ValueTask<(Dictionary<TKey, TValue> Results, CacheGetManyStats CacheStats)> GetFromCacheInner(IReadOnlyCollection<TKey> keys, int cacheKeysSkipped)
            {
                var getFromCacheResultsArray = ArrayPool<KeyValuePair<TKey, TValue>>.Shared.Rent(keys.Count);
                try
                {
                    var task = _cache.GetMany(keys, cacheKeysSkipped, getFromCacheResultsArray);

                    var cacheStats = task.IsCompleted
                        ? task.Result
                        : await task.ConfigureAwait(false);

                    var countFoundInCache = cacheStats.CacheHits;
                    if (countFoundInCache == 0)
                        return (new Dictionary<TKey, TValue>(_keyComparer), cacheStats);

                    var resultsFromCache = new Dictionary<TKey, TValue>(countFoundInCache, _keyComparer);
                    for (var i = 0; i < countFoundInCache; i++)
                    {
                        var kv = getFromCacheResultsArray[i];
                        resultsFromCache[kv.Key] = kv.Value;
                    }

                    return (resultsFromCache, cacheStats);
                }
                finally
                {
                    ArrayPool<KeyValuePair<TKey, TValue>>.Shared.Return(getFromCacheResultsArray);
                }
            }
        }

        private async ValueTask GetValuesFromFunc(
            TParams parameters,
            IReadOnlyCollection<TKey> keys,
            CancellationToken cancellationToken,
            Dictionary<TKey, TValue> resultsDictionary,
            object resultsDictionaryLock = null)
        {
            var getValuesFromFuncTask = _originalFunction(parameters, keys, cancellationToken);

            var valuesFromFunc = getValuesFromFuncTask.IsCompleted
                ? getValuesFromFuncTask.Result
                : await getValuesFromFuncTask.ConfigureAwait(false);

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
                if (!_cacheEnabled || _skipCacheSetOuterPredicate?.Invoke(parameters) == true)
                    return default;
                
                if (_skipCacheSetInnerPredicate is null)
                {
                    var timeToLive = _timeToLiveFactory?.Invoke(parameters, keys) ?? _timeToLive;

                    return timeToLive == TimeSpan.Zero
                        ? default
                        : _cache.SetMany(valuesFromFuncCollection, timeToLive);
                }

                var filteredValues = CacheValuesFilter<TParams, TKey, TValue>.Filter(
                    parameters,
                    valuesFromFuncCollection,
                    _skipCacheSetInnerPredicate,
                    out var pooledArray);

                try
                {
                    if (filteredValues.Count > 0)
                    {
                        var filteredKeys = filteredValues.Count == keys.Count
                            ? keys
                            : new ReadOnlyCollectionKeyValuePairKeys<TKey, TValue>(filteredValues);
                        
                        var timeToLive = _timeToLiveFactory?.Invoke(parameters, filteredKeys) ?? _timeToLive;

                        return timeToLive == TimeSpan.Zero
                            ? default
                            : _cache.SetMany(filteredValues, timeToLive);
                    }
                }
                finally
                {
                    if (!(pooledArray is null))
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