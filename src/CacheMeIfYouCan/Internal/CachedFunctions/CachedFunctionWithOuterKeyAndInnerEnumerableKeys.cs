using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CacheMeIfYouCan.Events.CachedFunction.OuterKeyAndInnerEnumerableKeys;
using CacheMeIfYouCan.Internal.CachedFunctions.Configuration;

namespace CacheMeIfYouCan.Internal.CachedFunctions
{
    internal sealed class CachedFunctionWithOuterKeyAndInnerEnumerableKeys<TParams, TOuterKey, TInnerKey, TValue>
    {
        private readonly Func<TParams, IReadOnlyCollection<TInnerKey>, CancellationToken, ValueTask<IEnumerable<KeyValuePair<TInnerKey, TValue>>>> _originalFunction;
        private readonly Func<TParams, TOuterKey> _keySelector;
        private readonly TimeSpan _timeToLive;
        private readonly Func<TParams, IReadOnlyCollection<TInnerKey>, TimeSpan> _timeToLiveFactory;
        private readonly IEqualityComparer<TInnerKey> _keyComparer;
        private readonly Func<TParams, bool> _skipCacheGetOuterPredicate;
        private readonly Func<TParams, TInnerKey, bool> _skipCacheGetInnerPredicate;
        private readonly Func<TParams, bool> _skipCacheSetOuterPredicate;
        private readonly Func<TParams, TInnerKey, TValue, bool> _skipCacheSetInnerPredicate;
        private readonly int _maxBatchSize;
        private readonly BatchBehaviour _batchBehaviour;
        private readonly bool _shouldFillMissingKeys;
        private readonly bool _shouldFillMissingKeysWithConstantValue;
        private readonly TValue _fillMissingKeysConstantValue;
        private readonly Func<TOuterKey, TInnerKey, TValue> _fillMissingKeysValueFactory;
        private readonly ICache<TOuterKey, TInnerKey, TValue> _cache;
        private readonly Action<SuccessfulRequestEvent<TParams, TOuterKey, TInnerKey, TValue>> _onSuccessAction;
        private readonly Action<ExceptionEvent<TParams, TOuterKey, TInnerKey>> _onExceptionAction;
        private readonly bool _cacheEnabled;

        public CachedFunctionWithOuterKeyAndInnerEnumerableKeys(
            Func<TParams, IReadOnlyCollection<TInnerKey>, CancellationToken, ValueTask<IEnumerable<KeyValuePair<TInnerKey, TValue>>>> originalFunction,
            Func<TParams, TOuterKey> keySelector,
            CachedFunctionWithOuterKeyAndInnerEnumerableKeysConfiguration<TParams, TOuterKey, TInnerKey, TValue> config)
        {
            _originalFunction = originalFunction;
            _keySelector = keySelector;
            _maxBatchSize = config.MaxBatchSize;
            _batchBehaviour = config.BatchBehaviour;
            _onSuccessAction = config.OnSuccessAction;
            _onExceptionAction = config.OnExceptionAction;

            if (config.DisableCaching)
            {
                _cache = NullCache<TOuterKey, TInnerKey, TValue>.Instance;
            }
            else
            {
                if (config.TimeToLive.HasValue)
                    _timeToLive = config.TimeToLive.Value;
                else
                    _timeToLiveFactory = config.TimeToLiveFactory;
                
                _keyComparer = config.KeyComparer ?? EqualityComparer<TInnerKey>.Default;
                
                _cache = CacheBuilder.Build(config);
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

        public async ValueTask<Dictionary<TInnerKey, TValue>> GetMany(
            TParams parameters,
            IEnumerable<TInnerKey> innerKeys,
            CancellationToken cancellationToken)
        {
            var start = DateTime.UtcNow;
            var timer = Stopwatch.StartNew();
            TOuterKey outerKey = default;
            var innerKeysCollection = innerKeys.ToReadOnlyCollection();
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                outerKey = _keySelector(parameters);

                var getFromCacheTask = GetFromCache(parameters, outerKey, innerKeysCollection);

                var (resultsDictionary, cacheStats) = getFromCacheTask.IsCompleted
                    ? getFromCacheTask.Result
                    : await getFromCacheTask.ConfigureAwait(false);

                if (cacheStats.CacheHits == innerKeysCollection.Count)
                {
                    _onSuccessAction?.Invoke(new SuccessfulRequestEvent<TParams, TOuterKey, TInnerKey, TValue>(
                        parameters,
                        outerKey,
                        innerKeysCollection,
                        resultsDictionary,
                        start,
                        timer.Elapsed,
                        cacheStats));

                    return resultsDictionary;
                }

                var missingKeys =
                    MissingKeysResolver<TInnerKey, TValue>.GetMissingKeys(innerKeysCollection, resultsDictionary);

                if (missingKeys.Count < _maxBatchSize)
                {
                    var getValuesFromFuncTask = GetValuesFromFunc(
                        parameters,
                        outerKey,
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
                            outerKey,
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

                _onSuccessAction?.Invoke(new SuccessfulRequestEvent<TParams, TOuterKey, TInnerKey, TValue>(
                    parameters,
                    outerKey,
                    innerKeysCollection,
                    resultsDictionary,
                    start,
                    timer.Elapsed,
                    cacheStats));

                return resultsDictionary;
            }
            catch (Exception ex) when (!(_onExceptionAction is null))
            {
                _onExceptionAction?.Invoke(new ExceptionEvent<TParams, TOuterKey, TInnerKey>(
                    parameters,
                    outerKey,
                    innerKeysCollection,
                    start,
                    timer.Elapsed,
                    ex));

                throw;
            }
        }

        private async ValueTask<(Dictionary<TInnerKey, TValue> Results, CacheGetManyStats CacheStats)> GetFromCache(
            TParams parameters, TOuterKey outerKey, IReadOnlyCollection<TInnerKey> innerKeysCollection)
        {
            if (!_cacheEnabled)
                return (new Dictionary<TInnerKey, TValue>(_keyComparer), default);

            if (_skipCacheGetOuterPredicate?.Invoke(parameters) == true)
                return GetResultIfAllKeysSkipped();

            if (_skipCacheGetInnerPredicate is null)
            {
                var task = GetFromCacheInner(innerKeysCollection, 0);

                return task.IsCompleted
                    ? task.Result
                    : await task.ConfigureAwait(false);
            }

            var filteredKeys = CacheKeysFilter<TParams, TInnerKey>.Filter(
                parameters,
                innerKeysCollection,
                _skipCacheGetInnerPredicate,
                out var pooledKeyArray);

            try
            {
                if (filteredKeys.Count == 0)
                    return GetResultIfAllKeysSkipped();

                var task = GetFromCacheInner(filteredKeys, innerKeysCollection.Count - filteredKeys.Count);

                return task.IsCompleted
                    ? task.Result
                    : await task.ConfigureAwait(false);
            }
            finally
            {
                if (!(pooledKeyArray is null))
                    CacheKeysFilter<TInnerKey>.ReturnPooledArray(pooledKeyArray);
            }

            (Dictionary<TInnerKey, TValue>, CacheGetManyStats) GetResultIfAllKeysSkipped()
            {
                var cacheStats = new CacheGetManyStats(
                    cacheKeysRequested: 0,
                    cacheKeysSkipped: innerKeysCollection.Count,
                    localCacheEnabled: _cache.LocalCacheEnabled,
                    distributedCacheEnabled: _cache.DistributedCacheEnabled);

                return (new Dictionary<TInnerKey, TValue>(_keyComparer), cacheStats);
            }

            async ValueTask<(Dictionary<TInnerKey, TValue> Results, CacheGetManyStats CacheStats)> GetFromCacheInner(
                IReadOnlyCollection<TInnerKey> keys, int cacheKeysSkipped)
            {
                var getFromCacheResultsArray = ArrayPool<KeyValuePair<TInnerKey, TValue>>.Shared.Rent(keys.Count);
                try
                {
                    var task = _cache.GetMany(outerKey, keys, cacheKeysSkipped, getFromCacheResultsArray);

                    var cacheStats = task.IsCompleted
                        ? task.Result
                        : await task.ConfigureAwait(false);

                    var countFoundInCache = cacheStats.CacheHits;
                    if (countFoundInCache == 0)
                        return (new Dictionary<TInnerKey, TValue>(_keyComparer), cacheStats);

                    var resultsFromCache = new Dictionary<TInnerKey, TValue>(countFoundInCache, _keyComparer);
                    for (var i = 0; i < countFoundInCache; i++)
                    {
                        var kv = getFromCacheResultsArray[i];
                        resultsFromCache[kv.Key] = kv.Value;
                    }

                    return (resultsFromCache, cacheStats);
                }
                finally
                {
                    ArrayPool<KeyValuePair<TInnerKey, TValue>>.Shared.Return(getFromCacheResultsArray);
                }
            }
        }

        private async ValueTask GetValuesFromFunc(
            TParams parameters,
            TOuterKey outerKey,
            IReadOnlyCollection<TInnerKey> innerKeys,
            CancellationToken cancellationToken,
            Dictionary<TInnerKey, TValue> resultsDictionary,
            object resultsDictionaryLock = null)
        {
            var getValuesFromFuncTask = _originalFunction(parameters, innerKeys, cancellationToken);

            var valuesFromFunc = getValuesFromFuncTask.IsCompleted
                ? getValuesFromFuncTask.Result
                : await getValuesFromFuncTask.ConfigureAwait(false);

            if (_shouldFillMissingKeys)
                valuesFromFunc = FillMissingKeys(outerKey, innerKeys, valuesFromFunc);
            
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
                    var timeToLive = _timeToLiveFactory?.Invoke(parameters, innerKeys) ?? _timeToLive;

                    return timeToLive == TimeSpan.Zero
                        ? default
                        : _cache.SetMany(outerKey, valuesFromFuncCollection, timeToLive);
                }

                var filteredValues = CacheValuesFilter<TParams, TInnerKey, TValue>.Filter(
                    parameters,
                    valuesFromFuncCollection,
                    _skipCacheSetInnerPredicate,
                    out var pooledArray);

                try
                {
                    if (filteredValues.Count > 0)
                    {
                        var filteredKeys = filteredValues.Count == innerKeys.Count
                            ? innerKeys
                            : new ReadOnlyCollectionKeyValuePairKeys<TInnerKey, TValue>(filteredValues);
                        
                        var timeToLive = _timeToLiveFactory?.Invoke(parameters, filteredKeys) ?? _timeToLive;

                        return timeToLive == TimeSpan.Zero
                            ? default
                            : _cache.SetMany(outerKey, filteredValues, timeToLive);
                    }
                }
                finally
                {
                    if (!(pooledArray is null))
                        CacheValuesFilter<TInnerKey, TValue>.ReturnPooledArray(pooledArray);
                }

                return default;
            }
        }
        
        private Dictionary<TInnerKey, TValue> FillMissingKeys(
            TOuterKey outerKey,
            IReadOnlyCollection<TInnerKey> innerKeys,
            IEnumerable<KeyValuePair<TInnerKey, TValue>> valuesFromFunc)
        {
            if (!(valuesFromFunc is Dictionary<TInnerKey, TValue> valuesDictionary && valuesDictionary.Comparer.Equals(_keyComparer)))
            {
                valuesDictionary = new Dictionary<TInnerKey, TValue>(innerKeys.Count, _keyComparer);

                if (!(valuesFromFunc is null))
                {
                    foreach (var kv in valuesFromFunc)
                        valuesDictionary[kv.Key] = kv.Value;
                }
            }
            
            foreach (var innerKey in innerKeys)
            {
                if (valuesDictionary.ContainsKey(innerKey))
                    continue;
                
                var value = _shouldFillMissingKeysWithConstantValue
                    ? _fillMissingKeysConstantValue
                    : _fillMissingKeysValueFactory(outerKey, innerKey);
                
                valuesDictionary[innerKey] = value;
            }

            return valuesDictionary;
        }
    }
}