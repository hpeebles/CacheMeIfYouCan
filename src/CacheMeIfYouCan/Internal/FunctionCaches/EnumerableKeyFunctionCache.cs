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
        private readonly TimeSpan _timeToLive;
        private readonly Func<TK, string> _keySerializer;
        private readonly Func<TV> _defaultValueFactory;
        private readonly bool _continueOnException;
        private readonly Action<FunctionCacheGetResult<TK, TV>> _onResult;
        private readonly Action<FunctionCacheFetchResult<TK, TV>> _onFetch;
        private readonly Action<FunctionCacheException<TK>> _onException;
        private readonly KeyComparer<TK> _keyComparer;
        private readonly int _maxFetchBatchSize;
        private readonly DuplicateTaskCatcherMulti<TK, TV> _fetchHandler;
        private readonly Func<TK, bool> _skipCacheGetPredicate;
        private readonly Func<DuplicateTaskCatcherMultiResult<TK, TV>, bool> _setInCachePredicate;
        private int _pendingRequestsCount;
        private bool _disposed;
        
        public EnumerableKeyFunctionCache(
            Func<IEnumerable<TK>, Task<IDictionary<TK, TV>>> func,
            string functionName,
            ICacheInternal<TK, TV> cache,
            TimeSpan timeToLive,
            Func<TK, string> keySerializer,
            Func<TV> defaultValueFactory,
            Action<FunctionCacheGetResult<TK, TV>> onResult,
            Action<FunctionCacheFetchResult<TK, TV>> onFetch,
            Action<FunctionCacheException<TK>> onException,
            KeyComparer<TK> keyComparer,
            int maxFetchBatchSize,
            Func<TK, bool> skipCacheGetPredicate,
            Func<TK, bool> skipCacheSetPredicate,
            Func<TK, TV> negativeCachingValueFactory)
        {
            Name = functionName;
            Type = GetType().Name;
            _cache = cache;
            _timeToLive = timeToLive;
            _keySerializer = keySerializer;
            _defaultValueFactory = defaultValueFactory;
            _continueOnException = defaultValueFactory != null;
            _onResult = onResult;
            _onFetch = onFetch;
            _onException = onException;
            _keyComparer = keyComparer;
            _maxFetchBatchSize = maxFetchBatchSize <= 0 ? Int32.MaxValue : maxFetchBatchSize;
            _fetchHandler = new DuplicateTaskCatcherMulti<TK, TV>(
                negativeCachingValueFactory == null
                    ? func
                    : ConvertIntoNegativeCachingFunc(func, negativeCachingValueFactory, keyComparer),
                keyComparer);
            _skipCacheGetPredicate = skipCacheGetPredicate;

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
        
        public async Task<IReadOnlyCollection<IKeyValuePair<TK, TV>>> GetMulti(IEnumerable<TK> keyObjs)
        {
            if (_disposed)
                throw new ObjectDisposedException($"{Name} - {Type}");
            
            var timestamp = Timestamp.Now;
            var stopwatchStart = Stopwatch.GetTimestamp();
            
            Dictionary<Key<TK>, FunctionCacheGetResultInner<TK, TV>> results;
            if (keyObjs is ICollection<TK> c)
                results = new Dictionary<Key<TK>, FunctionCacheGetResultInner<TK, TV>>(c.Count, _keyComparer);
            else
                results = new Dictionary<Key<TK>, FunctionCacheGetResultInner<TK, TV>>(_keyComparer);

            var keys = keyObjs
                .Select(k => new Key<TK>(k, _keySerializer))
                .ToArray();

            var error = false;

            IReadOnlyCollection<FunctionCacheGetResultInner<TK, TV>> readonlyResults;
    
            using (SynchronizationContextRemover.StartNew())
            {
                try
                {
                    Interlocked.Increment(ref _pendingRequestsCount);

                    Key<TK>[] missingKeys = null;
                    if (_cache != null)
                    {
                        var keysToGet = _skipCacheGetPredicate == null
                            ? keys
                            : keys.Where(k => !_skipCacheGetPredicate(k)).ToArray();

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
                        var fetched = await Fetch(missingKeys);

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

                    _onResult?.Invoke(new FunctionCacheGetResult<TK, TV>(
                        Name,
                        readonlyResults,
                        !error,
                        timestamp,
                        StopwatchHelper.GetDuration(stopwatchStart)));
                }
            }

            return readonlyResults;
        }

        private async Task<IList<FunctionCacheFetchResultInner<TK, TV>>> Fetch(Key<TK>[] keys)
        {
            if (keys.Length < _maxFetchBatchSize)
                return await FetchBatch(keys);

            var tasks = keys
                .Batch(_maxFetchBatchSize)
                .Select(FetchBatch)
                .ToArray();

            await Task.WhenAll(tasks);

            return tasks
                .SelectMany(t => t.Result)
                .ToArray();
            
            async Task<IList<FunctionCacheFetchResultInner<TK, TV>>> FetchBatch(IList<Key<TK>> batchKeys)
            {
                var timestamp = Timestamp.Now;
                var stopwatchStart = Stopwatch.GetTimestamp();
                var error = false;

                var results = new List<FunctionCacheFetchResultInner<TK, TV>>();

                try
                {
                    var fetched = await _fetchHandler.ExecuteAsync(batchKeys.Select(k => k.AsObject).ToArray());

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
                                var setValueTask = _cache.Set(valuesToSetInCache, _timeToLive);

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

                    var exception = new FunctionCacheFetchException<TK>(
                        Name,
                        batchKeys,
                        Timestamp.Now,
                        "Unable to fetch value(s)",
                        ex);

                    _onException?.Invoke(exception);

                    error = true;
                    throw exception;
                }
                finally
                {
                    _onFetch?.Invoke(new FunctionCacheFetchResult<TK, TV>(
                        Name,
                        results,
                        !error,
                        timestamp,
                        StopwatchHelper.GetDuration(stopwatchStart)));
                }

                return results;
            }
        }

        private IReadOnlyCollection<FunctionCacheGetResultInner<TK, TV>> HandleError(IList<Key<TK>> keys, Exception ex)
        {
            var message = _continueOnException
                ? "Unable to get value(s). Default being returned"
                : "Unable to get value(s)";

            var exception = new FunctionCacheGetException<TK>(
                Name,
                keys,
                Timestamp.Now,
                message,
                ex);
            
            _onException?.Invoke(exception);

            if (!_continueOnException)
                throw exception;
            
            var defaultValue = _defaultValueFactory == null
                ? default
                : _defaultValueFactory();
            
            return keys
                .Select(k => new FunctionCacheGetResultInner<TK, TV>(k, defaultValue, Outcome.Error, null))
                .ToArray();
        }
        
        private static Func<IEnumerable<TK>, Task<IDictionary<TK, TV>>> ConvertIntoNegativeCachingFunc(
            Func<IEnumerable<TK>, Task<IDictionary<TK, TV>>> func,
            Func<TK, TV> negativeCachingValueFactory,
            IEqualityComparer<TK> keyComparer)
        {
            return async keys =>
            {
                var values = await func(keys);

                IDictionary<TK, TV> valuesWithFills = null;
                foreach (var key in keys)
                {
                    if (values.ContainsKey(key))
                        continue;
                    
                    if (valuesWithFills == null)
                        valuesWithFills = values.ToDictionary(kv => kv.Key, kv => kv.Value, keyComparer);

                    valuesWithFills[key] = negativeCachingValueFactory(key);
                }

                return valuesWithFills ?? values;
            };
        }
    }
}