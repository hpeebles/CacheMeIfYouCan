using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CacheMeIfYouCan.Internal.DuplicateTaskCatcher;
using CacheMeIfYouCan.Notifications;

namespace CacheMeIfYouCan.Internal
{
    internal sealed class EnumerableKeyFunctionCache<TK, TV> : IPendingRequestsCounter, IDisposable
    {
        private readonly ICacheInternal<TK, TV> _cache;
        private readonly TimeSpan _timeToLive;
        private readonly Func<TK, string> _keySerializer;
        private readonly bool _earlyFetchEnabled;
        private readonly Func<TV> _defaultValueFactory;
        private readonly bool _continueOnException;
        private readonly Action<FunctionCacheGetResult<TK, TV>> _onResult;
        private readonly Action<FunctionCacheFetchResult<TK, TV>> _onFetch;
        private readonly Action<FunctionCacheException<TK>> _onException;
        private readonly KeyComparer<TK> _keyComparer;
        private readonly Func<IDictionary<TK, TV>> _dictionaryFactoryFunc;
        private readonly DuplicateTaskCatcherMulti<TK, TV> _fetchHandler;
        private readonly Random _rng;
        private int _pendingRequestsCount;
        private long _averageFetchDuration;
        private bool _disposed;
        
        public EnumerableKeyFunctionCache(
            Func<IEnumerable<TK>, Task<IDictionary<TK, TV>>> func,
            string functionName,
            ICacheInternal<TK, TV> cache,
            TimeSpan timeToLive,
            Func<TK, string> keySerializer,
            bool earlyFetchEnabled,
            Func<TV> defaultValueFactory,
            Action<FunctionCacheGetResult<TK, TV>> onResult,
            Action<FunctionCacheFetchResult<TK, TV>> onFetch,
            Action<FunctionCacheException<TK>> onException,
            KeyComparer<TK> keyComparer,
            Func<IDictionary<TK, TV>> dictionaryFactoryFunc)
        {
            Name = functionName;
            Type = GetType().Name;
            _cache = cache;
            _timeToLive = timeToLive;
            _keySerializer = keySerializer;
            _earlyFetchEnabled = earlyFetchEnabled;
            _defaultValueFactory = defaultValueFactory;
            _continueOnException = defaultValueFactory != null;
            _onResult = onResult;
            _onFetch = onFetch;
            _onException = onException;
            _keyComparer = keyComparer;
            _dictionaryFactoryFunc = dictionaryFactoryFunc;
            _fetchHandler = new DuplicateTaskCatcherMulti<TK, TV>(func, keyComparer);
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

                if (results == null)
                    return null;
                
                var dictionary = _dictionaryFactoryFunc();

                foreach (var result in results)
                    dictionary[result.Key.AsObject] = result.Value;

                return dictionary;
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
#if NET45
                    results.Values.ToArray(),
#else
                    results.Values,
#endif
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

            try
            {
                var fetched = await _fetchHandler.ExecuteAsync(keys.Select(k => k.Key.AsObject).ToArray());
                
                if (fetched != null && fetched.Any())
                {
                    var keysAndFetchedValues = new Dictionary<Key<TK>, DuplicateTaskCatcherMultiResult<TK, TV>>();
                    foreach (var key in keys.Select(k => k.Key))
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
                        var nonDuplicates = keysAndFetchedValues
                            .Where(kv => !kv.Value.Duplicate)
                            .ToDictionary(kv => kv.Key, kv => kv.Value.Value, _keyComparer);

                        if (nonDuplicates.Any())
                        {
                            var setValueTask = _cache.Set(nonDuplicates, _timeToLive);

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
                
                _onException?.Invoke(exception);
                
                error = true;
                throw exception;
            }
            finally
            {
                if (_onFetch != null)
                {
                    var duration = StopwatchHelper.GetDuration(stopwatchStart);

                    _averageFetchDuration += (duration.Ticks - _averageFetchDuration) / 10;

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

        private IList<FunctionCacheGetResultInner<TK, TV>> HandleError(IList<Key<TK>> keys, Exception ex)
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

        private bool ShouldFetchEarly(TimeSpan timeToLive)
        {
            var random = _rng.NextDouble();

            return -Math.Log(random) * _averageFetchDuration > timeToLive.Ticks;
        }
        
        private readonly struct KeyToFetch
        {
            public KeyToFetch(Key<TK> key, TimeSpan? timeToLive = null)
            {
                Key = key;
                TimeToLive = timeToLive;
            }

            public Key<TK> Key { get; }
            public TimeSpan? TimeToLive { get; }

            public static implicit operator Key<TK>(KeyToFetch value)
            {
                return value.Key;
            }
        }
    }
}