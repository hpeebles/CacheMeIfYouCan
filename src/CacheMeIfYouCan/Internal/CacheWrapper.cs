using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using CacheMeIfYouCan.Notifications;

namespace CacheMeIfYouCan.Internal
{
    internal class CacheWrapper<TK, TV> : ICache<TK, TV> 
    {
        private readonly ICache<TK, TV> _cache;
        private readonly Action<CacheGetResult<TK, TV>> _onCacheGetResult;
        private readonly Action<CacheSetResult<TK, TV>> _onCacheSetResult;
        private readonly Action<CacheErrorEvent<TK>> _onError;

        public CacheWrapper(
            ICache<TK, TV> cache,
            Action<CacheGetResult<TK, TV>> onCacheGetResult,
            Action<CacheSetResult<TK, TV>> onCacheSetResult,
            Action<CacheErrorEvent<TK>> onError)
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

        public async Task<IList<GetFromCacheResult<TK, TV>>> Get(ICollection<Key<TK>> keys)
        {
            var timestamp = Timestamp.Now;
            var stopwatchStart = Stopwatch.GetTimestamp();
            var error = false;
            IList<GetFromCacheResult<TK, TV>> results = null;
            
            try
            {
                results = await _cache.Get(keys);
            }
            catch (Exception ex)
            {
                error = true;

                _onError?.Invoke(new CacheErrorEvent<TK>(
                    CacheName,
                    CacheType,
                    keys,
                    timestamp,
                    "Cache.Get exception",
                    ex));

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

        public async Task Set(ICollection<KeyValuePair<Key<TK>, TV>> values, TimeSpan timeToLive)
        {
            var timestamp = Timestamp.Now;
            var stopwatchStart = Stopwatch.GetTimestamp();
            var error = false;

            try
            {
                await _cache.Set(values, timeToLive);
            }
            catch (Exception ex)
            {
                error = true;

                _onError?.Invoke(new CacheErrorEvent<TK>(
                    CacheName,
                    CacheType,
                    values.Select(kv => kv.Key).ToArray(),
                    timestamp,
                    "Cache.Set exception",
                    ex));

                throw;
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
    }
}