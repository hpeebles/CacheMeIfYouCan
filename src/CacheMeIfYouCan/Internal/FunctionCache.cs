using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading.Tasks;

namespace CacheMeIfYouCan.Internal
{
    internal class FunctionCache<T>
    {
        private readonly ICache<T> _cache;
        private readonly Func<string, Task<T>> _func;
        private readonly TimeSpan _timeToLive;
        private readonly bool _earlyFetchEnabled;
        private readonly Func<T> _defaultValueFactory;
        private readonly bool _continueOnException;
        private readonly Action<FunctionCacheGetResult<T>> _onResult;
        private readonly Action<FunctionCacheFetchResult<T>> _onFetch;
        private readonly Action<FunctionCacheErrorEvent> _onError;
        private readonly ConcurrentDictionary<string, Task<T>> _activeFetches;
        private readonly Random _rng;
        private long _averageFetchDuration;
        
        public FunctionCache(
            Func<string, Task<T>> func,
            ICache<T> cache,
            TimeSpan timeToLive,
            bool earlyFetchEnabled,
            Func<T> defaultValueFactory,
            Action<FunctionCacheGetResult<T>> onResult,
            Action<FunctionCacheFetchResult<T>> onFetch,
            Action<FunctionCacheErrorEvent> onError)
        {
            _func = func;
            _cache = cache;
            _timeToLive = timeToLive;
            _earlyFetchEnabled = earlyFetchEnabled;
            _defaultValueFactory = defaultValueFactory;
            _continueOnException = defaultValueFactory != null;
            _onResult = onResult;
            _onFetch = onFetch;
            _onError = onError;
            _activeFetches = new ConcurrentDictionary<string, Task<T>>();
            _rng = new Random();
        }

        public async Task<T> Get(string key)
        {
            var start = Stopwatch.GetTimestamp();
            var result = new Result();
            try
            {
                result = await GetImpl(key);
                
                return result.Value;
            }
            catch (Exception ex)
            {
                if (_onError != null)
                {
                    var message = _continueOnException
                        ? "Unable to get value. Default value being returned"
                        : "Unable to get value";

                    _onError(new FunctionCacheErrorEvent(message, key, ex));
                }

                var defaultValue = _defaultValueFactory == null
                    ? default(T)
                    : _defaultValueFactory();

                result = new Result(defaultValue, Outcome.Error, null);

                if (_continueOnException)
                    return defaultValue;

                throw;
            }
            finally
            {
                if (_onResult != null)
                {
                    var duration = Stopwatch.GetTimestamp() - start;

                    _onResult(new FunctionCacheGetResult<T>(key, result.Value, result.Outcome, duration, result.CacheType));
                }
            }
        }

        private async Task<Result> GetImpl(string key)
        {
            T value;
            Outcome outcome;
            string cacheType;

            var getFromCacheResult = await _cache.Get(key);
            
            if (getFromCacheResult.Success)
            {
                value = getFromCacheResult.Value;
                outcome = Outcome.FromCache;
                cacheType = getFromCacheResult.CacheType;

                if (_earlyFetchEnabled && ShouldFetchEarly(getFromCacheResult.TimeToLive))
                    Task.Run(() => Fetch(key, getFromCacheResult.TimeToLive));
            }
            else
            {
                value = await Fetch(key);
                outcome = Outcome.Fetch;
                cacheType = null;
            }
            
            return new Result(value, outcome, cacheType);
        }

        private async Task<T> Fetch(string key, TimeSpan? existingTtl = null)
        {
            var start = Stopwatch.GetTimestamp();
            var value = default(T);
            var duplicate = true;
            var error = false;
            
            try
            {
                value = await _activeFetches.GetOrAdd(
                    key,
                    async k =>
                    {
                        duplicate = false;
                        
                        var fetchedValue = await _func(k);

                        await _cache.Set(k, fetchedValue, _timeToLive);

                        _activeFetches.TryRemove(k, out _);

                        return fetchedValue;
                    });
            }
            catch (Exception ex)
            {
                if (_onError != null)
                    _onError(new FunctionCacheErrorEvent("Unable to fetch value", key, ex));

                duplicate = false;
                error = true;
                throw;
            }
            finally
            {
                var duration = Stopwatch.GetTimestamp() - start;

                _averageFetchDuration += (duration - _averageFetchDuration) / 10;
                
                if (_onFetch != null)
                    _onFetch(new FunctionCacheFetchResult<T>(key, value, !error, duration, duplicate, existingTtl));
            }

            return value;
        }

        private bool ShouldFetchEarly(TimeSpan timeToLive)
        {
            var random = _rng.NextDouble();

            return -Math.Log(random) * _averageFetchDuration > timeToLive.Ticks;
        }

        private struct Result
        {
            public readonly T Value;
            public readonly Outcome Outcome;
            public readonly string CacheType;

            public Result(T value, Outcome outcome, string cacheType)
            {
                Value = value;
                Outcome = outcome;
                CacheType = cacheType;
            }
        }
    }
}