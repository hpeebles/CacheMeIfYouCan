using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using CacheMeIfYouCan.Events.CachedFunction.EnumerableKeys;
using CacheMeIfYouCan.Internal.CachedFunctions.Configuration;

namespace CacheMeIfYouCan.Internal.CachedFunctions
{
    internal sealed class CachedFunctionWithEnumerableKeys<TParams, TKey, TValue>
    {
        private readonly Func<TParams, ReadOnlyMemory<TKey>, CancellationToken, ValueTask<IEnumerable<KeyValuePair<TKey, TValue>>>> _originalFunction;
        private readonly TimeSpan _timeToLive;
        private readonly Func<TParams, ReadOnlyMemory<TKey>, TimeSpan> _timeToLiveFactory;
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
        private readonly Func<TKey, TValue, bool> _filterResponsePredicate;
        private readonly ICache<TKey, TValue> _cache;
        private readonly Action<SuccessfulRequestEvent<TParams, TKey, TValue>> _onSuccessAction;
        private readonly Action<ExceptionEvent<TParams, TKey>> _onExceptionAction;
        private readonly bool _cacheEnabled;
        private readonly bool _measurementsEnabled;

        public CachedFunctionWithEnumerableKeys(
            Func<TParams, ReadOnlyMemory<TKey>, CancellationToken, ValueTask<IEnumerable<KeyValuePair<TKey, TValue>>>> originalFunction,
            CachedFunctionWithEnumerableKeysConfiguration<TParams, TKey, TValue> config)
        {
            _originalFunction = originalFunction;
            _maxBatchSize = config.MaxBatchSize;
            _batchBehaviour = config.BatchBehaviour;
            _onSuccessAction = config.OnSuccessAction;
            _onExceptionAction = config.OnExceptionAction;
            _filterResponsePredicate = config.FilterResponsePredicate;

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
            _measurementsEnabled = _onSuccessAction != null || _onExceptionAction != null;
        }

        public async ValueTask<Dictionary<TKey, TValue>> GetMany(
            TParams parameters,
            IEnumerable<TKey> keys,
            CancellationToken cancellationToken)
        {
            var (start, timer) = GetDateAndTimer();
            var keysMemory = keys.ToReadOnlyMemory(out var pooledKeysArray);
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                var getFromCacheTask = GetFromCache(parameters, keysMemory);

                var (resultsDictionary, cacheStats) = getFromCacheTask.IsCompleted
                    ? getFromCacheTask.Result
                    : await getFromCacheTask.ConfigureAwait(false);

                if (cacheStats.CacheHits == keysMemory.Length)
                    return FinalizeResults(resultsDictionary, cacheStats);

                var missingKeys = MissingKeysResolver<TKey, TValue>.GetMissingKeys(
                    keysMemory,
                    resultsDictionary,
                    out var pooledMissingKeysArray);

                try
                {
                    if (missingKeys.Length < _maxBatchSize)
                    {
                        var resultsDictionaryTask = GetValuesFromFunc(
                            parameters,
                            missingKeys,
                            cancellationToken,
                            resultsDictionary);

                        resultsDictionary = resultsDictionaryTask.IsCompleted
                            ? resultsDictionaryTask.Result
                            : await resultsDictionaryTask.ConfigureAwait(false);
                    }
                    else
                    {
                        var batchSizes = BatchingHelper.GetBatchSizes(missingKeys.Length, _maxBatchSize, _batchBehaviour);

                        resultsDictionary ??= new Dictionary<TKey, TValue>(_keyComparer);
                        
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
                                keysMemory.Slice(keysStartIndex, batchSize),
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
                        ArrayPool<TKey>.Shared.Return(pooledMissingKeysArray);
                }
                
                return FinalizeResults(resultsDictionary, cacheStats);
            }
            catch (Exception ex) when (!(_onExceptionAction is null))
            {
                _onExceptionAction?.Invoke(new ExceptionEvent<TParams, TKey>(
                    parameters,
                    keysMemory,
                    start,
                    timer.Elapsed,
                    ex));

                throw;
            }
            finally
            {
                if (!(pooledKeysArray is null))
                    ArrayPool<TKey>.Shared.Return(pooledKeysArray);
            }
            
            Dictionary<TKey, TValue> FinalizeResults(
                Dictionary<TKey, TValue> resultsDictionary,
                CacheGetManyStats cacheStats)
            {
                resultsDictionary = FilterResults(resultsDictionary, out var countExcluded);
                    
                _onSuccessAction?.Invoke(new SuccessfulRequestEvent<TParams, TKey, TValue>(
                    parameters,
                    keysMemory,
                    resultsDictionary,
                    start,
                    timer.Elapsed,
                    cacheStats,
                    countExcluded));

                return resultsDictionary;
            }
        }

        private async ValueTask<(Dictionary<TKey, TValue> Results, CacheGetManyStats CacheStats)> GetFromCache(
            TParams parameters, ReadOnlyMemory<TKey> keys)
        {
            if (!_cacheEnabled)
                return default;
            
            if (_skipCacheGetOuterPredicate?.Invoke(parameters) == true)
                return GetResultIfAllKeysSkipped();

            if (_skipCacheGetInnerPredicate is null)
            {
                var task = GetFromCacheInner(keys, 0);

                return task.IsCompleted
                    ? task.Result
                    : await task.ConfigureAwait(false);
            }
            
            var filteredKeys = CacheKeysFilter<TParams, TKey>.Filter(
                parameters,
                keys,
                _skipCacheGetInnerPredicate,
                out var pooledFilteredKeysArray);

            try
            {
                if (filteredKeys.Length == 0)
                    return GetResultIfAllKeysSkipped();

                var task = GetFromCacheInner(filteredKeys, keys.Length - filteredKeys.Length);
                
                return task.IsCompleted
                    ? task.Result
                    : await task.ConfigureAwait(false);
            }
            finally
            {
                if (!(pooledFilteredKeysArray is null))
                    ArrayPool<TKey>.Shared.Return(pooledFilteredKeysArray);
            }

            (Dictionary<TKey, TValue>, CacheGetManyStats) GetResultIfAllKeysSkipped()
            {
                var cacheStats = new CacheGetManyStats(
                    cacheKeysRequested: 0,
                    cacheKeysSkipped: keys.Length,
                    localCacheEnabled: _cache.LocalCacheEnabled,
                    distributedCacheEnabled: _cache.DistributedCacheEnabled);
                
                return (null, cacheStats);
            }
            
            async ValueTask<(Dictionary<TKey, TValue> Results, CacheGetManyStats CacheStats)> GetFromCacheInner(
                ReadOnlyMemory<TKey> keys, int cacheKeysSkipped)
            {
                using var getFromCacheResultsMemoryOwner = MemoryPool<KeyValuePair<TKey, TValue>>.Shared.Rent(keys.Length);
                var getFromCacheResultsMemory = getFromCacheResultsMemoryOwner.Memory;

                var task = _cache.GetMany(keys, cacheKeysSkipped, getFromCacheResultsMemory);

                var cacheStats = task.IsCompleted
                    ? task.Result
                    : await task.ConfigureAwait(false);

                var countFoundInCache = cacheStats.CacheHits;
                if (countFoundInCache == 0)
                    return (null, cacheStats);

                var resultsFromCache = new Dictionary<TKey, TValue>(countFoundInCache, _keyComparer);
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
        private async ValueTask<Dictionary<TKey, TValue>> GetValuesFromFunc(
            TParams parameters,
            ReadOnlyMemory<TKey> keys,
            CancellationToken cancellationToken,
            Dictionary<TKey, TValue> resultsDictionary,
            object resultsDictionaryLock = null)
        {
            var getValuesFromFuncTask = _originalFunction(parameters, keys, cancellationToken);

            var valuesFromFunc = getValuesFromFuncTask.IsCompleted
                ? getValuesFromFuncTask.Result
                : await getValuesFromFuncTask.ConfigureAwait(false);

            Dictionary<TKey, TValue> valuesFromFuncDictionary;
            if (valuesFromFunc is Dictionary<TKey, TValue> d && d.Comparer.Equals(_keyComparer))
                valuesFromFuncDictionary = d;
            else
                valuesFromFuncDictionary = null;

            if (_shouldFillMissingKeys)
            {
                valuesFromFuncDictionary = valuesFromFuncDictionary is null
                    ? ConvertToDictionaryAndFillMissingKeys(keys.Span, valuesFromFunc)
                    : FillMissingKeys(keys.Span, valuesFromFuncDictionary);
            }

            var valuesFromFuncMemory = valuesFromFuncDictionary is null
                ? valuesFromFunc.ToReadOnlyMemory(out var pooledArray)
                : valuesFromFuncDictionary.ToReadOnlyMemory(out pooledArray);

            if (valuesFromFuncMemory.Length == 0)
                return resultsDictionary;

            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                var setInCacheTask = SetInCache(parameters, keys, valuesFromFuncMemory);

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
                        resultsDictionary ??= new Dictionary<TKey, TValue>(valuesFromFuncMemory.Length, _keyComparer);
                    
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

                if (!setInCacheTask.IsCompleted)
                    await setInCacheTask.ConfigureAwait(false);
            }
            finally
            {
                if (!(pooledArray is null))
                    ArrayPool<KeyValuePair<TKey, TValue>>.Shared.Return(pooledArray);
            }

            return resultsDictionary;
        }

        private ValueTask SetInCache(
            TParams parameters,
            ReadOnlyMemory<TKey> keys,
            ReadOnlyMemory<KeyValuePair<TKey, TValue>> valuesFromFuncMemory)
        {
            if (!_cacheEnabled || _skipCacheSetOuterPredicate?.Invoke(parameters) == true)
                return default;

            if (_skipCacheSetInnerPredicate is null)
            {
                var timeToLive = _timeToLiveFactory?.Invoke(parameters, keys) ?? _timeToLive;

                return timeToLive == TimeSpan.Zero
                    ? default
                    : _cache.SetMany(valuesFromFuncMemory, timeToLive);
            }

            var filteredValues = CacheValuesFilter<TParams, TKey, TValue>.Filter(
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
                        var pooledKeysArray = ArrayPool<TKey>.Shared.Rent(filteredValues.Length);
                        var span = filteredValues.Span;
                        for (var i = 0; i < span.Length; i++)
                            pooledKeysArray[i] = span[i].Key;

                        try
                        {
                            timeToLive = _timeToLiveFactory(
                                parameters,
                                new ReadOnlyMemory<TKey>(pooledKeysArray, 0, span.Length));
                        }
                        finally
                        {
                            ArrayPool<TKey>.Shared.Return(pooledKeysArray);
                        }
                    }

                    return timeToLive == TimeSpan.Zero
                        ? default
                        : _cache.SetMany(filteredValues, timeToLive);
                }
            }
            finally
            {
                if (!(pooledFilteredValuesArray is null))
                    ArrayPool<KeyValuePair<TKey, TValue>>.Shared.Return(pooledFilteredValuesArray);
            }

            return default;
        }

        private Dictionary<TKey, TValue> ConvertToDictionaryAndFillMissingKeys(
            ReadOnlySpan<TKey> keys,
            IEnumerable<KeyValuePair<TKey, TValue>> valuesFromFunc)
        {
            var valuesDictionary = new Dictionary<TKey, TValue>(keys.Length, _keyComparer);

            if (!(valuesFromFunc is null))
            {
                foreach (var kv in valuesFromFunc)
                    valuesDictionary[kv.Key] = kv.Value;
            }

            return FillMissingKeys(keys, valuesDictionary);
        }
        
        private Dictionary<TKey, TValue> FillMissingKeys(
            ReadOnlySpan<TKey> keys,
            Dictionary<TKey, TValue> valuesDictionary)
        {
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Dictionary<TKey, TValue> FilterResults(Dictionary<TKey, TValue> results, out int countExcluded)
        {
            if (_filterResponsePredicate is null)
            {
                countExcluded = 0;
                return results;
            }
            
            return FilterResultsInner(out countExcluded);

            Dictionary<TKey, TValue> FilterResultsInner(out int countExcluded)
            {
                countExcluded = 0;
                var filter = _filterResponsePredicate;
                foreach (var kv in results)
                {
                    if (!filter(kv.Key, kv.Value))
                    {
                        countExcluded++;
                        results.Remove(kv.Key);
                    }
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