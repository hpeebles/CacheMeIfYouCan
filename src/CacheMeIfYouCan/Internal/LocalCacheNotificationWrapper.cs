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

        public LocalCacheNotificationWrapper(
            ILocalCache<TK, TV> cache,
            Action<CacheGetResult<TK, TV>> onCacheGetResult,
            Action<CacheSetResult<TK, TV>> onCacheSetResult)
        {
            _cache = cache;
            _onCacheGetResult = onCacheGetResult;
            _onCacheSetResult = onCacheSetResult;

            CacheName = _cache.CacheName;
            CacheType = _cache.CacheType;
        }

        public string CacheName { get; }
        public string CacheType { get; }

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
            catch
            {
                error = true;
                throw;
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
            catch
            {
                error = true;
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