using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CacheMeIfYouCan.Notifications;

namespace CacheMeIfYouCan.Internal
{
    internal class LocalCacheNotificationWrapper<TK, TV> : ILocalCache<TK, TV> 
    {
        private readonly ILocalCache<TK, TV> _cache;
        private readonly Action<CacheGetResult<TK, TV>> _onCacheGetResult;
        private readonly Action<CacheSetResult<TK, TV>> _onCacheSetResult;
        private readonly Action<CacheException<TK>> _onError;

        public LocalCacheNotificationWrapper(
            ILocalCache<TK, TV> cache,
            Action<CacheGetResult<TK, TV>> onCacheGetResult,
            Action<CacheSetResult<TK, TV>> onCacheSetResult,
            Action<CacheException<TK>> onError)
        {
            _cache = cache;
            _onCacheGetResult = onCacheGetResult;
            _onCacheSetResult = onCacheSetResult;
            _onError = onError;

            CacheName = _cache.CacheName;
            CacheType = _cache.CacheType;
        }

        public string CacheName { get; }
        public string CacheType { get; }

        public GetFromCacheResult<TK, TV> Get(Key<TK> key)
        {
            var timestamp = Timestamp.Now;
            var stopwatchStart = Stopwatch.GetTimestamp();
            var error = false;
            var result = new GetFromCacheResult<TK, TV>();
            
            try
            {
                result = _cache.Get(key);
            }
            catch (Exception ex)
            {
                error = true;

                var exception = new CacheException<TK>(
                    CacheName,
                    CacheType,
                    new[] { key },
                    timestamp,
                    "LocalCache.Get exception",
                    ex);
                
                _onError?.Invoke(exception);

                throw exception;
            }
            finally
            {
                if (_onCacheGetResult != null)
                {
                    var duration = StopwatchHelper.GetDuration(stopwatchStart);

                    _onCacheGetResult(new CacheGetResult<TK, TV>(
                        CacheName,
                        CacheType,
                        result.Success ? new[] { result } : new GetFromCacheResult<TK, TV>[0],
                        result.Success ? new Key<TK>[0] : new[] { key },
                        !error,
                        timestamp,
                        duration));
                }
            }

            return result;
        }

        public void Set(Key<TK> key, TV value, TimeSpan timeToLive)
        {
            var timestamp = Timestamp.Now;
            var stopwatchStart = Stopwatch.GetTimestamp();
            var error = false;

            try
            {
                _cache.Set(key, value, timeToLive);
            }
            catch (Exception ex)
            {
                error = true;

                var exception = new CacheException<TK>(
                    CacheName,
                    CacheType,
                    new[] { key },
                    timestamp,
                    "LocalCache.Set exception",
                    ex);
                
                _onError?.Invoke(exception);

                throw exception;
            }
            finally
            {
                if (_onCacheSetResult != null)
                {
                    var duration = StopwatchHelper.GetDuration(stopwatchStart);

                    _onCacheSetResult(new CacheSetResult<TK, TV>(
                        CacheName,
                        CacheType,
                        new[] { new KeyValuePair<Key<TK>, TV>(key, value) },
                        !error,
                        timestamp,
                        duration));
                }
            }
        }
        
        public IList<GetFromCacheResult<TK, TV>> Get(ICollection<Key<TK>> keys)
        {
            var timestamp = Timestamp.Now;
            var stopwatchStart = Stopwatch.GetTimestamp();
            var error = false;
            IList<GetFromCacheResult<TK, TV>> results = null;
            
            try
            {
                results = _cache.Get(keys);
            }
            catch (Exception ex)
            {
                error = true;

                var exception = new CacheException<TK>(
                    CacheName,
                    CacheType,
                    keys,
                    timestamp,
                    "LocalCache.Get exception",
                    ex);
                
                _onError?.Invoke(exception);

                throw exception;
            }
            finally
            {
                if (_onCacheGetResult != null)
                {
                    var duration = StopwatchHelper.GetDuration(stopwatchStart);

                    var misses = results == null || !results.Any()
                        ? keys
                        : keys.Except(results.Select(r => r.Key)).ToArray();
                
                    _onCacheGetResult(new CacheGetResult<TK, TV>(
                        CacheName,
                        CacheType,
                        results,
                        misses,
                        !error,
                        timestamp,
                        duration));
                }
            }

            return results;
        }

        public void Set(ICollection<KeyValuePair<Key<TK>, TV>> values, TimeSpan timeToLive)
        {
            var timestamp = Timestamp.Now;
            var stopwatchStart = Stopwatch.GetTimestamp();
            var error = false;

            try
            {
                _cache.Set(values, timeToLive);
            }
            catch (Exception ex)
            {
                error = true;

                var exception = new CacheException<TK>(
                    CacheName,
                    CacheType,
                    values.Select(kv => kv.Key).ToArray(),
                    timestamp,
                    "LocalCache.Set exception",
                    ex);
                
                _onError?.Invoke(exception);

                throw exception;
            }
            finally
            {
                if (_onCacheSetResult != null)
                {
                    var duration = StopwatchHelper.GetDuration(stopwatchStart);

                    _onCacheSetResult(new CacheSetResult<TK, TV>(
                        CacheName,
                        CacheType,
                        values,
                        !error,
                        timestamp,
                        duration));
                }
            }
        }

        public void Remove(Key<TK> key)
        {
            _cache.Remove(key);
        }
    }
}