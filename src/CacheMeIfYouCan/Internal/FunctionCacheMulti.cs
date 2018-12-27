using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CacheMeIfYouCan.Notifications;

namespace CacheMeIfYouCan.Internal
{
    internal sealed class FunctionCacheMulti<TK, TV> : IPendingRequestsCounter, IDisposable
    {
        private readonly Func<IEnumerable<TK>, Task<IDictionary<TK, TV>>> _func;
        private readonly ICacheInternal<TK, TV> _cache;
        private readonly TimeSpan _timeToLive;
        private readonly Func<TK, string> _keySerializer;
        private readonly bool _earlyFetchEnabled;
        private readonly Func<TV> _defaultValueFactory;
        private readonly bool _continueOnException;
        private readonly Action<FunctionCacheGetResult<TK, TV>> _onResult;
        private readonly Action<FunctionCacheFetchResult<TK, TV>> _onFetch;
        private readonly Action<FunctionCacheException<TK>> _onError;
        private readonly IEqualityComparer<Key<TK>> _keyComparer;
        private readonly ConcurrentDictionary<Key<TK>, Task<FetchResults>> _activeFetches;
        private readonly Random _rng;
        private int _pendingRequestsCount;
        private long _averageFetchDuration;
        private bool _disposed;
        
        public FunctionCacheMulti(
            Func<IEnumerable<TK>, Task<IDictionary<TK, TV>>> func,
            string functionName,
            ICacheInternal<TK, TV> cache,
            TimeSpan timeToLive,
            Func<TK, string> keySerializer,
            bool earlyFetchEnabled,
            Func<TV> defaultValueFactory,
            Action<FunctionCacheGetResult<TK, TV>> onResult,
            Action<FunctionCacheFetchResult<TK, TV>> onFetch,
            Action<FunctionCacheException<TK>> onError,
            IEqualityComparer<Key<TK>> keyComparer)
        {
            Name = functionName;
            Type = GetType().Name;
            _func = func;
            _cache = cache;
            _timeToLive = timeToLive;
            _keySerializer = keySerializer;
            _earlyFetchEnabled = earlyFetchEnabled;
            _defaultValueFactory = defaultValueFactory;
            _continueOnException = defaultValueFactory != null;
            _onResult = onResult;
            _onFetch = onFetch;
            _onError = onError;
            _keyComparer = keyComparer;
            _activeFetches = new ConcurrentDictionary<Key<TK>, Task<FetchResults>>(keyComparer);
            _rng = new Random();
        }

        public string Name { get; }
        public string Type { get; }
        public int PendingRequestsCount => _pendingRequestsCount;
        
        public void Dispose()
        {
            _disposed = true;
            PendingRequestsCounterContainer.Remove(this);
        }
        
        public async Task<IDictionary<TK, TV>> GetMulti(IEnumerable<TK> keyObjs)
        {
            if (_disposed)
                throw new ObjectDisposedException($"{Name} - {Type}");
            
            using (SynchronizationContextRemover.StartNew())
            {
                var results = await GetImpl(keyObjs as ICollection<TK> ?? keyObjs.ToArray());

                return results?.ToDictionary(kv => kv.Key.AsObject, kv => kv.Value);
            }
        }

        private async Task<IEnumerable<FunctionCacheGetResultInner<TK, TV>>> GetImpl(ICollection<TK> keyObjs)
        {
            var timestamp = Timestamp.Now;
            var stopwatchStart = Stopwatch.GetTimestamp();
            
            var results = new Dictionary<Key<TK>, FunctionCacheGetResultInner<TK, TV>>(keyObjs.Count, _keyComparer);
            
            var keys = keyObjs
                .Select(k => new Key<TK>(k, _keySerializer))
                .ToArray();

            var error = false;
            
            try
            {
                Interlocked.Increment(ref _pendingRequestsCount);
                
                IList<Key<TK>> missingKeys = null;
                if (_cache != null)
                {
                    var fromCacheTask = _cache.Get(keys);

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

                        if (_earlyFetchEnabled)
                        {
                            var keysToFetchEarly = fromCache
                                .Where(r => ShouldFetchEarly(r.TimeToLive))
                                .Select(r => new KeyToFetch(r.Key, r.TimeToLive))
                                .ToArray();

                            if (keysToFetchEarly.Any())
                                FetchEarly(keysToFetchEarly);
                        }
                    }
                }

                if (missingKeys == null)
                    missingKeys = keys;
                
                if (missingKeys.Any())
                {
                    var fetched = await FetchOnDemand(missingKeys);

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

                return results.Values;
            }
            catch (Exception ex)
            {
                error = true;
                return HandleError(keys, ex);
            }
            finally
            {
                Interlocked.Decrement(ref _pendingRequestsCount);
                
                _onResult?.Invoke(new FunctionCacheGetResult<TK, TV>(
                    Name,
                    results.Values,
                    !error,
                    timestamp,
                    StopwatchHelper.GetDuration(stopwatchStart)));
            }
        }

        private Task<IList<FunctionCacheFetchResultInner<TK, TV>>> FetchOnDemand(IList<Key<TK>> keys)
        {
            return FetchImpl(keys.Select(k => new KeyToFetch(k)).ToArray(), FetchReason.OnDemand);
        }

        private void FetchEarly(IList<KeyToFetch> keys)
        {
            Task.Run(async () =>
            {
                try
                {
                    await FetchImpl(keys, FetchReason.EarlyFetch);
                }
                catch // Any exceptions that reach here will already have been handled
                { }
            });
        }

        private async Task<IList<FunctionCacheFetchResultInner<TK, TV>>> FetchImpl(IList<KeyToFetch> keys, FetchReason reason)
        {
            var timestamp = Timestamp.Now;
            var stopwatchStart = Stopwatch.GetTimestamp();
            var error = false;

            var results = new List<FunctionCacheFetchResultInner<TK, TV>>();
            
            var tcs = new TaskCompletionSource<FetchResults>();
            var toFetch = new Dictionary<TK, Key<TK>>();
            var alreadyPendingFetches = new List<KeyValuePair<Key<TK>, Task<FetchResults>>>();
            
            foreach (var key in keys)
            {
                var task = _activeFetches.GetOrAdd(key, k => tcs.Task);
                
                if (task == tcs.Task)
                    toFetch.Add(key.Key.AsObject, key.Key);
                else
                    alreadyPendingFetches.Add(new KeyValuePair<Key<TK>, Task<FetchResults>>(key, task));
            }

            var waitForPendingFetchesTask = alreadyPendingFetches.Any()
                ? WaitForPendingFetches(alreadyPendingFetches, stopwatchStart)
                : null;
            
            try
            {
                if (toFetch.Any())
                {
                    var fetched = await _func(toFetch.Keys);
    
                    tcs.SetResult(new FetchResults(fetched, Stopwatch.GetTimestamp()));
                    
                    if (fetched != null && fetched.Any())
                    {
                        var duration = StopwatchHelper.GetDuration(stopwatchStart);
    
                        var fetchedDictionary = fetched.ToDictionary(f => toFetch[f.Key], f => f.Value);
                        
                        results.AddRange(fetchedDictionary.Select(f => new FunctionCacheFetchResultInner<TK, TV>(f.Key, f.Value, true, false, duration)));
                        
                        if (_cache != null)
                        {
                            var setValueTask = _cache.Set(fetchedDictionary, _timeToLive);

                            if (!setValueTask.IsCompleted)
                                await setValueTask;
                        }
                    }
                }
                else
                {
                    tcs.SetCanceled();
                }

                if (waitForPendingFetchesTask != null)
                    results.AddRange(await waitForPendingFetchesTask);
            }
            catch (Exception ex)
            {
                tcs.TrySetException(ex);

                var duration = StopwatchHelper.GetDuration(stopwatchStart);

                var fetchedKeys = new HashSet<Key<TK>>(results.Select(r => r.Key), _keyComparer);
                
                results.AddRange(keys
                    .Select(k => k.Key)
                    .Except(fetchedKeys)
                    .Select(k => new FunctionCacheFetchResultInner<TK, TV>(k, default, false, false, duration)));
                
                var exception = new FunctionCacheFetchException<TK>(
                    Name,
                    keys.Select(k => (Key<TK>)k).ToArray(),
                    Timestamp.Now,
                    "Unable to fetch value(s)",
                    ex);
                
                _onError?.Invoke(exception);
                
                error = true;
                throw exception;
            }
            finally
            {
                foreach (var key in toFetch)
                    _activeFetches.TryRemove(key.Value, out _);
                
                if (_onFetch != null)
                {
                    var duration = StopwatchHelper.GetDuration(stopwatchStart);

                    _averageFetchDuration += (duration - _averageFetchDuration) / 10;

                    _onFetch(new FunctionCacheFetchResult<TK, TV>(
                        Name,
                        results,
                        !error,
                        timestamp,
                        StopwatchHelper.GetDuration(stopwatchStart),
                        reason));
                }
            }

            return results;
        }

        private async Task<IList<FunctionCacheFetchResultInner<TK, TV>>> WaitForPendingFetches(
            IList<KeyValuePair<Key<TK>, Task<FetchResults>>> fetches,
            long stopwatchStart)
        {
            await Task.WhenAll(fetches.Select(f => f.Value).Distinct());

            var results = new List<FunctionCacheFetchResultInner<TK, TV>>(fetches.Count);

            foreach (var fetch in fetches)
            {
                var result = fetch.Value.Result;
                
                if (!result.Results.TryGetValue(fetch.Key, out var value))
                    continue;
                
                results.Add(new FunctionCacheFetchResultInner<TK, TV>(
                    fetch.Key,
                    value,
                    true,
                    true,
                    StopwatchHelper.GetDuration(stopwatchStart, result.StopwatchTimestampCompleted)));
            }

            return results;
        }
        
        private IEnumerable<FunctionCacheGetResultInner<TK, TV>> HandleError(IList<Key<TK>> keys, Exception ex)
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
            
            _onError?.Invoke(exception);

            if (!_continueOnException)
                throw exception;
            
            var defaultValue = _defaultValueFactory == null
                ? default
                : _defaultValueFactory();
            
            foreach (var key in keys)
                yield return new FunctionCacheGetResultInner<TK, TV>(key, defaultValue, Outcome.Error, null);
        }

        private bool ShouldFetchEarly(TimeSpan timeToLive)
        {
            var random = _rng.NextDouble();

            return -Math.Log(random) * _averageFetchDuration > timeToLive.Ticks;
        }
        
        private readonly struct KeyToFetch
        {
            public Key<TK> Key { get; }
            public TimeSpan? TimeToLive { get; }

            public KeyToFetch(Key<TK> key, TimeSpan? timeToLive = null)
            {
                Key = key;
                TimeToLive = timeToLive;
            }

            public static implicit operator Key<TK>(KeyToFetch value)
            {
                return value.Key;
            }
        }
        
        private readonly struct FetchResults
        {
            public IDictionary<TK, TV> Results { get; }
            public long StopwatchTimestampCompleted { get; }

            public FetchResults(IDictionary<TK, TV> results, long stopwatchTimestampCompleted)
            {
                Results = results;
                StopwatchTimestampCompleted = stopwatchTimestampCompleted;
            }
        }
    }
}