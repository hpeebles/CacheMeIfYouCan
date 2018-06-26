using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace CacheMeIfYouCan.Internal
{
    internal class FunctionCache<TK, TV>
    {
        private readonly Func<TK, Task<TV>> _func;
        private readonly string _interfaceName;
        private readonly string _functionName;
        private readonly ICache<TV> _cache;
        private readonly TimeSpan _timeToLive;
        private readonly Func<TK, string> _keySerializer;
        private readonly bool _earlyFetchEnabled;
        private readonly Func<TV> _defaultValueFactory;
        private readonly bool _continueOnException;
        private readonly Action<FunctionCacheGetResult<TK, TV>> _onResult;
        private readonly Action<FunctionCacheFetchResult<TK, TV>> _onFetch;
        private readonly Action<FunctionCacheErrorEvent<TK>> _onError;
        private readonly ConcurrentDictionary<string, Task<TV>> _activeFetches;
        private readonly Random _rng;
        private readonly KeyRenewer<TK> _keyRenewer;
        private long _averageFetchDuration;
        
        public FunctionCache(
            Func<TK, Task<TV>> func,
            string interfaceName,
            string functionName,
            ICache<TV> cache,
            TimeSpan timeToLive,
            Func<TK, string> keySerializer,
            bool earlyFetchEnabled,
            Func<TV> defaultValueFactory,
            Action<FunctionCacheGetResult<TK, TV>> onResult,
            Action<FunctionCacheFetchResult<TK, TV>> onFetch,
            Action<FunctionCacheErrorEvent<TK>> onError,
            Func<Task<IList<TK>>> keysToKeepAliveFunc)
        {
            _func = func;
            _interfaceName = interfaceName;
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
            _activeFetches = new ConcurrentDictionary<string, Task<TV>>();
            _rng = new Random();

            if (keysToKeepAliveFunc != null)
            {
                async Task<TimeSpan?> GetTimeToLive(string key)
                {
                    var result = await _cache.Get(key);

                    return result.Success ? (TimeSpan?) result.TimeToLive : null;
                }

                Task RefreshKey(TK key, TimeSpan? existingTimeToLive)
                {
                    return Fetch(new Key<TK>(key, _keySerializer(key)), existingTimeToLive);
                }

                _keyRenewer = new KeyRenewer<TK>(
                    _timeToLive,
                    GetTimeToLive,
                    RefreshKey,
                    keysToKeepAliveFunc,
                    _keySerializer);

                Task.Run(_keyRenewer.Run);
            }
        }

        public async Task<TV> Get(TK keyObj)
        {
            var start = Stopwatch.GetTimestamp();
            var result = new Result<TV>();
            var key = new Key<TK>(keyObj, _keySerializer(keyObj));
            
            try
            {
                result = await GetImpl(key);
            }
            catch (Exception ex)
            {
                result = HandleError(key, ex);
            }
            finally
            {
                if (_onResult != null)
                {
                    var duration = Stopwatch.GetTimestamp() - start;

                    _onResult(new FunctionCacheGetResult<TK, TV>(
                        _interfaceName,
                        _functionName,
                        key.AsObject,
                        result.Value,
                        key.AsString,
                        result.Outcome,
                        duration,
                        result.CacheType));
                }
            }

            return result.Value;
        }

        private async Task<Result<TV>> GetImpl(Key<TK> key)
        {
            TV value;
            Outcome outcome;
            string cacheType;

            var getFromCacheResult = await _cache.Get(key.AsString);
            
            if (getFromCacheResult.Success)
            {
                value = getFromCacheResult.Value;
                outcome = Outcome.FromCache;
                cacheType = getFromCacheResult.CacheType;

                if (_earlyFetchEnabled && ShouldFetchEarly(getFromCacheResult.TimeToLive))
                    TriggerEarlyFetch(key, getFromCacheResult.TimeToLive);
            }
            else
            {
                value = await Fetch(key);
                outcome = Outcome.Fetch;
                cacheType = null;
            }
            
            return new Result<TV>(value, outcome, cacheType);
        }

        private async Task<TV> Fetch(Key<TK> key, TimeSpan? existingTtl = null)
        {
            var start = Stopwatch.GetTimestamp();
            var value = default(TV);
            var duplicate = true;
            var error = false;
            
            try
            {
                value = await _activeFetches.GetOrAdd(
                    key.AsString,
                    async k =>
                    {
                        duplicate = false;
                        
                        var fetchedValue = await _func(key.AsObject);

                        await _cache.Set(k, fetchedValue, _timeToLive);

                        return fetchedValue;
                    });
            }
            catch (Exception ex)
            {
                if (_onError != null)
                {
                    _onError(new FunctionCacheErrorEvent<TK>(
                        _interfaceName,
                        _functionName,
                        key.AsObject,
                        key.AsString,
                        "Unable to fetch value",
                        ex));
                }

                duplicate = false;
                error = true;
                throw;
            }
            finally
            {
                _activeFetches.TryRemove(key.AsString, out _);
                
                if (_onFetch != null)
                {
                    var duration = Stopwatch.GetTimestamp() - start;

                    _averageFetchDuration += (duration - _averageFetchDuration) / 10;

                    _onFetch(new FunctionCacheFetchResult<TK, TV>(
                        _interfaceName,
                        _functionName,
                        key.AsObject,
                        value,
                        key.AsString,
                        !error,
                        duration,
                        duplicate,
                        existingTtl));
                }
            }

            return value;
        }
        
        private void TriggerEarlyFetch(Key<TK> key, TimeSpan timeToLive)
        {
            Task.Run(async () =>
            {
                try
                {
                    await Fetch(key, timeToLive);
                }
                catch // Any exceptions that reach here will already have been handled
                { }
            });
        }

        private Result<TV> HandleError(Key<TK> key, Exception ex)
        {
            if (_onError != null)
            {
                var message = _continueOnException
                    ? "Unable to get value. Default value being returned"
                    : "Unable to get value";

                _onError(new FunctionCacheErrorEvent<TK>(
                    _interfaceName,
                    _functionName,
                    key.AsObject,
                    key.AsString,
                    message,
                    ex));
            }

            var defaultValue = _defaultValueFactory == null
                ? default(TV)
                : _defaultValueFactory();

            if (!_continueOnException)
                throw ex;
            
            return new Result<TV>(defaultValue, Outcome.Error, null);
        }

        private bool ShouldFetchEarly(TimeSpan timeToLive)
        {
            var random = _rng.NextDouble();

            return -Math.Log(random) * _averageFetchDuration > timeToLive.Ticks;
        }
    }
}