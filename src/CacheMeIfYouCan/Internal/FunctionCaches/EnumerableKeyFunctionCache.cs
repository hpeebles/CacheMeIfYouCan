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
    internal sealed class EnumerableKeyFunctionCache<TK, TV> : IPendingRequestsCounter, IDisposable
    {
        private readonly ICacheInternal<TK, TV> _cache;
        private readonly Func<TimeSpan> _timeToLiveFactory;
        private readonly bool _catchDuplicateRequests;
        private readonly Func<TK, string> _keySerializer;
        private readonly Func<TK, TV> _defaultValueFactory;
        private readonly bool _continueOnException;
        private readonly Action<FunctionCacheGetResult<TK, TV>> _onResult;
        private readonly Action<FunctionCacheFetchResult<TK, TV>> _onFetch;
        private readonly Action<FunctionCacheException<TK>> _onException;
        private readonly KeyComparer<TK> _keyComparer;
        private readonly int _maxFetchBatchSize;
        private readonly BatchBehaviour _batchBehaviour;
        private readonly Func<TK, bool> _skipCacheGetPredicate;
        private readonly Func<TK, TV, bool> _skipCacheSetPredicate;
        private readonly IDuplicateTaskCatcherMulti<TK, TV> _fetchHandler;
        private int _pendingRequestsCount;
        private bool _disposed;
        
        public EnumerableKeyFunctionCache(
            Func<IEnumerable<TK>, CancellationToken, Task<IDictionary<TK, TV>>> func,
            string functionName,
            ICacheInternal<TK, TV> cache,
            Func<TimeSpan> timeToLiveFactory,
            bool catchDuplicateRequests,
            Func<TK, string> keySerializer,
            Func<TK, TV> defaultValueFactory,
            Action<FunctionCacheGetResult<TK, TV>> onResult,
            Action<FunctionCacheFetchResult<TK, TV>> onFetch,
            Action<FunctionCacheException<TK>> onException,
            KeyComparer<TK> keyComparer,
            int maxFetchBatchSize,
            BatchBehaviour batchBehaviour,
            Func<TK, bool> skipCacheGetPredicate,
            Func<TK, TV, bool> skipCacheSetPredicate,
            Func<TK, TV> missingKeyValueFactory)
        {
            Name = functionName;
            Type = GetType().Name;
            _cache = cache;
            _timeToLiveFactory = timeToLiveFactory;
            _catchDuplicateRequests = catchDuplicateRequests;
            _keySerializer = keySerializer;
            _defaultValueFactory = defaultValueFactory;
            _continueOnException = defaultValueFactory != null;
            _onResult = onResult;
            _onFetch = onFetch;
            _onException = onException;
            _keyComparer = keyComparer;
            _maxFetchBatchSize = maxFetchBatchSize <= 0 ? Int32.MaxValue : maxFetchBatchSize;
            _batchBehaviour = batchBehaviour;
            _skipCacheGetPredicate = skipCacheGetPredicate;
            _skipCacheSetPredicate = skipCacheSetPredicate;
            
            var convertedFunc = missingKeyValueFactory == null
                ? func
                : ConvertToFuncThatFillsMissingKeys(func, missingKeyValueFactory, keyComparer);
            
            if (catchDuplicateRequests)
                _fetchHandler = new DuplicateTaskCatcherMulti<TK, TV>(convertedFunc, keyComparer);
            else
                _fetchHandler = new DisabledDuplicateTaskCatcherMulti<TK, TV>(convertedFunc, keyComparer);
        }

        public string Name { get; }
        public string Type { get; }
        public int PendingRequestsCount => _pendingRequestsCount;
        
        public void Dispose()
        {
            _disposed = true;
            PendingRequestsCounterContainer.Remove(this);
        }
        
        public async Task<IReadOnlyCollection<IKeyValuePair<TK, TV>>> GetMulti(IEnumerable<TK> keyObjs, CancellationToken token)
        {
            if (_disposed)
                throw new ObjectDisposedException($"{Name} - {Type}");
            
            var start = DateTime.UtcNow;
            var stopwatchStart = Stopwatch.GetTimestamp();
            
            var keys = keyObjs
                .Distinct(_keyComparer)
                .Select(k => new Key<TK>(k, _keySerializer))
                .ToList();

            var results = new Dictionary<Key<TK>, FunctionCacheGetResultInner<TK, TV>>(keys.Count, _keyComparer);

            IReadOnlyCollection<FunctionCacheGetResultInner<TK, TV>> readonlyResults = null;
            FunctionCacheGetException<TK> exception = null;
    
            using (SynchronizationContextRemover.StartNew())
            using (TraceHandlerInternal.Start())
            {
                try
                {
                    token.ThrowIfCancellationRequested();
                    
                    Interlocked.Increment(ref _pendingRequestsCount);

                    List<Key<TK>> missingKeys = null;
                    if (_cache != null)
                    {
                        var keysToGet = FilterToKeysToGetFromCache(keys);

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
                                    results[result.Key] = new FunctionCacheGetResultInner<TK, TV>(
                                        result.Key,
                                        result.Value,
                                        Outcome.FromCache,
                                        result.CacheType);
                                }

                                missingKeys = keys
                                    .Except(results.Keys, _keyComparer)
                                    .ToList();
                            }
                        }
                    }

                    if (missingKeys == null)
                        missingKeys = keys;

                    if (missingKeys.Any())
                    {
                        var fetched = await Fetch(missingKeys, token);

                        if (fetched != null && fetched.Any())
                        {
                            foreach (var result in fetched)
                            {
                                results[result.Key] = new FunctionCacheGetResultInner<TK, TV>(
                                    result.Key,
                                    result.Value,
                                    Outcome.Fetch,
                                    null);
                            }
                        }
                    }
                    
#if NET45
                    readonlyResults = results.Values.ToList();
#else
                    readonlyResults = results.Values;
#endif
                }
                catch (Exception ex)
                {
                    var message = _continueOnException
                        ? "Unable to get value(s). Default being returned"
                        : "Unable to get value(s)";

                    exception = new FunctionCacheGetException<TK>(
                        Name,
                        keys,
                        message,
                        ex);

                    _onException?.Invoke(exception);
                    TraceHandlerInternal.Mark(exception);

                    if (!_continueOnException)
                        throw exception;
            
                    readonlyResults = keys
                        .Select(k => new FunctionCacheGetResultInner<TK, TV>(k, _defaultValueFactory(k), Outcome.Error, null))
                        .ToList();
                }
                finally
                {
                    Interlocked.Decrement(ref _pendingRequestsCount);

                    var notifyResult = _onResult != null || TraceHandlerInternal.Enabled;
                    if (notifyResult)
                    {
                        var functionCacheGetResult = new FunctionCacheGetResult<TK, TV>(
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

        private async Task<IList<FunctionCacheFetchResultInner<TK, TV>>> Fetch(IReadOnlyList<Key<TK>> keys, CancellationToken token)
        {
            if (keys.Count <= _maxFetchBatchSize)
                return await FetchBatch(keys);

            int batchSize;
            if (_batchBehaviour == BatchBehaviour.FillBatchesEvenly)
            {
                var batchesCount = (keys.Count / _maxFetchBatchSize) + 1;

                batchSize = (keys.Count / batchesCount) + 1;
            }
            else
            {
                batchSize = _maxFetchBatchSize;
            }
            
            var tasks = keys
                .Batch(batchSize)
                .Select(FetchBatch)
                .ToList();

            await Task.WhenAll(tasks);

            return tasks
                .SelectMany(t => t.Result)
                .ToList();
            
            async Task<IList<FunctionCacheFetchResultInner<TK, TV>>> FetchBatch(IReadOnlyCollection<Key<TK>> batchKeys)
            {
                var start = DateTime.UtcNow;
                var stopwatchStart = Stopwatch.GetTimestamp();

                List<FunctionCacheFetchResultInner<TK, TV>> results = null;
                FunctionCacheFetchException<TK> exception = null;

                try
                {
                    var fetched = await _fetchHandler.ExecuteAsync(batchKeys.Select(k => k.AsObject).ToList(), token);

                    if (fetched != null && fetched.Any())
                    {
                        results = ProcessFetchedValues(batchKeys, fetched, stopwatchStart, out var valuesToSetInCache);

                        if (valuesToSetInCache != null && valuesToSetInCache.Any())
                        {
                            var setValueTask = _cache.Set(valuesToSetInCache, _timeToLiveFactory());

                            if (!setValueTask.IsCompleted)
                                await setValueTask;
                        }
                    }
                }
                catch (Exception ex)
                {
                    var duration = StopwatchHelper.GetDuration(stopwatchStart);

                    if (results == null)
                    {
                        results = new List<FunctionCacheFetchResultInner<TK, TV>>(batchKeys.Count);
                        
                        foreach (var key in batchKeys)
                            results.Add(new FunctionCacheFetchResultInner<TK, TV>(key, default, false, false, duration));
                    }
                    else
                    {
                        var fetchedKeys = results.Select(r => r.Key);

                        results.AddRange(batchKeys
                            .Except(fetchedKeys, _keyComparer)
                            .Select(k => new FunctionCacheFetchResultInner<TK, TV>(k, default, false, false, duration)));
                    }
                    
                    exception = new FunctionCacheFetchException<TK>(
                        Name,
                        batchKeys,
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
                        var functionCacheFetchResult = new FunctionCacheFetchResult<TK, TV>(
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

        private IReadOnlyCollection<Key<TK>> FilterToKeysToGetFromCache(IReadOnlyCollection<Key<TK>> keys)
        {
            if (_skipCacheGetPredicate == null)
                return keys;

            List<Key<TK>> keysToGetIfAnyExcluded = null; // This will be null if no keys have been excluded
            var index = 0;
            foreach (var key in keys)
            {
                if (_skipCacheGetPredicate(key))
                {
                    if (keysToGetIfAnyExcluded == null)
                        keysToGetIfAnyExcluded = keys.Take(index).ToList();
                }
                else
                {
                    keysToGetIfAnyExcluded?.Add(key);
                }
                
                index++;
            }

            return keysToGetIfAnyExcluded ?? keys;
        }

        private List<FunctionCacheFetchResultInner<TK, TV>> ProcessFetchedValues(
            IReadOnlyCollection<Key<TK>> keys,
            IDictionary<TK, DuplicateTaskCatcherMultiResult<TK, TV>> values,
            long stopwatchStart,
            out Dictionary<Key<TK>, TV> valuesToSetInCache)
        {
            var results = new List<FunctionCacheFetchResultInner<TK, TV>>(values.Count);

            var keysMap = keys.ToDictionary(k => k.AsObject, _keyComparer);

            if (_cache == null)
            {
                valuesToSetInCache = null;
                foreach (var kv in values)
                    results.Add(ConvertValue(keysMap[kv.Key], kv.Value));
            }
            else if (_skipCacheSetPredicate == null)
            {
                valuesToSetInCache = new Dictionary<Key<TK>, TV>(values.Count, _keyComparer);

                if (_catchDuplicateRequests)
                {
                    foreach (var kv in values)
                    {
                        var key = keysMap[kv.Key];
                        
                        if (!kv.Value.Duplicate)
                            valuesToSetInCache[key] = kv.Value.Value;
                        
                        results.Add(ConvertValue(key, kv.Value));
                    }
                }
                else
                {
                    foreach (var kv in values)
                    {
                        var key = keysMap[kv.Key];
                        
                        valuesToSetInCache[key] = kv.Value.Value;
                        results.Add(ConvertValue(key, kv.Value));
                    }
                }
            }
            else
            {
                valuesToSetInCache = new Dictionary<Key<TK>, TV>(_keyComparer);

                foreach (var kv in values)
                {
                    var key = keysMap[kv.Key];
                    var value = kv.Value.Value;
                    
                    if (!kv.Value.Duplicate && !_skipCacheSetPredicate(kv.Key, value))
                        valuesToSetInCache[key] = value;

                    results.Add(ConvertValue(key, kv.Value));
                }
            }

            return results;
            
            FunctionCacheFetchResultInner<TK, TV> ConvertValue(Key<TK> key, DuplicateTaskCatcherMultiResult<TK, TV> fetchedValue)
            {
                return new FunctionCacheFetchResultInner<TK, TV>(
                    key,
                    fetchedValue.Value,
                    true,
                    fetchedValue.Duplicate,
                    StopwatchHelper.GetDuration(stopwatchStart, fetchedValue.StopwatchTimestampCompleted));
            }
        }
        
        private static Func<IReadOnlyCollection<TK>, CancellationToken, Task<IDictionary<TK, TV>>> ConvertToFuncThatFillsMissingKeys(
            Func<IReadOnlyCollection<TK>, CancellationToken, Task<IDictionary<TK, TV>>> func,
            Func<TK, TV> missingKeyValueFactory,
            IEqualityComparer<TK> keyComparer)
        {
            return async (keys, token) =>
            {
                var values = await func(keys, token) ?? new Dictionary<TK, TV>(keyComparer);

                IDictionary<TK, TV> valuesWithFills = null;
                foreach (var key in keys)
                {
                    if (values.ContainsKey(key))
                        continue;
                    
                    if (valuesWithFills == null)
                        valuesWithFills = values.ToDictionary(kv => kv.Key, kv => kv.Value, keyComparer);

                    valuesWithFills[key] = missingKeyValueFactory(key);
                }

                return valuesWithFills ?? values;
            };
        }
    }
}