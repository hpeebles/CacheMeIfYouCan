﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CacheMeIfYouCan.Internal.DuplicateTaskCatcher;
using CacheMeIfYouCan.Notifications;

namespace CacheMeIfYouCan.Internal.FunctionCaches
{
    internal sealed class MultiParamEnumerableKeyFunctionCache<TK1, TK2, TV> : IPendingRequestsCounter, IDisposable
    {
        private readonly ICacheInternal<(TK1, TK2), TV> _cache;
        private readonly Func<TK1, TimeSpan> _timeToLiveFactory;
        private readonly Func<TK1, string> _outerKeySerializer;
        private readonly Func<TK2, string> _innerKeySerializer;
        private readonly Func<(TK1, TK2), TV> _defaultValueFactory;
        private readonly bool _continueOnException;
        private readonly Action<FunctionCacheGetResult<(TK1, TK2), TV>> _onResult;
        private readonly Action<FunctionCacheFetchResult<(TK1, TK2), TV>> _onFetch;
        private readonly Action<FunctionCacheException<(TK1, TK2)>> _onException;
        private readonly KeyComparer<TK2> _innerKeyComparer;
        private readonly IEqualityComparer<Key<(TK1, TK2)>> _tupleKeysOnlyDifferingBySecondItemComparer;
        private readonly string _keyParamSeparator;
        private readonly Func<TK1, int> _maxFetchBatchSizeFunc;
        private readonly BatchBehaviour _batchBehaviour;
        private readonly IDuplicateTaskCatcherCombinedMulti<TK1, TK2, TV> _fetchHandler;
        private readonly Func<TK1, Key<(TK1, TK2)>[], Key<(TK1, TK2)>[]> _keysToGetFromCacheFunc;
        private readonly Func<(TK1, TK2), bool> _skipCacheSetPredicate;
        private readonly Func<TK1, bool> _skipCacheSetPredicateOuterKeyOnly;
        private readonly Func<int, List<KeyValuePair<Key<(TK1, TK2)>, TV>>> _valuesToSetInCacheListFactory;
        private readonly Func<DuplicateTaskCatcherMultiResult<TK2, TV>, bool> _setInCachePredicate;
        private readonly Func<DuplicateTaskCatcherMultiResult<TK2, TV>, bool> _alwaysFalseSetInCachePredicate = k => false;
        private readonly Func<DuplicateTaskCatcherMultiResult<TK2, TV>, bool> _skipDuplicatesSetInCachePredicate = k => !k.Duplicate;
        private readonly Key<(TK1, TK2)>[] _emptyKeyArray = new Key<(TK1, TK2)>[0];
        private int _pendingRequestsCount;
        private bool _disposed;
        
        public MultiParamEnumerableKeyFunctionCache(
            Func<TK1, IEnumerable<TK2>, CancellationToken, Task<IDictionary<TK2, TV>>> func,
            string functionName,
            ICacheInternal<(TK1, TK2), TV> cache,
            Func<TK1, TimeSpan> timeToLiveFactory,
            bool catchDuplicateRequests,
            Func<TK1, string> outerKeySerializer,
            Func<TK2, string> innerKeySerializer,
            Func<(TK1, TK2), TV> defaultValueFactory,
            Action<FunctionCacheGetResult<(TK1, TK2), TV>> onResult,
            Action<FunctionCacheFetchResult<(TK1, TK2), TV>> onFetch,
            Action<FunctionCacheException<(TK1, TK2)>> onException,
            KeyComparer<TK1> outerKeyComparer,
            KeyComparer<TK2> innerKeyComparer,
            string keyParamSeparator,
            Func<TK1, int> maxFetchBatchSizeFunc,
            BatchBehaviour batchBehaviour,
            Func<(TK1, TK2), bool> skipCacheGetPredicate,
            Func<(TK1, TK2), bool> skipCacheSetPredicate,
            Func<TK1, bool> skipCacheGetPredicateOuterKeyOnly,
            Func<TK1, bool> skipCacheSetPredicateOuterKeyOnly,
            Func<(TK1, TK2), TV> negativeCachingValueFactory)
        {
            Name = functionName;
            Type = GetType().Name;
            _cache = cache;
            _timeToLiveFactory = timeToLiveFactory;
            _outerKeySerializer = outerKeySerializer;
            _innerKeySerializer = innerKeySerializer;
            _defaultValueFactory = defaultValueFactory;
            _continueOnException = defaultValueFactory != null;
            _onResult = onResult;
            _onFetch = onFetch;
            _onException = onException;
            _innerKeyComparer = innerKeyComparer;
            _tupleKeysOnlyDifferingBySecondItemComparer = new TupleKeysOnlyDifferingBySecondItemComparer(innerKeyComparer);
            _keyParamSeparator = keyParamSeparator;
            _maxFetchBatchSizeFunc = maxFetchBatchSizeFunc;
            _batchBehaviour = batchBehaviour;

            var convertedFunc = negativeCachingValueFactory == null
                ? func
                : ConvertIntoNegativeCachingFunc(func, negativeCachingValueFactory, _innerKeyComparer);

            if (catchDuplicateRequests)
            {
                _fetchHandler = new DuplicateTaskCatcherCombinedMulti<TK1, TK2, TV>(
                    convertedFunc,
                    outerKeyComparer.Inner,
                    innerKeyComparer.Inner);
            }
            else
            {
                _fetchHandler = new DisabledDuplicateTaskCatcherCombinedMulti<TK1, TK2, TV>(
                    convertedFunc,
                    innerKeyComparer.Inner);
            }

            _keysToGetFromCacheFunc = SetupKeysToGetFromCacheFunc(skipCacheGetPredicate, skipCacheGetPredicateOuterKeyOnly);

            _skipCacheSetPredicate = skipCacheSetPredicate;
            _skipCacheSetPredicateOuterKeyOnly = skipCacheSetPredicateOuterKeyOnly;

            // ----------------------------------------
            // The following section is a micro optimization...
            // When possible we reuse the same setInCachePredicate into which we only need to pass a single parameter.
            // When this is not possible we must build a new predicate each time based on the outer key.
            // By doing this ahead of time in the constructor we avoid having to compute the predicate for each request
            // whenever possible.
            // ----------------------------------------
            // If cache is null, setInCachePredicate should always return false so valuesToSetInCache can stay as null
            if (_cache == null)
            {
                _setInCachePredicate = _alwaysFalseSetInCachePredicate;
                _valuesToSetInCacheListFactory = count => null;
            }
            // If skipCacheSetPredicate and skipCacheSetPredicateOuterKeyOnly are both null, setInCachePredicate should
            // only exclude duplicates. Given that in the vast majority of cases duplicates will be rare we should
            // initialize the list with enough capacity to store all of the fetched values.
            else if (skipCacheSetPredicate == null && _skipCacheSetPredicateOuterKeyOnly == null)
            {
                _setInCachePredicate = x => !x.Duplicate;
                _valuesToSetInCacheListFactory = count => new List<KeyValuePair<Key<(TK1, TK2)>, TV>>(count);
            }
            // If at least one of skipCacheSetPredicate and skipCacheSetPredicateOuterKeyOnly are not null, initialize
            // valuesToSetInCache using the default list constructor. Also, we must build a new version of
            // setInCachePredicate each time.
            else
            {
                _valuesToSetInCacheListFactory = count => new List<KeyValuePair<Key<(TK1, TK2)>, TV>>();
            }
        }

        public string Name { get; }
        public string Type { get; }
        public int PendingRequestsCount => _pendingRequestsCount;
        
        public void Dispose()
        {
            _disposed = true;
            PendingRequestsCounterContainer.Remove(this);
        }
        
        public async Task<IReadOnlyCollection<IKeyValuePair<(TK1, TK2), TV>>> GetMulti(
            TK1 outerKey,
            IEnumerable<TK2> innerKeys,
            CancellationToken token)
        {
            if (_disposed)
                throw new ObjectDisposedException($"{Name} - {Type}");
            
            var start = DateTime.UtcNow;
            var stopwatchStart = Stopwatch.GetTimestamp();

            var keys = BuildCombinedKeys(outerKey, innerKeys);

            var results = new Dictionary<Key<(TK1, TK2)>, FunctionCacheGetResultInner<(TK1, TK2), TV>>(
                keys.Length,
                _tupleKeysOnlyDifferingBySecondItemComparer);

            IReadOnlyCollection<FunctionCacheGetResultInner<(TK1, TK2), TV>> readonlyResults = null;
            FunctionCacheGetException<(TK1, TK2)> exception = null;    

            using (SynchronizationContextRemover.StartNew())
            using (TraceHandlerInternal.Start())
            {
                try
                {
                    token.ThrowIfCancellationRequested();
                    
                    Interlocked.Increment(ref _pendingRequestsCount);

                    Key<(TK1, TK2)>[] missingKeys = null;
                    if (_cache != null)
                    {
                        var keysToGet = _keysToGetFromCacheFunc(outerKey, keys);

                        if (keysToGet.Any())
                        {
                            var fromCacheTask = _cache.Get(keysToGet);

                            var fromCache = fromCacheTask.IsCompleted
                                ? fromCacheTask.Result
                                : await fromCacheTask;

                            if (fromCache != null && fromCache.Any())
                            {
                                foreach (var result in fromCache)
                                {
                                    results[result.Key] = new FunctionCacheGetResultInner<(TK1, TK2), TV>(
                                        result.Key,
                                        result.Value,
                                        Outcome.FromCache,
                                        result.CacheType);
                                }

                                missingKeys = keys
                                    .Except(results.Keys, _tupleKeysOnlyDifferingBySecondItemComparer)
                                    .ToArray();
                            }
                        }
                    }

                    if (missingKeys == null)
                        missingKeys = keys;

                    if (missingKeys.Any())
                    {
                        var fetched = await Fetch(outerKey, missingKeys, token);

                        if (fetched != null && fetched.Any())
                        {
                            foreach (var result in fetched)
                            {
                                results[result.Key] = new FunctionCacheGetResultInner<(TK1, TK2), TV>(
                                    result.Key,
                                    result.Value,
                                    Outcome.Fetch,
                                    null);
                            }
                        }
                    }
                    
#if NET45
                    readonlyResults = results.Values.ToArray();
#else
                    readonlyResults = results.Values;
#endif
                }
                catch (Exception ex)
                {
                    var message = _continueOnException
                        ? "Unable to get value(s). Default being returned"
                        : "Unable to get value(s)";

                    exception = new FunctionCacheGetException<(TK1, TK2)>(
                        Name,
                        keys,
                        message,
                        ex);
            
                    _onException?.Invoke(exception);
                    TraceHandlerInternal.Mark(exception);

                    if (!_continueOnException)
                        throw exception;
            
                    readonlyResults = keys
                        .Select(k => new FunctionCacheGetResultInner<(TK1, TK2), TV>(k, _defaultValueFactory(k), Outcome.Error, null))
                        .ToArray();
                }
                finally
                {
                    Interlocked.Decrement(ref _pendingRequestsCount);

                    var notifyResult = _onResult != null || TraceHandlerInternal.Enabled;
                    if (notifyResult)
                    {
                        var functionCacheGetResult = new FunctionCacheGetResult<(TK1, TK2), TV>(
                            Name,
                            readonlyResults,
                            exception,
                            start,
                            StopwatchHelper.GetDuration(stopwatchStart));
                        
                        _onResult?.Invoke(functionCacheGetResult);
                        TraceHandlerInternal.Mark(functionCacheGetResult);
                    }
                }
            }

            return readonlyResults;
        }

        private async Task<IList<FunctionCacheFetchResultInner<(TK1, TK2), TV>>> Fetch(
            TK1 outerKey,
            Key<(TK1, TK2)>[] innerKeys,
            CancellationToken token)
        {
            if (_maxFetchBatchSizeFunc == null)
                return await FetchBatch(innerKeys);
            
            var batchSize = _maxFetchBatchSizeFunc(outerKey);

            if (batchSize <= 0)
                batchSize = Int32.MaxValue;

            if (innerKeys.Length <= batchSize)
                return await FetchBatch(innerKeys);

            if (_batchBehaviour == BatchBehaviour.FillBatchesEvenly)
            {
                var batchesCount = (innerKeys.Length / batchSize) + 1;

                batchSize = (innerKeys.Length / batchesCount) + 1;
            }
            
            var tasks = innerKeys
                .Batch(batchSize)
                .Select(FetchBatch)
                .ToArray();

            await Task.WhenAll(tasks);

            return tasks
                .SelectMany(t => t.Result)
                .ToArray();
            
            async Task<IList<FunctionCacheFetchResultInner<(TK1, TK2), TV>>> FetchBatch(
                IList<Key<(TK1, TK2)>> batchInnerKeys)
            {
                var start = DateTime.UtcNow;
                var stopwatchStart = Stopwatch.GetTimestamp();

                var results = new List<FunctionCacheFetchResultInner<(TK1, TK2), TV>>();
                FunctionCacheFetchException<(TK1, TK2)> exception = null;
                
                try
                {
                    var fetched = await _fetchHandler.ExecuteAsync(
                        outerKey,
                        batchInnerKeys.Select(k => k.AsObject.Item2).ToArray(),
                        token);

                    if (fetched != null && fetched.Any())
                    {
                        var keysMap = batchInnerKeys.ToDictionary(k => k.AsObject.Item2, _innerKeyComparer);

                        var valuesToSetInCache = _valuesToSetInCacheListFactory(fetched.Count);
                        var setInCachePredicate = _setInCachePredicate ?? BuildSetInCachePredicate(outerKey);
                        
                        foreach (var kv in fetched)
                        {
                            var key = keysMap[kv.Key];

                            results.Add(new FunctionCacheFetchResultInner<(TK1, TK2), TV>(
                                key,
                                kv.Value.Value,
                                true,
                                kv.Value.Duplicate,
                                StopwatchHelper.GetDuration(stopwatchStart, kv.Value.StopwatchTimestampCompleted)));

                            if (setInCachePredicate(kv.Value))
                                valuesToSetInCache.Add(new KeyValuePair<Key<(TK1, TK2)>, TV>(key, kv.Value.Value));
                        }

                        if (valuesToSetInCache != null && valuesToSetInCache.Any())
                        {
                            var setValueTask = _cache.Set(valuesToSetInCache, _timeToLiveFactory(outerKey));

                            if (!setValueTask.IsCompleted)
                                await setValueTask;
                        }
                    }
                }
                catch (Exception ex)
                {
                    var duration = StopwatchHelper.GetDuration(stopwatchStart);

                    var fetchedKeys = results.Select(r => r.Key);

                    results.AddRange(batchInnerKeys
                        .Except(fetchedKeys, _tupleKeysOnlyDifferingBySecondItemComparer)
                        .Select(k =>
                            new FunctionCacheFetchResultInner<(TK1, TK2), TV>(k, default, false, false, duration)));

                    exception = new FunctionCacheFetchException<(TK1, TK2)>(
                        Name,
                        batchInnerKeys,
                        "Unable to fetch value(s)",
                        ex);

                    _onException?.Invoke(exception);
                    TraceHandlerInternal.Mark(exception);

                    throw exception;
                }
                finally
                {
                    var notifyFetch = _onFetch != null || TraceHandlerInternal.Enabled;
                    if (notifyFetch)
                    {
                        var functionCacheFetchResult = new FunctionCacheFetchResult<(TK1, TK2), TV>(
                            Name,
                            results,
                            exception,
                            start,
                            StopwatchHelper.GetDuration(stopwatchStart));

                        _onFetch?.Invoke(functionCacheFetchResult);
                        TraceHandlerInternal.Mark(functionCacheFetchResult);
                    }
                }

                return results;
            }
        }

        private Key<(TK1, TK2)>[] BuildCombinedKeys(TK1 outerKey, IEnumerable<TK2> innerKeys)
        {
            var outerKeySerializer = new Lazy<string>(() => _outerKeySerializer(outerKey));
            
            return innerKeys
                .Distinct(_innerKeyComparer)
                .Select(k => new Key<(TK1, TK2)>((outerKey, k), Serialize))
                .ToArray();

            string Serialize((TK1, TK2) key)
            {
                return outerKeySerializer.Value + _keyParamSeparator + _innerKeySerializer(key.Item2);
            }
        }

        private Func<DuplicateTaskCatcherMultiResult<TK2, TV>, bool> BuildSetInCachePredicate(TK1 outerKey)
        {
            if (_skipCacheSetPredicateOuterKeyOnly != null && _skipCacheSetPredicateOuterKeyOnly(outerKey))
                return _alwaysFalseSetInCachePredicate;
            
            if (_skipCacheSetPredicate != null)
                return k => !k.Duplicate && !_skipCacheSetPredicate((outerKey, k.Key));

            return _skipDuplicatesSetInCachePredicate;
        }

        private Func<TK1, Key<(TK1, TK2)>[], Key<(TK1, TK2)>[]> SetupKeysToGetFromCacheFunc(
            Func<(TK1, TK2), bool> skipCacheGetPredicate,
            Func<TK1, bool> skipCacheGetPredicateOuterKeyOnly)
        {
            if (skipCacheGetPredicate != null && skipCacheGetPredicateOuterKeyOnly != null)
            {
                return (outerKey, innerKeys) => skipCacheGetPredicateOuterKeyOnly(outerKey)
                    ? _emptyKeyArray
                    : innerKeys.Where(k => !skipCacheGetPredicate(k)).ToArray();
            }
            
            if (skipCacheGetPredicateOuterKeyOnly != null)
            {
                return (outerKey, innerKeys) => skipCacheGetPredicateOuterKeyOnly(outerKey)
                    ? _emptyKeyArray
                    : innerKeys;
            }
            
            if (skipCacheGetPredicate != null)
            {
                return (outerKey, innerKeys) => innerKeys
                    .Where(k => !skipCacheGetPredicate(k))
                    .ToArray();
            }

            return (outerKey, innerKeys) => innerKeys;
        }

        private static Func<TK1, ICollection<TK2>, CancellationToken, Task<IDictionary<TK2, TV>>> ConvertIntoNegativeCachingFunc(
            Func<TK1, ICollection<TK2>, CancellationToken, Task<IDictionary<TK2, TV>>> func,
            Func<(TK1, TK2), TV> negativeCachingValueFactory,
            IEqualityComparer<TK2> keyComparer)
        {
            return async (outerKey, innerKeys, token) =>
            {
                var values = await func(outerKey, innerKeys, token) ?? new Dictionary<TK2, TV>();

                IDictionary<TK2, TV> valuesWithFills = null;
                foreach (var key in innerKeys)
                {
                    if (values.ContainsKey(key))
                        continue;
                    
                    if (valuesWithFills == null)
                        valuesWithFills = values.ToDictionary(kv => kv.Key, kv => kv.Value, keyComparer);

                    valuesWithFills[key] = negativeCachingValueFactory((outerKey, key));
                }

                return valuesWithFills ?? values;
            };
        }

        // Use this comparer whenever we know the first component of each key is the same
        private class TupleKeysOnlyDifferingBySecondItemComparer : IEqualityComparer<Key<(TK1, TK2)>>
        {
            private readonly IEqualityComparer<TK2> _comparer;

            public TupleKeysOnlyDifferingBySecondItemComparer(IEqualityComparer<TK2> comparer)
            {
                _comparer = comparer;
            }

            public bool Equals(Key<(TK1, TK2)> x, Key<(TK1, TK2)> y)
            {
                return _comparer.Equals(x.AsObject.Item2, y.AsObject.Item2);
            }

            public int GetHashCode(Key<(TK1, TK2)> obj)
            {
                return _comparer.GetHashCode(obj.AsObject.Item2);
            }
        }
    }
}