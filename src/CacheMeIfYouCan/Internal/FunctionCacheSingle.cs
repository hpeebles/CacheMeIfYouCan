using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using CacheMeIfYouCan.Notifications;

namespace CacheMeIfYouCan.Internal
{
    internal sealed class FunctionCacheSingle<TK, TV> : IPendingRequestsCounter, IDisposable
    {
        private readonly Func<TK, Task<TV>> _func;
        private readonly ICacheInternal<TK, TV> _cache;
        private readonly Func<TK, TV, TimeSpan> _timeToLiveFactory;
        private readonly Func<TK, string> _keySerializer;
        private readonly bool _earlyFetchEnabled;
        private readonly Func<TV> _defaultValueFactory;
        private readonly bool _continueOnException;
        private readonly Action<FunctionCacheGetResult<TK, TV>> _onResult;
        private readonly Action<FunctionCacheFetchResult<TK, TV>> _onFetch;
        private readonly Action<FunctionCacheException<TK>> _onException;
        private readonly ConcurrentDictionary<Key<TK>, Task<FetchResult>> _activeFetches;
        private readonly Random _rng;
        private int _pendingRequestsCount;
        private long _averageFetchDuration;
        private bool _disposed;
        
        public FunctionCacheSingle(
            Func<TK, Task<TV>> func,
            string functionName,
            ICacheInternal<TK, TV> cache,
            Func<TK, TV, TimeSpan> timeToLiveFactory,
            Func<TK, string> keySerializer,
            bool earlyFetchEnabled,
            Func<TV> defaultValueFactory,
            Action<FunctionCacheGetResult<TK, TV>> onResult,
            Action<FunctionCacheFetchResult<TK, TV>> onFetch,
            Action<FunctionCacheException<TK>> onException,
            IEqualityComparer<Key<TK>> keyComparer)
        {
            Name = functionName;
            Type = GetType().Name;
            _func = func;
            _cache = cache;
            _timeToLiveFactory = timeToLiveFactory;
            _keySerializer = keySerializer;
            _earlyFetchEnabled = earlyFetchEnabled;
            _defaultValueFactory = defaultValueFactory;
            _continueOnException = defaultValueFactory != null;
            _onResult = onResult;
            _onFetch = onFetch;
            _onException = onException;
            _activeFetches = new ConcurrentDictionary<Key<TK>, Task<FetchResult>>(keyComparer);
            _rng = new Random();
        }

        public string Name { get; }
        public string Type { get; }
        public int PendingRequestsCount => _pendingRequestsCount;

        public void Dispose()
        {
            PendingRequestsCounterContainer.Remove(this);
            _disposed = true;
        }
        
        public async Task<TV> Get(TK keyObj)
        {
            if (_disposed)
                throw new ObjectDisposedException($"{Name} - {Type}");
            
            using (SynchronizationContextRemover.StartNew())
            {
                var timestamp = Timestamp.Now;
                var stopwatchStart = Stopwatch.GetTimestamp();

                var key = new Key<TK>(keyObj, _keySerializer);
                var error = false;

                FunctionCacheGetResultInner<TK, TV> result = null;
                try
                {
                    Interlocked.Increment(ref _pendingRequestsCount);
                    
                    if (_cache != null)
                    {
                        var fromCacheTask = _cache.Get(key);

                        var fromCache = fromCacheTask.IsCompleted
                            ? fromCacheTask.Result
                            : await fromCacheTask;
                        
                        if (fromCache.Success)
                        {
                            result = new FunctionCacheGetResultInner<TK, TV>(
                                fromCache.Key,
                                fromCache.Value,
                                Outcome.FromCache,
                                fromCache.CacheType);

                            if (_earlyFetchEnabled && ShouldFetchEarly(fromCache.TimeToLive))
                                FetchEarly(key, fromCache.TimeToLive);
                        }
                    }

                    if (result == null)
                    {
                        var fetched = await FetchImpl(key, FetchReason.OnDemand);

                        if (fetched != null)
                        {
                            result = new FunctionCacheGetResultInner<TK, TV>(
                                fetched.Key,
                                fetched.Value,
                                Outcome.Fetch,
                                null);
                        }
                        else
                        {
                            throw new Exception($"No value returned. Key: '{key}'");
                        }
                    }
                }
                catch (Exception ex)
                {
                    error = true;
                    result = HandleError(key, ex);
                }
                finally
                {
                    Interlocked.Decrement(ref _pendingRequestsCount);
                    
                    _onResult?.Invoke(new FunctionCacheGetResult<TK, TV>(
                        Name,
                        new[] { result },
                        !error,
                        timestamp,
                        StopwatchHelper.GetDuration(stopwatchStart)));
                }

                return result.Value;
            }
        }

        private void FetchEarly(Key<TK> key, TimeSpan timeToLive)
        {
            Task.Run(async () =>
            {
                try
                {
                    await FetchImpl(key, FetchReason.EarlyFetch, timeToLive);
                }
                catch // Any exceptions that reach here will already have been handled
                { }
            });
        }

        private async Task<FunctionCacheFetchResultInner<TK, TV>> FetchImpl(Key<TK> key, FetchReason reason, TimeSpan? existingTimeToLive = null)
        {
            var timestamp = Timestamp.Now;
            var stopwatchStart = Stopwatch.GetTimestamp();
            var error = false;
            var tcs = new TaskCompletionSource<FetchResult>();
            
            var task = _activeFetches.GetOrAdd(key, k => tcs.Task);
            
            var fetchAlreadyPending = task != tcs.Task;

            FunctionCacheFetchResultInner<TK, TV> result = null;
            try
            {
                if (fetchAlreadyPending)
                {
                    var value = await task;

                    result = new FunctionCacheFetchResultInner<TK, TV>(
                        key,
                        value.Value,
                        true,
                        true,
                        StopwatchHelper.GetDuration(stopwatchStart, value.StopwatchTimestampCompleted));
                }
                else
                {
                    var fetched = await _func(key);

                    tcs.SetResult(new FetchResult(fetched, Stopwatch.GetTimestamp()));

                    var duration = StopwatchHelper.GetDuration(stopwatchStart);

                    if (_cache != null)
                    {
                        var setValueTask = _cache.Set(key, fetched, _timeToLiveFactory(key, fetched));

                        if (!setValueTask.IsCompleted)
                            await setValueTask;
                    }

                    result = new FunctionCacheFetchResultInner<TK, TV>(key, fetched, true, false, duration);
                }
            }
            catch (Exception ex)
            {
                tcs.TrySetException(ex);

                var duration = StopwatchHelper.GetDuration(stopwatchStart);
                
                result = new FunctionCacheFetchResultInner<TK, TV>(key, default, false, false, duration);
                
                var exception = new FunctionCacheFetchException<TK>(
                    Name,
                    new[] { key },
                    Timestamp.Now,
                    "Unable to fetch value",
                    ex);
                
                _onException?.Invoke(exception);
                
                error = true;
                throw exception;
            }
            finally
            {
                _activeFetches.TryRemove(key, out _);
                
                if (_onFetch != null)
                {
                    var duration = StopwatchHelper.GetDuration(stopwatchStart);

                    _averageFetchDuration += (duration.Ticks - _averageFetchDuration) / 10;

                    _onFetch(new FunctionCacheFetchResult<TK, TV>(
                        Name,
                        new[] { result },
                        !error,
                        timestamp,
                        StopwatchHelper.GetDuration(stopwatchStart),
                        reason));
                }
            }

            return result;
        }
        
        private FunctionCacheGetResultInner<TK, TV> HandleError(Key<TK> key, Exception ex)
        {
            var message = _continueOnException
                ? "Unable to get value. Default being returned"
                : "Unable to get value";

            var exception = new FunctionCacheGetException<TK>(
                Name,
                new[] { key },
                Timestamp.Now,
                message,
                ex);
            
            _onException?.Invoke(exception);

            if (!_continueOnException)
                throw exception;
            
            var defaultValue = _defaultValueFactory == null
                ? default
                : _defaultValueFactory();
            
            return new FunctionCacheGetResultInner<TK, TV>(key, defaultValue, Outcome.Error, null);
        }

        private bool ShouldFetchEarly(TimeSpan timeToLive)
        {
            var random = _rng.NextDouble();

            return -Math.Log(random) * _averageFetchDuration > timeToLive.Ticks;
        }
        
        private readonly struct FetchResult
        {
            public TV Value { get; }
            public long StopwatchTimestampCompleted { get; }

            public FetchResult(TV value, long stopwatchTimestampCompleted)
            {
                Value = value;
                StopwatchTimestampCompleted = stopwatchTimestampCompleted;
            }
        }
    }
}