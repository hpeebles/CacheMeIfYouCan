using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading.Tasks;

namespace CacheMeIfYouCan.Internal
{
    internal class FunctionCache<TK, TV>
    {
        private readonly Func<TK, Task<TV>> _func;
        private readonly string _cacheName;
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
        private long _averageFetchDuration;
        
        public FunctionCache(
            Func<TK, Task<TV>> func,
            string cacheName,
            ICache<TV> cache,
            TimeSpan timeToLive,
            Func<TK, string> keySerializer,
            bool earlyFetchEnabled,
            Func<TV> defaultValueFactory,
            Action<FunctionCacheGetResult<TK, TV>> onResult,
            Action<FunctionCacheFetchResult<TK, TV>> onFetch,
            Action<FunctionCacheErrorEvent<TK>> onError)
        {
            _func = func;
            _cacheName = cacheName;
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
        }

        public async Task<TV> Get(TK keyObj)
        {
            var start = Stopwatch.GetTimestamp();
            var result = new Result();
            var key = new Key(keyObj, _keySerializer(keyObj));
            
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

                    _onResult(new FunctionCacheGetResult<TK, TV>(_cacheName, key.AsObject, result.Value, key.AsString, result.Outcome, duration, result.CacheType));
                }
            }

            return result.Value;
        }

        private async Task<Result> GetImpl(Key key)
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
            
            return new Result(value, outcome, cacheType);
        }

        private async Task<TV> Fetch(Key key, TimeSpan? existingTtl = null)
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
                    _onError(new FunctionCacheErrorEvent<TK>("Unable to fetch value", key.AsObject, key.AsString, ex));

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

                    _onFetch(new FunctionCacheFetchResult<TK, TV>(_cacheName, key.AsObject, value, key.AsString, !error, duration, duplicate, existingTtl));
                }
            }

            return value;
        }
        
        private void TriggerEarlyFetch(Key key, TimeSpan timeToLive)
        {
            Task.Run(async () =>
            {
                try
                {
                    await Fetch(key, timeToLive);
                }
                catch // any exceptions that reach here will already have been handled
                { }
            });
        }

        private Result HandleError(Key key, Exception ex)
        {
            if (_onError != null)
            {
                var message = _continueOnException
                    ? "Unable to get value. Default value being returned"
                    : "Unable to get value";

                _onError(new FunctionCacheErrorEvent<TK>(message, key.AsObject, key.AsString, ex));
            }

            var defaultValue = _defaultValueFactory == null
                ? default(TV)
                : _defaultValueFactory();

            if (!_continueOnException)
                throw ex;
            
            return new Result(defaultValue, Outcome.Error, null);
        }

        private bool ShouldFetchEarly(TimeSpan timeToLive)
        {
            var random = _rng.NextDouble();

            return -Math.Log(random) * _averageFetchDuration > timeToLive.Ticks;
        }

        private struct Key
        {
            public readonly TK AsObject;
            public readonly string AsString;

            public Key(TK key, string keyString)
            {
                AsObject = key;
                AsString = keyString;
            }
        }

        private struct Result
        {
            public readonly TV Value;
            public readonly Outcome Outcome;
            public readonly string CacheType;

            public Result(TV value, Outcome outcome, string cacheType)
            {
                Value = value;
                Outcome = outcome;
                CacheType = cacheType;
            }
        }
    }
}