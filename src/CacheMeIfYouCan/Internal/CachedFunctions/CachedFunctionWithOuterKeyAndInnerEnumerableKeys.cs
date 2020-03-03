using System;
using System.Buffers;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CacheMeIfYouCan.Internal.CachedFunctions.Configuration;

namespace CacheMeIfYouCan.Internal.CachedFunctions
{
    internal sealed class CachedFunctionWithOuterKeyAndInnerEnumerableKeys<TParams, TOuterKey, TInnerKey, TValue>
    {
        private readonly Func<TParams, IReadOnlyCollection<TInnerKey>, CancellationToken, ValueTask<IEnumerable<KeyValuePair<TInnerKey, TValue>>>> _originalFunction;
        private readonly Func<TParams, TOuterKey> _keySelector;
        private readonly Func<TOuterKey, IReadOnlyCollection<TInnerKey>, TimeSpan> _timeToLiveFactory;
        private readonly IEqualityComparer<TInnerKey> _keyComparer;
        private readonly Func<TOuterKey, bool> _skipCacheGetPredicateOuterKeyOnly;
        private readonly Func<TOuterKey, TInnerKey, bool> _skipCacheGetPredicate;
        private readonly Func<TOuterKey, bool> _skipCacheSetPredicateOuterKeyOnly;
        private readonly Func<TOuterKey, TInnerKey, TValue, bool> _skipCacheSetPredicate;
        private readonly int _maxBatchSize;
        private readonly BatchBehaviour _batchBehaviour;
        private readonly bool _shouldFillMissingKeys;
        private readonly bool _shouldFillMissingKeysWithConstantValue;
        private readonly TValue _fillMissingKeysConstantValue;
        private readonly Func<TOuterKey, TInnerKey, TValue> _fillMissingKeysValueFactory;
        private readonly ICache<TOuterKey, TInnerKey, TValue> _cache;

        public CachedFunctionWithOuterKeyAndInnerEnumerableKeys(
            Func<TParams, IReadOnlyCollection<TInnerKey>, CancellationToken, ValueTask<IEnumerable<KeyValuePair<TInnerKey, TValue>>>> originalFunction,
            Func<TParams, TOuterKey> keySelector,
            CachedFunctionWithOuterKeyAndInnerEnumerableKeysConfiguration<TOuterKey, TInnerKey, TValue> config)
        {
            _originalFunction = originalFunction;
            _keySelector = keySelector;

            if (config.DisableCaching)
            {
                _timeToLiveFactory = (_, __) => TimeSpan.Zero;
                _cache = NullCache<TOuterKey, TInnerKey, TValue>.Instance;
            }
            else
            {
                _timeToLiveFactory = config.TimeToLiveFactory;
                _keyComparer = config.KeyComparer ?? EqualityComparer<TInnerKey>.Default;
                
                _cache = CacheBuilder.Build(
                    config,
                    out var additionalSkipCacheGetPredicateOuterKeyOnly,
                    out var additionalSkipCacheGetPredicate,
                    out var additionalSkipCacheSetPredicateOuterKeyOnly,
                    out var additionalSkipCacheSetPredicate);
                
                _skipCacheGetPredicateOuterKeyOnly = config.SkipCacheGetPredicateOuterKeyOnly.Or(additionalSkipCacheGetPredicateOuterKeyOnly);
                _skipCacheGetPredicate = config.SkipCacheGetPredicate.Or(additionalSkipCacheGetPredicate);
                _skipCacheSetPredicateOuterKeyOnly = config.SkipCacheSetPredicateOuterKeyOnly.Or(additionalSkipCacheSetPredicateOuterKeyOnly);
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

        public async ValueTask<Dictionary<TInnerKey, TValue>> GetMany(
            TParams parameters,
            IEnumerable<TInnerKey> innerKeys,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var outerKey = _keySelector(parameters);
            var innerKeysCollection = innerKeys.ToReadOnlyCollection();

            var getFromCacheTask = GetFromCache();

            var resultsDictionary = getFromCacheTask.IsCompleted
                ? getFromCacheTask.Result
                : await getFromCacheTask.ConfigureAwait(false);

            if (resultsDictionary.Count == innerKeysCollection.Count)
                return resultsDictionary;

            var missingKeys = MissingKeysResolver<TInnerKey, TValue>.GetMissingKeys(innerKeysCollection, resultsDictionary);

            if (missingKeys.Count < _maxBatchSize)
            {
                var getValuesFromFuncTask = GetValuesFromFunc(parameters, outerKey, missingKeys, cancellationToken, resultsDictionary);

                if (!getValuesFromFuncTask.IsCompleted)
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

                    if (getValuesFromFuncTask.IsCompleted)
                        continue;
                    
                    if (tasks is null)
                        tasks = new Task[batches.Length - batchIndex];

                    tasks[tasksIndex++] = getValuesFromFuncTask.AsTask();
                }

                if (!(tasks is null))
                    await Task.WhenAll(new ArraySegment<Task>(tasks, 0, tasksIndex)).ConfigureAwait(false);
            }

            return resultsDictionary;
            
            async ValueTask<Dictionary<TInnerKey, TValue>> GetFromCache()
            {
                if (!(_skipCacheGetPredicateOuterKeyOnly is null) && _skipCacheGetPredicateOuterKeyOnly(outerKey))
                    return new Dictionary<TInnerKey, TValue>(_keyComparer);
                
                var getFromCacheResultsArray = ArrayPool<KeyValuePair<TInnerKey, TValue>>.Shared.Rent(innerKeysCollection.Count);
                Dictionary<TInnerKey, TValue> resultsFromCache;

                try
                {
                    int countFoundInCache;
                    if (_skipCacheGetPredicate is null)
                    {
                        var task = _cache.GetMany(outerKey, innerKeysCollection, getFromCacheResultsArray);

                        countFoundInCache = task.IsCompleted
                            ? task.Result
                            : await task.ConfigureAwait(false);
                    }
                    else
                    {
                        var filteredKeys = CacheKeysFilter<TOuterKey, TInnerKey>.Filter(
                            outerKey,
                            innerKeysCollection,
                            _skipCacheGetPredicate,
                            out var pooledArray);

                        try
                        {
                            var task = filteredKeys.Count > 0
                                ? _cache.GetMany(outerKey, filteredKeys, getFromCacheResultsArray)
                                : new ValueTask<int>(0);

                            countFoundInCache = task.IsCompleted
                                ? task.Result
                                : await task.ConfigureAwait(false);
                        }
                        finally
                        {
                            CacheKeysFilter<TInnerKey>.ReturnPooledArray(pooledArray);
                        }
                    }

                    if (countFoundInCache == 0)
                    {
                        resultsFromCache = new Dictionary<TInnerKey, TValue>(_keyComparer);
                    }
                    else
                    {
                        resultsFromCache = new Dictionary<TInnerKey, TValue>(countFoundInCache, _keyComparer);
                        for (var i = 0; i < countFoundInCache; i++)
                        {
                            var kv = getFromCacheResultsArray[i];
                            resultsFromCache[kv.Key] = kv.Value;
                        }
                    }
                }
                finally
                {
                    ArrayPool<KeyValuePair<TInnerKey, TValue>>.Shared.Return(getFromCacheResultsArray);
                }

                return resultsFromCache;
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
                if (!(_skipCacheSetPredicateOuterKeyOnly is null) && _skipCacheSetPredicateOuterKeyOnly(outerKey))
                    return default;

                if (_skipCacheSetPredicate is null)
                {
                    var timeToLive = _timeToLiveFactory(outerKey, innerKeys);

                    return timeToLive == TimeSpan.Zero
                        ? default
                        : _cache.SetMany(outerKey, valuesFromFuncCollection, timeToLive);
                }

                var filteredValues = CacheValuesFilter<TOuterKey, TInnerKey, TValue>.Filter(
                    outerKey,
                    valuesFromFuncCollection,
                    _skipCacheSetPredicate,
                    out var pooledArray);

                try
                {
                    if (filteredValues.Count > 0)
                    {
                        var filteredKeys = filteredValues.Count == innerKeys.Count
                            ? innerKeys
                            : new ReadOnlyCollectionKeyValuePairKeys<TInnerKey, TValue>(filteredValues);
                        
                        var timeToLive = _timeToLiveFactory(outerKey, filteredKeys);

                        return timeToLive == TimeSpan.Zero
                            ? default
                            : _cache.SetMany(outerKey, filteredValues, timeToLive);
                    }
                }
                finally
                {
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