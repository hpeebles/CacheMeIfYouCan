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
        private readonly bool _preFetchEnabled;
        private readonly ILogger _logger;
        private readonly Func<T> _defaultValueFactory;
        private readonly bool _continueOnException;
        private readonly Action<FunctionCacheGetResult<T>> _onResult;
        private readonly Action<FunctionCacheFetchResult<T>> _onFetch;
        private readonly ConcurrentDictionary<string, Task<T>> _activeFetches;
        private readonly Random _rng;
        private long _averageFetchDuration;
        
        public FunctionCache(
            Func<string, Task<T>> func,
            ICache<T> cache,
            TimeSpan timeToLive,
            bool preFetchEnabled,
            ILogger logger,
            Func<T> defaultValueFactory,
            Action<FunctionCacheGetResult<T>> onResult,
            Action<FunctionCacheFetchResult<T>> onFetch)
        {
            _func = func;
            _cache = cache;
            _timeToLive = timeToLive;
            _preFetchEnabled = preFetchEnabled;
            _logger = logger;
            _defaultValueFactory = defaultValueFactory;
            _continueOnException = defaultValueFactory != null;
            _onResult = onResult;
            _onFetch = onFetch;
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
                if (_logger != null)
                    _logger.Error(ex, $"Unable to get value. Key: '{key}'");

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

                if (_preFetchEnabled && ShouldPreFetch(getFromCacheResult.TimeToLive))
                    Task.Run(() => Fetch(key));
            }
            else
            {
                value = await Fetch(key);
                outcome = Outcome.Fetch;
                cacheType = null;
            }
            
            return new Result(value, outcome, cacheType);
        }

        private bool ShouldPreFetch(TimeSpan timeToLive)
        {
            return false;
        }

        private async Task<T> Fetch(string key)
        {
            return await _activeFetches.GetOrAdd(
                key,
                async k =>
                {
                    var start = Stopwatch.GetTimestamp();
                    
                    var value = await _func(k);

                    await _cache.Set(k, value, _timeToLive);

                    _activeFetches.TryRemove(k, out _);

                    var duration = Stopwatch.GetTimestamp() - start;

                    if (_onFetch != null)
                        _onFetch(new FunctionCacheFetchResult<T>(k, value, true, duration));
                    
                    _averageFetchDuration += (duration - _averageFetchDuration) / 10;
                    
                    return value;
                });
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