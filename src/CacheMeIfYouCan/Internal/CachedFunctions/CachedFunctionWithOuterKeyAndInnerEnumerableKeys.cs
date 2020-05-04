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
        private readonly Func<TParams, ReadOnlyMemory<TInnerKey>, CancellationToken, ValueTask<IEnumerable<KeyValuePair<TInnerKey, TValue>>>> _originalFunction;
        private readonly Func<TParams, TOuterKey> _keySelector;
        private readonly TimeSpan _timeToLive;
        private readonly Func<TParams, ReadOnlyMemory<TInnerKey>, TimeSpan> _timeToLiveFactory;
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
            Func<TParams, ReadOnlyMemory<TInnerKey>, CancellationToken, ValueTask<IEnumerable<KeyValuePair<TInnerKey, TValue>>>> originalFunction,
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

                _cache = CacheBuilder.Build(config);
                _keyComparer = config.KeyComparer ?? EqualityComparer<TInnerKey>.Default;
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
            var timer = StopwatchStruct.StartNew();
            TOuterKey outerKey = default;
            var innerKeysMemory = innerKeys.ToReadOnlyMemory(out var pooledKeysArray);
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                outerKey = _keySelector(parameters);

                var getFromCacheTask = GetFromCache(parameters, outerKey, innerKeysMemory);

                var (resultsDictionary, cacheStats) = getFromCacheTask.IsCompleted
                    ? getFromCacheTask.Result
                    : await getFromCacheTask.ConfigureAwait(false);

                if (cacheStats.CacheHits == innerKeysMemory.Length)
                {
                    _onSuccessAction?.Invoke(new SuccessfulRequestEvent<TParams, TOuterKey, TInnerKey, TValue>(
                        parameters,
                        outerKey,
                        innerKeysMemory,
                        resultsDictionary,
                        start,
                        timer.Elapsed,
                        cacheStats));

                    return resultsDictionary;
                }

                var missingKeys = MissingKeysResolver<TInnerKey, TValue>.GetMissingKeys(
                    innerKeysMemory,
                    resultsDictionary,
                    out var pooledMissingKeysArray);

                try
                {
                    if (missingKeys.Length < _maxBatchSize)
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
                        var batchSizes = BatchingHelper.GetBatchSizes(missingKeys.Length, _maxBatchSize, _batchBehaviour);

                        Task[] tasks = null;
                        var resultsDictionaryLock = new object();
                        var tasksIndex = 0;
                        var totalKeysBatchedCount = 0;
                        for (var batchIndex = 0; batchIndex < batchSizes.Length; batchIndex++)
                        {
                            var keysStartIndex = totalKeysBatchedCount;
                            var batchSize = batchSizes[batchIndex];

                            totalKeysBatchedCount += batchSize;

                            var getValuesFromFuncTask = GetValuesFromFunc(
                                parameters,
                                outerKey,
                                innerKeysMemory.Slice(keysStartIndex, batchSize),
                                cancellationToken,
                                resultsDictionary,
                                resultsDictionaryLock);

                            if (getValuesFromFuncTask.IsCompletedSuccessfully)
                                continue;

                            if (tasks is null)
                                tasks = new Task[batchSizes.Length - batchIndex];

                            tasks[tasksIndex++] = getValuesFromFuncTask.AsTask();
                        }

                        if (!(tasks is null))
                            await Task.WhenAll(new ArraySegment<Task>(tasks, 0, tasksIndex)).ConfigureAwait(false);
                    }
                }
                finally
                {
                    if (!(pooledMissingKeysArray is null))
                        ArrayPool<TInnerKey>.Shared.Return(pooledMissingKeysArray);
                }

                _onSuccessAction?.Invoke(new SuccessfulRequestEvent<TParams, TOuterKey, TInnerKey, TValue>(
                    parameters,
                    outerKey,
                    innerKeysMemory,
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
                    innerKeysMemory,
                    start,
                    timer.Elapsed,
                    ex));

                throw;
            }
            finally
            {
                if (!(pooledKeysArray is null))
                    ArrayPool<TInnerKey>.Shared.Return(pooledKeysArray);
            }
        }

        private async ValueTask<(Dictionary<TInnerKey, TValue> Results, CacheGetManyStats CacheStats)> GetFromCache(
            TParams parameters, TOuterKey outerKey, ReadOnlyMemory<TInnerKey> innerKeys)
        {
            if (!_cacheEnabled)
                return (new Dictionary<TInnerKey, TValue>(_keyComparer), default);
            
            if (_skipCacheGetOuterPredicate?.Invoke(parameters) == true)
                return GetResultIfAllKeysSkipped();

            if (_skipCacheGetInnerPredicate is null)
            {
                var task = GetFromCacheInner(innerKeys, 0);

                return task.IsCompleted
                    ? task.Result
                    : await task.ConfigureAwait(false);
            }
            
            var filteredKeys = CacheKeysFilter<TParams, TInnerKey>.Filter(
                parameters,
                innerKeys,
                _skipCacheGetInnerPredicate,
                out var pooledKeyArray);

            try
            {
                if (filteredKeys.Length == 0)
                    return GetResultIfAllKeysSkipped();

                var task = GetFromCacheInner(filteredKeys, innerKeys.Length - filteredKeys.Length);
                
                return task.IsCompleted
                    ? task.Result
                    : await task.ConfigureAwait(false);
            }
            finally
            {
                if (!(pooledKeyArray is null))
                    ArrayPool<TInnerKey>.Shared.Return(pooledKeyArray);
            }

            (Dictionary<TInnerKey, TValue>, CacheGetManyStats) GetResultIfAllKeysSkipped()
            {
                var cacheStats = new CacheGetManyStats(
                    cacheKeysRequested: 0,
                    cacheKeysSkipped: innerKeys.Length,
                    localCacheEnabled: _cache.LocalCacheEnabled,
                    distributedCacheEnabled: _cache.DistributedCacheEnabled);
                
                return (new Dictionary<TInnerKey, TValue>(_keyComparer), cacheStats);
            }
            
            async ValueTask<(Dictionary<TInnerKey, TValue> Results, CacheGetManyStats CacheStats)> GetFromCacheInner(
                ReadOnlyMemory<TInnerKey> innerKeys, int cacheKeysSkipped)
            {
                using var getFromCacheResultsMemoryOwner = MemoryPool<KeyValuePair<TInnerKey, TValue>>.Shared.Rent(innerKeys.Length);
                var getFromCacheResultsMemory = getFromCacheResultsMemoryOwner.Memory;

                var task = _cache.GetMany(outerKey, innerKeys, cacheKeysSkipped, getFromCacheResultsMemory);

                var cacheStats = task.IsCompleted
                    ? task.Result
                    : await task.ConfigureAwait(false);

                var countFoundInCache = cacheStats.CacheHits;
                if (countFoundInCache == 0)
                    return (new Dictionary<TInnerKey, TValue>(_keyComparer), cacheStats);

                var resultsFromCache = new Dictionary<TInnerKey, TValue>(countFoundInCache, _keyComparer);
                for (var i = 0; i < countFoundInCache; i++)
                {
                    var kv = getFromCacheResultsMemory.Span[i];
                    resultsFromCache[kv.Key] = kv.Value;
                }

                return (resultsFromCache, cacheStats);
            }
        }

        private async ValueTask GetValuesFromFunc(
            TParams parameters,
            TOuterKey outerKey,
            ReadOnlyMemory<TInnerKey> innerKeys,
            CancellationToken cancellationToken,
            Dictionary<TInnerKey, TValue> resultsDictionary,
            object resultsDictionaryLock = null)
        {
            var getValuesFromFuncTask = _originalFunction(parameters, innerKeys, cancellationToken);

            var valuesFromFunc = getValuesFromFuncTask.IsCompleted
                ? getValuesFromFuncTask.Result
                : await getValuesFromFuncTask.ConfigureAwait(false);

            if (_shouldFillMissingKeys)
                valuesFromFunc = FillMissingKeys(outerKey, innerKeys.Span, valuesFromFunc);
            
            if (valuesFromFunc is null)
                return;

            var valuesFromFuncMemory = valuesFromFunc.ToReadOnlyMemory(out var pooledArray);
            if (valuesFromFuncMemory.Length == 0)
                return;

            try
            {
                var setInCacheTask = SetInCache();

                cancellationToken.ThrowIfCancellationRequested();

                var lockTaken = false;
                if (!(resultsDictionaryLock is null))
                    Monitor.TryEnter(resultsDictionaryLock, ref lockTaken);

                CopyResultsToDictionary(valuesFromFuncMemory.Span, resultsDictionary);

                if (lockTaken)
                    Monitor.Exit(resultsDictionaryLock);

                if (!setInCacheTask.IsCompleted)
                    await setInCacheTask.ConfigureAwait(false);
            }
            finally
            {
                if (!(pooledArray is null))
                    ArrayPool<KeyValuePair<TInnerKey, TValue>>.Shared.Return(pooledArray);
            }

            static void CopyResultsToDictionary(
                ReadOnlySpan<KeyValuePair<TInnerKey, TValue>> values,
                Dictionary<TInnerKey, TValue> dictionary)
            {
                foreach (var kv in values)
                    dictionary[kv.Key] = kv.Value;
            }

            ValueTask SetInCache()
            {
                if (!_cacheEnabled || _skipCacheSetOuterPredicate?.Invoke(parameters) == true)
                    return default;
                
                if (_skipCacheSetInnerPredicate is null)
                {
                    var timeToLive = _timeToLiveFactory?.Invoke(parameters, innerKeys) ?? _timeToLive;

                    return timeToLive == TimeSpan.Zero
                        ? default
                        : _cache.SetMany(outerKey, valuesFromFuncMemory, timeToLive);
                }

                var filteredValues = CacheValuesFilter<TParams, TInnerKey, TValue>.Filter(
                    parameters,
                    valuesFromFuncMemory,
                    _skipCacheSetInnerPredicate,
                    out var pooledArray);

                try
                {
                    if (filteredValues.Length > 0)
                    {
                        TimeSpan timeToLive;
                        if (_timeToLiveFactory is null)
                        {
                            timeToLive = _timeToLive;
                        }
                        else
                        {
                            var pooledKeysArray = ArrayPool<TInnerKey>.Shared.Rent(filteredValues.Length);
                            var span = filteredValues.Span;
                            for (var i = 0; i < span.Length; i++)
                                pooledKeysArray[i] = span[i].Key;

                            try
                            {
                                timeToLive = _timeToLiveFactory(
                                    parameters,
                                    new ReadOnlyMemory<TInnerKey>(pooledKeysArray, 0, span.Length));
                            }
                            finally
                            {
                                ArrayPool<TInnerKey>.Shared.Return(pooledKeysArray);
                            }
                        }

                        return timeToLive == TimeSpan.Zero
                            ? default
                            : _cache.SetMany(outerKey, filteredValues, timeToLive);
                    }
                }
                finally
                {
                    if (!(pooledArray is null))
                        ArrayPool<KeyValuePair<TInnerKey, TValue>>.Shared.Return(pooledArray);
                }

                return default;
            }
        }

        private Dictionary<TInnerKey, TValue> FillMissingKeys(
            TOuterKey outerKey,
            ReadOnlySpan<TInnerKey> innerKeys,
            IEnumerable<KeyValuePair<TInnerKey, TValue>> valuesFromFunc)
        {
            if (!(valuesFromFunc is Dictionary<TInnerKey, TValue> valuesDictionary && valuesDictionary.Comparer.Equals(_keyComparer)))
            {
                valuesDictionary = new Dictionary<TInnerKey, TValue>(innerKeys.Length, _keyComparer);

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
