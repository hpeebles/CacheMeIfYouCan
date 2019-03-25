using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using CacheMeIfYouCan.Internal.DuplicateTaskCatcher;
using CacheMeIfYouCan.Notifications;

namespace CacheMeIfYouCan.Internal.FunctionCaches
{
    internal sealed class SingleKeyFunctionCache<TK, TV> : IPendingRequestsCounter, IDisposable
    {
        private readonly ICacheInternal<TK, TV> _cache;
        private readonly Func<TK, TV, TimeSpan> _timeToLiveFactory;
        private readonly Func<TK, string> _keySerializer;
        private readonly Func<TV> _defaultValueFactory;
        private readonly bool _continueOnException;
        private readonly Action<FunctionCacheGetResult<TK, TV>> _onResult;
        private readonly Action<FunctionCacheFetchResult<TK, TV>> _onFetch;
        private readonly Action<FunctionCacheException<TK>> _onException;
        private readonly IDuplicateTaskCatcherSingle<TK, TV> _fetchHandler;
        private readonly Func<TK, bool> _skipCacheGetPredicate;
        private readonly Func<TK, bool> _skipCacheSetPredicate;
        private int _pendingRequestsCount;
        private bool _disposed;
        
        public SingleKeyFunctionCache(
            Func<TK, CancellationToken, Task<TV>> func,
            string functionName,
            ICacheInternal<TK, TV> cache,
            Func<TK, TV, TimeSpan> timeToLiveFactory,
            bool catchDuplicateRequests,
            Func<TK, string> keySerializer,
            Func<TV> defaultValueFactory,
            Action<FunctionCacheGetResult<TK, TV>> onResult,
            Action<FunctionCacheFetchResult<TK, TV>> onFetch,
            Action<FunctionCacheException<TK>> onException,
            KeyComparer<TK> keyComparer,
            Func<TK, bool> skipCacheGetPredicate,
            Func<TK, bool> skipCacheSetPredicate)
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
            
            if (catchDuplicateRequests)
                _fetchHandler = new DuplicateTaskCatcherSingle<TK, TV>(func, keyComparer);
            else
                _fetchHandler = new DisabledDuplicateTaskCatcherSingle<TK, TV>(func);
            
            _skipCacheGetPredicate = skipCacheGetPredicate;
            _skipCacheSetPredicate = skipCacheSetPredicate;
        }

        public string Name { get; }
        public string Type { get; }
        public int PendingRequestsCount => _pendingRequestsCount;

        public void Dispose()
        {
            PendingRequestsCounterContainer.Remove(this);
            _disposed = true;
        }
        
        public async Task<TV> Get(TK keyObj, CancellationToken token)
        {
            if (_disposed)
                throw new ObjectDisposedException($"{Name} - {Type}");
            
            using (SynchronizationContextRemover.StartNew())
            using (TraceHandlerInternal.Start())
            {
                var start = DateTime.UtcNow;
                var stopwatchStart = Stopwatch.GetTimestamp();

                var key = new Key<TK>(keyObj, _keySerializer);
                var error = false;

                FunctionCacheGetResultInner<TK, TV> result = null;
                try
                {
                    token.ThrowIfCancellationRequested();
                    
                    Interlocked.Increment(ref _pendingRequestsCount);

                    if (_cache != null && (_skipCacheGetPredicate == null || !_skipCacheGetPredicate(key)))
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
                        }
                    }

                    if (result == null)
                    {
                        var fetchResult = await Fetch(key, token);

                        result = new FunctionCacheGetResultInner<TK, TV>(
                            fetchResult.Key,
                            fetchResult.Value,
                            Outcome.Fetch,
                            null);
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

                    var notifyResult = _onResult != null || TraceHandlerInternal.Enabled;
                    if (notifyResult)
                    {
                        var functionCacheGetResult = new FunctionCacheGetResult<TK, TV>(
                            Name,
                            new[] { result },
                            !error,
                            start,
                            StopwatchHelper.GetDuration(stopwatchStart));
                        
                        _onResult?.Invoke(functionCacheGetResult);
                        TraceHandlerInternal.Mark(functionCacheGetResult);
                    }
                }

                return result.Value;
            }
        }

        private async Task<FunctionCacheFetchResultInner<TK, TV>> Fetch(Key<TK> key, CancellationToken token)
        {
            var start = DateTime.UtcNow;
            var stopwatchStart = Stopwatch.GetTimestamp();
            var error = false;

            FunctionCacheFetchResultInner<TK, TV> result = null;
            try
            {
                var (fetched, duplicate) = await _fetchHandler.ExecuteAsync(key, token);

                result = new FunctionCacheFetchResultInner<TK, TV>(
                    key,
                    fetched.Value,
                    true,
                    duplicate,
                    StopwatchHelper.GetDuration(stopwatchStart, fetched.StopwatchTimestampCompleted));

                if (_cache != null && !duplicate && (_skipCacheSetPredicate == null || !_skipCacheSetPredicate(key)))
                {
                    var setValueTask = _cache.Set(key, fetched.Value, _timeToLiveFactory(key, fetched.Value));

                    if (!setValueTask.IsCompleted)
                        await setValueTask;
                }
            }
            catch (Exception ex)
            {
                var duration = StopwatchHelper.GetDuration(stopwatchStart);
                
                result = new FunctionCacheFetchResultInner<TK, TV>(key, default, false, false, duration);
                
                var exception = new FunctionCacheFetchException<TK>(
                    Name,
                    new[] { key },
                    "Unable to fetch value",
                    ex);
                
                _onException?.Invoke(exception);
                TraceHandlerInternal.Mark(exception);
                
                error = true;
                throw exception;
            }
            finally
            {
                var notifyFetch = _onFetch != null || TraceHandlerInternal.Enabled;
                if (notifyFetch)
                {
                    var functionCacheFetchResult = new FunctionCacheFetchResult<TK, TV>(
                        Name,
                        new[] { result },
                        !error,
                        start,
                        StopwatchHelper.GetDuration(stopwatchStart));

                    _onFetch?.Invoke(functionCacheFetchResult);
                    TraceHandlerInternal.Mark(functionCacheFetchResult);
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
                message,
                ex);
            
            _onException?.Invoke(exception);
            TraceHandlerInternal.Mark(exception);

            if (!_continueOnException)
                throw exception;
            
            var defaultValue = _defaultValueFactory == null
                ? default
                : _defaultValueFactory();
            
            return new FunctionCacheGetResultInner<TK, TV>(key, defaultValue, Outcome.Error, null);
        }
    }
}