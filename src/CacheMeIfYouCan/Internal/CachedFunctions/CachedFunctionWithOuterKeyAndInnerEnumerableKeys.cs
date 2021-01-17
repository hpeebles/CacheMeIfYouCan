using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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
        private readonly Func<TParams, ReadOnlyMemory<TInnerKey>, int> _maxBatchSizeFactory;
        private readonly BatchBehaviour _batchBehaviour;
        private readonly bool _shouldFillMissingKeys;
        private readonly bool _shouldFillMissingKeysWithConstantValue;
        private readonly TValue _fillMissingKeysConstantValue;
        private readonly Func<TOuterKey, TInnerKey, TValue> _fillMissingKeysValueFactory;
        private readonly Func<TInnerKey, TValue, bool> _filterResponsePredicate;
        private readonly ICache<TOuterKey, TInnerKey, TValue> _cache;
        private readonly Action<SuccessfulRequestEvent<TParams, TOuterKey, TInnerKey, TValue>> _onSuccessAction;
        private readonly Action<ExceptionEvent<TParams, TOuterKey, TInnerKey>> _onExceptionAction;
        private readonly bool _cacheEnabled;
        private readonly bool _measurementsEnabled;

        public CachedFunctionWithOuterKeyAndInnerEnumerableKeys(
            Func<TParams, ReadOnlyMemory<TInnerKey>, CancellationToken, ValueTask<IEnumerable<KeyValuePair<TInnerKey, TValue>>>> originalFunction,
            Func<TParams, TOuterKey> keySelector,
            CachedFunctionWithOuterKeyAndInnerEnumerableKeysConfiguration<TParams, TOuterKey, TInnerKey, TValue> config)
        {
            _originalFunction = originalFunction;
            _keySelector = keySelector;
            _maxBatchSize = config.MaxBatchSize;
            _maxBatchSizeFactory = config.MaxBatchSizeFactory;
            _batchBehaviour = config.BatchBehaviour;
            _onSuccessAction = config.OnSuccessAction;
            _onExceptionAction = config.OnExceptionAction;
            _filterResponsePredicate = config.FilterResponsePredicate;

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
            _measurementsEnabled = _onSuccessAction != null || _onExceptionAction != null;
        }

        public async ValueTask<Dictionary<TInnerKey, TValue>> GetMany(
            TParams parameters,
            IEnumerable<TInnerKey> innerKeys,
            CancellationToken cancellationToken)
        {
            var (start, timer) = GetDateAndTimer();
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
                    return FinalizeResults(resultsDictionary, cacheStats);

                var missingKeys = MissingKeysResolver<TInnerKey, TValue>.GetMissingKeys(
                    innerKeysMemory,
                    resultsDictionary,
                    out var pooledMissingKeysArray);

                try
                {
                    var maxBatchSize = _maxBatchSizeFactory?.Invoke(parameters, innerKeysMemory) ?? _maxBatchSize;
                    if (missingKeys.Length < maxBatchSize)
                    {
                        var resultsDictionaryTask = GetValuesFromFunc(
                            parameters,
                            outerKey,
                            missingKeys,
                            cancellationToken,
                            resultsDictionary);

                        resultsDictionary = resultsDictionaryTask.IsCompleted
                            ? resultsDictionaryTask.Result
                            : await resultsDictionaryTask.ConfigureAwait(false);
                    }
                    else
                    {
                        var batchSizes = BatchingHelper.GetBatchSizes(missingKeys.Length, maxBatchSize, _batchBehaviour);

                        resultsDictionary ??= new Dictionary<TInnerKey, TValue>(_keyComparer);
                        
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

                return FinalizeResults(resultsDictionary, cacheStats);
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

            Dictionary<TInnerKey, TValue> FinalizeResults(
                Dictionary<TInnerKey, TValue> resultsDictionary,
                CacheGetManyStats cacheStats)
            {
                resultsDictionary = FilterResults(resultsDictionary, out var countExcluded);
                    
                _onSuccessAction?.Invoke(new SuccessfulRequestEvent<TParams, TOuterKey, TInnerKey, TValue>(
                    parameters,
                    outerKey,
                    innerKeysMemory,
                    resultsDictionary,
                    start,
                    timer.Elapsed,
                    cacheStats,
                    countExcluded));

                return resultsDictionary;
            }
        }

        private async ValueTask<(Dictionary<TInnerKey, TValue> Results, CacheGetManyStats CacheStats)> GetFromCache(
            TParams parameters, TOuterKey outerKey, ReadOnlyMemory<TInnerKey> innerKeys)
        {
            if (!_cacheEnabled)
                return default;
            
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
                out var pooledFilteredKeysArray);

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
                if (!(pooledFilteredKeysArray is null))
                    ArrayPool<TInnerKey>.Shared.Return(pooledFilteredKeysArray);
            }

            (Dictionary<TInnerKey, TValue>, CacheGetManyStats) GetResultIfAllKeysSkipped()
            {
                var cacheStats = new CacheGetManyStats(
                    cacheKeysRequested: 0,
                    cacheKeysSkipped: innerKeys.Length,
                    localCacheEnabled: _cache.LocalCacheEnabled,
                    distributedCacheEnabled: _cache.DistributedCacheEnabled);
                
                return (null, cacheStats);
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
                    return (null, cacheStats);

                var resultsFromCache = new Dictionary<TInnerKey, TValue>(countFoundInCache, _keyComparer);
                for (var i = 0; i < countFoundInCache; i++)
                {
                    var kv = getFromCacheResultsMemory.Span[i];
                    resultsFromCache[kv.Key] = kv.Value;
                }

                return (resultsFromCache, cacheStats);
            }
        }

        // This is called for both batched and non-batched requests.
        //
        // When batched, the results of calling _originalFunction for each batch are added to resultsDictionary,
        // resultsDictionaryLock is used to synchronize these updates. The return value is ignored.
        //
        // When not batched, the return value is a dictionary containing the values from resultsDictionary and the
        // values returned by _originalFunction. If _originalFunction itself returns a dictionary with the same comparer
        // as resultsDictionary, then whichever dictionary is largest is chosen as the return value and the values from
        // the smaller dictionary are added to it. If not, the results of calling _originalFunction are added to
        // resultsDictionary which is then returned.
        private async ValueTask<Dictionary<TInnerKey, TValue>> GetValuesFromFunc(
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

            Dictionary<TInnerKey, TValue> valuesFromFuncDictionary;
            if (valuesFromFunc is Dictionary<TInnerKey, TValue> d && d.Comparer.Equals(_keyComparer))
                valuesFromFuncDictionary = d;
            else
                valuesFromFuncDictionary = null;

            if (_shouldFillMissingKeys)
            {
                valuesFromFuncDictionary = valuesFromFuncDictionary is null
                    ? ConvertToDictionaryAndFillMissingKeys(outerKey, innerKeys.Span, valuesFromFunc)
                    : FillMissingKeys(outerKey, innerKeys.Span, valuesFromFuncDictionary);
            }

            var valuesFromFuncMemory = valuesFromFuncDictionary is null
                ? valuesFromFunc.ToReadOnlyMemory(out var pooledArray)
                : valuesFromFuncDictionary.ToReadOnlyMemory(out pooledArray);

            if (valuesFromFuncMemory.Length == 0)
                return resultsDictionary;

            ValueTask setInCacheTask = default;
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                setInCacheTask = SetInCache(parameters, outerKey, innerKeys, valuesFromFuncMemory);

                if (resultsDictionaryLock is null)
                {
                    if (!(valuesFromFuncDictionary is null) &&
                        valuesFromFuncDictionary.Count > (resultsDictionary?.Count ?? 0))
                    {
                        if (!(resultsDictionary is null))
                        {
                            foreach (var kv in resultsDictionary)
                                valuesFromFuncDictionary[kv.Key] = kv.Value;
                        }
                    
                        resultsDictionary = valuesFromFuncDictionary;
                    }
                    else
                    {
                        resultsDictionary ??= new Dictionary<TInnerKey, TValue>(valuesFromFuncMemory.Length, _keyComparer);
                    
                        Utilities.AddValuesToDictionary(valuesFromFuncMemory.Span, resultsDictionary);
                    }
                }
                else
                {
                    var lockTaken = false;
                    Monitor.TryEnter(resultsDictionaryLock, ref lockTaken);

                    Utilities.AddValuesToDictionary(valuesFromFuncMemory.Span, resultsDictionary);

                    if (lockTaken)
                        Monitor.Exit(resultsDictionaryLock);
                }
            }
            finally
            {
                if (!(pooledArray is null))
                    ArrayPool<KeyValuePair<TInnerKey, TValue>>.Shared.Return(pooledArray);
                
                if (!setInCacheTask.IsCompleted)
                    await setInCacheTask.ConfigureAwait(false);
            }

            return resultsDictionary;
        }

        private ValueTask SetInCache(
            TParams parameters,
            TOuterKey outerKey,
            ReadOnlyMemory<TInnerKey> keys,
            ReadOnlyMemory<KeyValuePair<TInnerKey, TValue>> valuesFromFuncMemory)
        {
            if (!_cacheEnabled || _skipCacheSetOuterPredicate?.Invoke(parameters) == true)
                return default;

            if (_skipCacheSetInnerPredicate is null)
            {
                var timeToLive = _timeToLiveFactory?.Invoke(parameters, keys) ?? _timeToLive;

                return timeToLive == TimeSpan.Zero
                    ? default
                    : _cache.SetMany(outerKey, valuesFromFuncMemory, timeToLive);
            }

            var filteredValues = CacheValuesFilter<TParams, TInnerKey, TValue>.Filter(
                parameters,
                valuesFromFuncMemory,
                _skipCacheSetInnerPredicate,
                out var pooledFilteredValuesArray);

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
                if (!(pooledFilteredValuesArray is null))
                    ArrayPool<KeyValuePair<TInnerKey, TValue>>.Shared.Return(pooledFilteredValuesArray);
            }

            return default;
        }

        private Dictionary<TInnerKey, TValue> ConvertToDictionaryAndFillMissingKeys(
            TOuterKey outerKey,
            ReadOnlySpan<TInnerKey> innerKeys,
            IEnumerable<KeyValuePair<TInnerKey, TValue>> valuesFromFunc)
        {
            var valuesDictionary = new Dictionary<TInnerKey, TValue>(innerKeys.Length, _keyComparer);

            if (!(valuesFromFunc is null))
            {
                foreach (var kv in valuesFromFunc)
                    valuesDictionary[kv.Key] = kv.Value;
            }

            return FillMissingKeys(outerKey, innerKeys, valuesDictionary);
        }
        
        private Dictionary<TInnerKey, TValue> FillMissingKeys(
            TOuterKey outerKey,
            ReadOnlySpan<TInnerKey> innerKeys,
            Dictionary<TInnerKey, TValue> valuesDictionary)
        {
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
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Dictionary<TInnerKey, TValue> FilterResults(Dictionary<TInnerKey, TValue> results, out int countExcluded)
        {
            if (_filterResponsePredicate is null)
            {
                countExcluded = 0;
                return results;
            }
            
            return FilterResultsInner(out countExcluded);

            Dictionary<TInnerKey, TValue> FilterResultsInner(out int countExcluded)
            {
                countExcluded = 0;
                List<TInnerKey> toRemove = null;
                var filter = _filterResponsePredicate;
                foreach (var kv in results)
                {
                    if (!filter(kv.Key, kv.Value))
                    {
                        countExcluded++;
                        
                        toRemove ??= new List<TInnerKey>();
                        toRemove.Add(kv.Key);
                    }
                }

                if (toRemove != null)
                {
                    foreach (var key in toRemove)
                        results.Remove(key);
                }

                return results;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private (DateTime Start, StopwatchStruct Timer) GetDateAndTimer()
        {
            return _measurementsEnabled
                ? (DateTime.UtcNow, StopwatchStruct.StartNew())
                : (default, default);
        }
    }
}
