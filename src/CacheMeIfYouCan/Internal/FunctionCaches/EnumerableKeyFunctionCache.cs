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
    internal sealed class EnumerableKeyFunctionCache<TK, TV> : IPendingRequestsCounter, IDisposable
    {
        private readonly ICacheInternal<TK, TV> _cache;
        private readonly Func<TimeSpan> _timeToLiveFactory;
        private readonly Func<TK, string> _keySerializer;
        private readonly Func<TK, TV> _defaultValueFactory;
        private readonly bool _continueOnException;
        private readonly Action<FunctionCacheGetResult<TK, TV>> _onResult;
        private readonly Action<FunctionCacheFetchResult<TK, TV>> _onFetch;
        private readonly Action<FunctionCacheException<TK>> _onException;
        private readonly KeyComparer<TK> _keyComparer;
        private readonly int _maxFetchBatchSize;
        private readonly BatchBehaviour _batchBehaviour;
        private readonly IDuplicateTaskCatcherMulti<TK, TV> _fetchHandler;
        private readonly Func<Key<TK>[], Key<TK>[]> _keysToGetFromCacheFunc;
        private readonly Func<DuplicateTaskCatcherMultiResult<TK, TV>, bool> _setInCachePredicate;
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
            Func<TK, bool> skipCacheSetPredicate,
            Func<TK, TV> missingKeyValueFactory)
        {
            Name = functionName;
            Type = GetType().Name;
            _cache = cache;
            _timeToLiveFactory = timeToLiveFactory;
            _keySerializer = keySerializer;
            _defaultValueFactory = defaultValueFactory;
            _continueOnException = defaultValueFactory != null;
            _onResult = onResult;
            _onFetch = onFetch;
            _onException = onException;
            _keyComparer = keyComparer;
            _maxFetchBatchSize = maxFetchBatchSize <= 0 ? Int32.MaxValue : maxFetchBatchSize;
            _batchBehaviour = batchBehaviour;

            var convertedFunc = missingKeyValueFactory == null
                ? func
                : ConvertToFuncThatFillsMissingKeys(func, missingKeyValueFactory, keyComparer);
            
            if (catchDuplicateRequests)
                _fetchHandler = new DuplicateTaskCatcherMulti<TK, TV>(convertedFunc, keyComparer);
            else
                _fetchHandler = new DisabledDuplicateTaskCatcherMulti<TK, TV>(convertedFunc, keyComparer);

            if (skipCacheGetPredicate == null)
                _keysToGetFromCacheFunc = keys => keys;
            else
                _keysToGetFromCacheFunc = keys => keys.Where(k => !skipCacheGetPredicate(k)).ToArray();

            if (skipCacheSetPredicate == null)
                _setInCachePredicate = kv => !kv.Duplicate;
            else
                _setInCachePredicate = kv => !kv.Duplicate && !skipCacheSetPredicate(kv.Key);
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
                .ToArray();

            var results = new Dictionary<Key<TK>, FunctionCacheGetResultInner<TK, TV>>(keys.Length, _keyComparer);

            IReadOnlyCollection<FunctionCacheGetResultInner<TK, TV>> readonlyResults = null;
            FunctionCacheGetException<TK> exception = null;
    
            using (SynchronizationContextRemover.StartNew())
            using (TraceHandlerInternal.Start())
            {
                try
                {
                    token.ThrowIfCancellationRequested();
                    
                    Interlocked.Increment(ref _pendingRequestsCount);

                    Key<TK>[] missingKeys = null;
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
                                    results[result.Key] = new FunctionCacheGetResultInner<TK, TV>(
                                        result.Key,
                                        result.Value,
                                        Outcome.FromCache,
                                        result.CacheType);
                                }

                                missingKeys = keys
                                    .Except(results.Keys, _keyComparer)
                                    .ToArray();
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
                        .ToArray();
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

        private async Task<IList<FunctionCacheFetchResultInner<TK, TV>>> Fetch(Key<TK>[] keys, CancellationToken token)
        {
            if (keys.Length <= _maxFetchBatchSize)
                return await FetchBatch(keys);

            int batchSize;
            if (_batchBehaviour == BatchBehaviour.FillBatchesEvenly)
            {
                var batchesCount = (keys.Length / _maxFetchBatchSize) + 1;

                batchSize = (keys.Length / batchesCount) + 1;
            }
            else
            {
                batchSize = _maxFetchBatchSize;
            }
            
            var tasks = keys
                .Batch(batchSize)
                .Select(FetchBatch)
                .ToArray();

            await Task.WhenAll(tasks);

            return tasks
                .SelectMany(t => t.Result)
                .ToArray();
            
            async Task<IList<FunctionCacheFetchResultInner<TK, TV>>> FetchBatch(IList<Key<TK>> batchKeys)
            {
                var start = DateTime.UtcNow;
                var stopwatchStart = Stopwatch.GetTimestamp();

                var results = new List<FunctionCacheFetchResultInner<TK, TV>>();
                FunctionCacheFetchException<TK> exception = null;

                try
                {
                    var fetched = await _fetchHandler.ExecuteAsync(batchKeys.Select(k => k.AsObject).ToArray(), token);

                    if (fetched != null && fetched.Any())
                    {
                        var keysAndFetchedValues = new Dictionary<Key<TK>, DuplicateTaskCatcherMultiResult<TK, TV>>(_keyComparer);
                        foreach (var key in batchKeys)
                        {
                            if (fetched.TryGetValue(key, out var value))
                                keysAndFetchedValues[key] = value;
                        }

                        results.AddRange(keysAndFetchedValues
                            .Select(f => new FunctionCacheFetchResultInner<TK, TV>(
                                f.Key,
                                f.Value.Value,
                                true,
                                f.Value.Duplicate,
                                StopwatchHelper.GetDuration(stopwatchStart, f.Value.StopwatchTimestampCompleted))));

                        if (_cache != null)
                        {
                            var valuesToSetInCache = keysAndFetchedValues
                                .Where(kv => _setInCachePredicate(kv.Value))
                                .ToDictionary(kv => kv.Key, kv => kv.Value.Value, _keyComparer);

                            if (valuesToSetInCache.Any())
                            {
                                var setValueTask = _cache.Set(valuesToSetInCache, _timeToLiveFactory());

                                if (!setValueTask.IsCompleted)
                                    await setValueTask;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    var duration = StopwatchHelper.GetDuration(stopwatchStart);

                    var fetchedKeys = new HashSet<Key<TK>>(results.Select(r => r.Key), _keyComparer);

                    results.AddRange(batchKeys
                        .Except(fetchedKeys)
                        .Select(k => new FunctionCacheFetchResultInner<TK, TV>(k, default, false, false, duration)));

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
        
        private static Func<ICollection<TK>, CancellationToken, Task<IDictionary<TK, TV>>> ConvertToFuncThatFillsMissingKeys(
            Func<ICollection<TK>, CancellationToken, Task<IDictionary<TK, TV>>> func,
            Func<TK, TV> missingKeyValueFactory,
            IEqualityComparer<TK> keyComparer)
        {
            return async (keys, token) =>
            {
                var values = await func(keys, token) ?? new Dictionary<TK, TV>();

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