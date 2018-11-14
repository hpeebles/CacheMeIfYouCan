using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using CacheMeIfYouCan.Caches;
using CacheMeIfYouCan.Notifications;

namespace CacheMeIfYouCan.Internal
{
    internal class FunctionCache<TK, TV>
    {
        private readonly Func<IEnumerable<TK>, Task<IDictionary<TK, TV>>> _func;
        private readonly string _functionName;
        private readonly ICache<TK, TV> _cache;
        private readonly TimeSpan _timeToLive;
        private readonly Func<TK, string> _keySerializer;
        private readonly bool _earlyFetchEnabled;
        private readonly Func<TV> _defaultValueFactory;
        private readonly bool _continueOnException;
        private readonly Action<FunctionCacheGetResult<TK, TV>> _onResult;
        private readonly Action<FunctionCacheFetchResult<TK, TV>> _onFetch;
        private readonly Action<FunctionCacheErrorEvent<TK>> _onError;
        private readonly IEqualityComparer<Key<TK>> _keyComparer;
        private readonly bool _multiKey;
        private readonly ConcurrentDictionary<Key<TK>, Task<FetchResults>> _activeFetches;
        private readonly Random _rng;
        private long _averageFetchDuration;
        
        public FunctionCache(
            Func<IEnumerable<TK>, Task<IDictionary<TK, TV>>> func,
            string functionName,
            ICache<TK, TV> cache,
            TimeSpan timeToLive,
            Func<TK, string> keySerializer,
            bool earlyFetchEnabled,
            Func<TV> defaultValueFactory,
            Action<FunctionCacheGetResult<TK, TV>> onResult,
            Action<FunctionCacheFetchResult<TK, TV>> onFetch,
            Action<FunctionCacheErrorEvent<TK>> onError,
            IEqualityComparer<Key<TK>> keyComparer,
            bool multiKey,
            Func<Task<IList<TK>>> keysToKeepAliveFunc)
        {
            _func = func;
            _functionName = functionName;
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
            _multiKey = multiKey;
            _activeFetches = new ConcurrentDictionary<Key<TK>, Task<FetchResults>>(keyComparer);
            _rng = new Random();

            if (_cache != null && keysToKeepAliveFunc != null)
            {
                async Task<TimeSpan?> GetTimeToLive(Key<TK> key)
                {
                    var results = await _cache.Get(new[] { key });

                    return results.Any() ? (TimeSpan?)results.Single().TimeToLive : null;
                }

                Task RefreshKey(TK key, TimeSpan? existingTimeToLive)
                {
                    var keyToFetch = new KeyToFetch(new Key<TK>(key, new Lazy<string>(() => _keySerializer(key))), existingTimeToLive);
                    
                    return FetchImpl(new[] { keyToFetch }, FetchReason.KeysToKeepAliveFunc);
                }

                var keysToKeepAliveProcessor = new KeysToKeepAliveProcessor<TK>(
                    _timeToLive,
                    GetTimeToLive,
                    RefreshKey,
                    keysToKeepAliveFunc,
                    _keySerializer,
                    keyComparer);

                Task.Run(keysToKeepAliveProcessor.Run);
            }
        }

        public async Task<TV> GetSingle(TK keyObj)
        {
            using (SynchronizationContextRemover.StartNew())
            {
                var result = await GetImpl(new[] { keyObj });
    
                return result == null || result.Count != 1
                    ? default(TV)
                    : result.First().Value;
            }
        }

        public async Task<IDictionary<TK, TV>> GetMulti(IEnumerable<TK> keyObjs)
        {
            using (SynchronizationContextRemover.StartNew())
            {
                var results = await GetImpl(keyObjs.ToArray());

                return results?.ToDictionary(kv => kv.Key.AsObject, kv => kv.Value);
            }
        }

        private async Task<ICollection<FunctionCacheGetResultInner<TK, TV>>>     GetImpl(IList<TK> keyObjs)
        {
            var timestamp = Timestamp.Now;
            var stopwatchStart = Stopwatch.GetTimestamp();
            
            var results = new Dictionary<Key<TK>, FunctionCacheGetResultInner<TK, TV>>(keyObjs.Count, _keyComparer);
            
            var keys = keyObjs
                .Select(k => new Key<TK>(k, new Lazy<string>(() => _keySerializer(k))))
                .ToArray();

            var error = false;
            
            try
            {
                IList<Key<TK>> missingKeys = null;
                if (_cache != null)
                {
                    var fromCache = await _cache.Get(keys);
                    
                    if (fromCache != null && fromCache.Any())
                    {
                        foreach (var result in fromCache)
                        {
                            results[result.Key] = new FunctionCacheGetResultInner<TK, TV>(
                                result.Key,
                                result.Value,
                                Outcome.FromCache, result.CacheType);
                        }

                        missingKeys = keys
                            .Except(results.Select(r => r.Key), _keyComparer)
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
            }
            catch (Exception ex)
            {
                error = true;
                results = HandleError(keys, ex);
            }
            finally
            {
                if (_onResult != null)
                {
                    _onResult(new FunctionCacheGetResult<TK, TV>(
                        _functionName,
                        results.Values,
                        !error,
                        timestamp,
                        StopwatchHelper.GetDuration(stopwatchStart)));
                }
            }

            return results.Values;
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
            var toFetch = new List<Key<TK>>();
            var alreadyPendingFetches = new List<KeyValuePair<Key<TK>, Task<FetchResults>>>();
            
            foreach (var key in keys)
            {
                var task = _activeFetches.GetOrAdd(key, k => tcs.Task);
                
                if (task == tcs.Task)
                    toFetch.Add(key);
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
                    var fetched = await _func(toFetch.Select(k => (TK)k).ToArray());
    
                    tcs.SetResult(new FetchResults(fetched, Stopwatch.GetTimestamp()));
                    
                    if (fetched != null && fetched.Any())
                    {
                        var duration = StopwatchHelper.GetDuration(stopwatchStart);
    
                        // If this is not a multi key function it is not guaranteed that we can use TK as a key in
                        // a dictionary so rather than find the Key<TK> from the TK just use the single value
                        IDictionary<Key<TK>, TV> fetchedDictionary;
                        if (_multiKey)
                        {
                            var keysDictionary = toFetch.ToDictionary(k => k.AsObject);
    
                            fetchedDictionary = fetched.ToDictionary(f => keysDictionary[f.Key], f => f.Value);
                        }
                        else
                        {
                            fetchedDictionary = new Dictionary<Key<TK>, TV> { { toFetch.Single(), fetched.Single().Value } };
                        }
                        
                        results.AddRange(fetchedDictionary.Select(f => new FunctionCacheFetchResultInner<TK, TV>(f.Key, f.Value, true, false, duration)));
                        
                        if (_cache != null)
                            await _cache.Set(fetchedDictionary, _timeToLive);
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
                    .Select(k => new FunctionCacheFetchResultInner<TK, TV>(k, default(TV), false, false, duration)));
                
                if (_onError != null)
                {
                    _onError(new FunctionCacheErrorEvent<TK>(
                        _functionName,
                        keys.Select(k => (Key<TK>)k).ToArray(),
                        Timestamp.Now,
                        "Unable to fetch value(s)",
                        ex));
                }

                error = true;
                throw;
            }
            finally
            {
                foreach (var key in toFetch)
                    _activeFetches.TryRemove(key, out _);
                
                if (_onFetch != null)
                {
                    var duration = StopwatchHelper.GetDuration(stopwatchStart);

                    _averageFetchDuration += (duration - _averageFetchDuration) / 10;

                    _onFetch(new FunctionCacheFetchResult<TK, TV>(
                        _functionName,
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

            if (!_multiKey)
            {
                var fetch = fetches.Single();
                var result = fetch.Value.Result;
                
                return new[]
                {
                    new FunctionCacheFetchResultInner<TK, TV>(
                        fetch.Key,
                        result.Results.Values.Single(),
                        true,
                        true,
                        StopwatchHelper.GetDuration(stopwatchStart, result.StopwatchTimestampCompleted))
                };
            }
            
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
        
        private Dictionary<Key<TK>, FunctionCacheGetResultInner<TK, TV>> HandleError(IList<Key<TK>> keys, Exception ex)
        {
            if (_onError != null)
            {
                var message = _continueOnException
                    ? "Unable to get value(s). Default being returned"
                    : "Unable to get value(s)";

                _onError(new FunctionCacheErrorEvent<TK>(
                    _functionName,
                    keys,
                    Timestamp.Now,
                    message,
                    ex));
            }

            if (!_continueOnException)
                throw ex;
            
            var defaultValue = _defaultValueFactory == null
                ? default(TV)
                : _defaultValueFactory();
            
            return keys.ToDictionary(
                k => k,
                k => new FunctionCacheGetResultInner<TK, TV>(k, defaultValue, Outcome.Error, null), _keyComparer);
        }

        private bool ShouldFetchEarly(TimeSpan timeToLive)
        {
            var random = _rng.NextDouble();

            return -Math.Log(random) * _averageFetchDuration > timeToLive.Ticks;
        }
        
        private struct KeyToFetch
        {
            public readonly Key<TK> Key;
            public readonly TimeSpan? TimeToLive;

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
        
        private struct FetchResults
        {
            public readonly IDictionary<TK, TV> Results;
            public readonly long StopwatchTimestampCompleted;

            public FetchResults(IDictionary<TK, TV> results, long stopwatchTimestampCompleted)
            {
                Results = results;
                StopwatchTimestampCompleted = stopwatchTimestampCompleted;
            }
        }
    }
}