using System;
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
        private readonly Func<TV> _defaultValueFactory;
        private readonly bool _continueOnException;
        private readonly Action<FunctionCacheGetResult<(TK1, TK2), TV>> _onResult;
        private readonly Action<FunctionCacheFetchResult<(TK1, TK2), TV>> _onFetch;
        private readonly Action<FunctionCacheException<(TK1, TK2)>> _onException;
        private readonly KeyComparer<TK2> _innerKeyComparer;
        private readonly IEqualityComparer<Key<(TK1, TK2)>> _tupleKeysOnlyDifferingBySecondItemComparer;
        private readonly string _keyParamSeparator;
        private readonly int _maxFetchBatchSize;
        private readonly DuplicateTaskCatcherCombinedMulti<TK1, TK2, TV> _fetchHandler;
        private readonly Func<Key<(TK1, TK2)>[], Key<(TK1, TK2)>[]> _keysToGetFromCacheFunc;
        private readonly Func<(TK1, TK2), bool> _skipCacheSetPredicate;
        private readonly Func<int, List<KeyValuePair<Key<(TK1, TK2)>, TV>>> _valuesToSetInCacheListFactory;
        private readonly Func<DuplicateTaskCatcherMultiResult<TK2, TV>, bool> _setInCachePredicate;
        private int _pendingRequestsCount;
        private bool _disposed;
        
        public MultiParamEnumerableKeyFunctionCache(
            Func<TK1, IEnumerable<TK2>, CancellationToken, Task<IDictionary<TK2, TV>>> func,
            string functionName,
            ICacheInternal<(TK1, TK2), TV> cache,
            Func<TK1, TimeSpan> timeToLiveFactory,
            Func<TK1, string> outerKeySerializer,
            Func<TK2, string> innerKeySerializer,
            Func<TV> defaultValueFactory,
            Action<FunctionCacheGetResult<(TK1, TK2), TV>> onResult,
            Action<FunctionCacheFetchResult<(TK1, TK2), TV>> onFetch,
            Action<FunctionCacheException<(TK1, TK2)>> onException,
            KeyComparer<TK1> outerKeyComparer,
            KeyComparer<TK2> innerKeyComparer,
            string keyParamSeparator,
            int maxFetchBatchSize,
            Func<(TK1, TK2), bool> skipCacheGetPredicate,
            Func<(TK1, TK2), bool> skipCacheSetPredicate,
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
            _maxFetchBatchSize = maxFetchBatchSize <= 0 ? Int32.MaxValue : maxFetchBatchSize;
            
            _fetchHandler = new DuplicateTaskCatcherCombinedMulti<TK1, TK2, TV>(
                negativeCachingValueFactory == null
                    ? func
                    : ConvertIntoNegativeCachingFunc(func, negativeCachingValueFactory, _innerKeyComparer),
                outerKeyComparer.Inner,
                innerKeyComparer.Inner);

            if (skipCacheGetPredicate == null)
                _keysToGetFromCacheFunc = keys => keys;
            else
                _keysToGetFromCacheFunc = keys => keys.Where(k => !skipCacheGetPredicate(k)).ToArray();

            _skipCacheSetPredicate = skipCacheSetPredicate;

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
                _setInCachePredicate = x => false;
                _valuesToSetInCacheListFactory = null;
            }
            // If skipCacheSetPredicate is null, setInCachePredicate should only exclude duplicates. Given that in the
            // vast majority of cases duplicates will be rare we should initialize the list with enough capacity to
            // store all of the fetched values.
            else if (skipCacheSetPredicate == null)
            {
                _setInCachePredicate = x => !x.Duplicate;
                _valuesToSetInCacheListFactory = count => new List<KeyValuePair<Key<(TK1, TK2)>, TV>>(count);
            }
            // If skipCacheSetPredicate is not null, initialize valuesToSetInCache using the default list constructor.
            // Also, we must build a new version of setInCachePredicate each time
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
            
            Dictionary<Key<(TK1, TK2)>, FunctionCacheGetResultInner<(TK1, TK2), TV>> results;
            if (innerKeys is ICollection<TK2> c)
            {
                results = new Dictionary<Key<(TK1, TK2)>, FunctionCacheGetResultInner<(TK1, TK2), TV>>(
                    c.Count,
                    _tupleKeysOnlyDifferingBySecondItemComparer);
            }
            else
            {
                results = new Dictionary<Key<(TK1, TK2)>, FunctionCacheGetResultInner<(TK1, TK2), TV>>(
                    _tupleKeysOnlyDifferingBySecondItemComparer);
            }

            var keys = BuildCombinedKeys(outerKey, innerKeys);

            var error = false;

            IReadOnlyCollection<FunctionCacheGetResultInner<(TK1, TK2), TV>> readonlyResults;
    
            using (SynchronizationContextRemover.StartNew())
            {
                try
                {
                    token.ThrowIfCancellationRequested();
                    
                    Interlocked.Increment(ref _pendingRequestsCount);

                    Key<(TK1, TK2)>[] missingKeys = null;
                    if (_cache != null)
                    {
                        var keysToGet = _keysToGetFromCacheFunc(keys);

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
                }
                catch (Exception ex)
                {
                    error = true;
                    return HandleError(keys, ex);
                }
                finally
                {
#if NET45
                    readonlyResults = results.Values.ToArray();
#else
                    readonlyResults = results.Values;
#endif

                    Interlocked.Decrement(ref _pendingRequestsCount);

                    _onResult?.Invoke(new FunctionCacheGetResult<(TK1, TK2), TV>(
                        Name,
                        readonlyResults,
                        !error,
                        start,
                        StopwatchHelper.GetDuration(stopwatchStart)));
                }
            }

            return readonlyResults;
        }

        private async Task<IList<FunctionCacheFetchResultInner<(TK1, TK2), TV>>> Fetch(
            TK1 outerKey,
            Key<(TK1, TK2)>[] innerKeys,
            CancellationToken token)
        {
            if (innerKeys.Length < _maxFetchBatchSize)
                return await FetchBatch(innerKeys);

            var tasks = innerKeys
                .Batch(_maxFetchBatchSize)
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
                var error = false;

                var results = new List<FunctionCacheFetchResultInner<(TK1, TK2), TV>>();

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

                    var exception = new FunctionCacheFetchException<(TK1, TK2)>(
                        Name,
                        batchInnerKeys,
                        "Unable to fetch value(s)",
                        ex);

                    _onException?.Invoke(exception);

                    error = true;
                    throw exception;
                }
                finally
                {
                    _onFetch?.Invoke(new FunctionCacheFetchResult<(TK1, TK2), TV>(
                        Name,
                        results,
                        !error,
                        start,
                        StopwatchHelper.GetDuration(stopwatchStart)));
                }

                return results;
            }
        }

        private IReadOnlyCollection<FunctionCacheGetResultInner<(TK1, TK2), TV>> HandleError(
            IList<Key<(TK1, TK2)>> keys,
            Exception ex)
        {
            var message = _continueOnException
                ? "Unable to get value(s). Default being returned"
                : "Unable to get value(s)";

            var exception = new FunctionCacheGetException<(TK1, TK2)>(
                Name,
                keys,
                message,
                ex);
            
            _onException?.Invoke(exception);

            if (!_continueOnException)
                throw exception;
            
            var defaultValue = _defaultValueFactory == null
                ? default
                : _defaultValueFactory();
            
            return keys
                .Select(k => new FunctionCacheGetResultInner<(TK1, TK2), TV>(k, defaultValue, Outcome.Error, null))
                .ToArray();
        }

        private Key<(TK1, TK2)>[] BuildCombinedKeys(TK1 outerKey, IEnumerable<TK2> innerKeys)
        {
            var outerKeySerializer = new Lazy<string>(() => _outerKeySerializer(outerKey));
            
            return innerKeys
                .Select(k => new Key<(TK1, TK2)>((outerKey, k), Serialize))
                .ToArray();

            string Serialize((TK1, TK2) key)
            {
                return outerKeySerializer.Value + _keyParamSeparator + _innerKeySerializer(key.Item2);
            }
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

        private Func<DuplicateTaskCatcherMultiResult<TK2, TV>, bool> BuildSetInCachePredicate(TK1 outerKey)
        {
            return k => !k.Duplicate && !_skipCacheSetPredicate((outerKey, k.Key));
        }

        private static Func<TK1, ICollection<TK2>, CancellationToken, Task<IDictionary<TK2, TV>>> ConvertIntoNegativeCachingFunc(
            Func<TK1, ICollection<TK2>, CancellationToken, Task<IDictionary<TK2, TV>>> func,
            Func<(TK1, TK2), TV> negativeCachingValueFactory,
            IEqualityComparer<TK2> keyComparer)
        {
            return async (outerKey, innerKeys, token) =>
            {
                var values = await func(outerKey, innerKeys, token);

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
    }
}