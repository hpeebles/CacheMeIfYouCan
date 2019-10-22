using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using CacheMeIfYouCan.Notifications;

namespace CacheMeIfYouCan.Internal.DistributedCache
{
    internal class DistributedCacheNotificationWrapper<TK, TV> : IDistributedCache<TK, TV> 
    {
        private readonly IDistributedCache<TK, TV> _cache;
        private readonly Action<CacheGetResult<TK, TV>> _onCacheGetResult;
        private readonly Action<CacheSetResult<TK, TV>> _onCacheSetResult;
        private readonly Action<CacheRemoveResult<TK>> _onCacheRemoveResult;
        private readonly Action<CacheException<TK>> _onException;
        private readonly IEqualityComparer<Key<TK>> _keyComparer;

        public DistributedCacheNotificationWrapper(
            IDistributedCache<TK, TV> cache,
            Action<CacheGetResult<TK, TV>> onCacheGetResult,
            Action<CacheSetResult<TK, TV>> onCacheSetResult,
            Action<CacheRemoveResult<TK>> onCacheRemoveResult,
            Action<CacheException<TK>> onException,
            IEqualityComparer<Key<TK>> keyComparer)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _onCacheGetResult = onCacheGetResult;
            _onCacheSetResult = onCacheSetResult;
            _onCacheRemoveResult = onCacheRemoveResult;
            _onException = onException;
            _keyComparer = keyComparer;

            CacheName = _cache.CacheName;
            CacheType = _cache.CacheType;
        }

        public string CacheName { get; }
        public string CacheType { get; }
        
        public void Dispose() => _cache.Dispose();
        
        public async Task<GetFromCacheResult<TK, TV>> Get(Key<TK> key)
        {
            var start = DateTime.UtcNow;
            var stopwatchStart = Stopwatch.GetTimestamp();
            var error = false;
            var result = new GetFromCacheResult<TK, TV>();

            try
            {
                result = await _cache.Get(key);
            }
            catch (CacheException<TK> ex)
            {
                error = true;
                _onException?.Invoke(ex);
                TraceHandlerInternal.Mark(ex);

                throw;
            }
            finally
            {
                var notifyResult = _onCacheGetResult != null || TraceHandlerInternal.Enabled;
                if (notifyResult)
                {
                    var cacheGetResult = new CacheGetResult<TK, TV>(
                        CacheName,
                        CacheType,
                        result.Success ? new[] { result } : new GetFromCacheResult<TK, TV>[0],
                        result.Success ? new Key<TK>[0] : new[] { key },
                        !error,
                        start,
                        StopwatchHelper.GetDuration(stopwatchStart));
                    
                    _onCacheGetResult?.Invoke(cacheGetResult);
                    TraceHandlerInternal.Mark(cacheGetResult);
                }
            }

            return result;
        }

        public async Task Set(Key<TK> key, TV value, TimeSpan timeToLive)
        {
            var start = DateTime.UtcNow;
            var stopwatchStart = Stopwatch.GetTimestamp();
            var error = false;

            try
            {
                await _cache.Set(key, value, timeToLive);
            }
            catch (CacheException<TK> ex)
            {
                error = true;
                _onException?.Invoke(ex);
                TraceHandlerInternal.Mark(ex);

                throw;
            }
            finally
            {
                var notifyResult = _onCacheSetResult != null || TraceHandlerInternal.Enabled;
                if (notifyResult)
                {
                    var cacheSetResult = new CacheSetResult<TK, TV>(
                        CacheName,
                        CacheType,
                        new[] { new KeyValuePair<Key<TK>, TV>(key, value) },
                        !error,
                        start,
                        StopwatchHelper.GetDuration(stopwatchStart));
                    
                    _onCacheSetResult?.Invoke(cacheSetResult);
                    TraceHandlerInternal.Mark(cacheSetResult);
                }
            }
        }

        public async Task<IList<GetFromCacheResult<TK, TV>>> Get(ICollection<Key<TK>> keys)
        {
            var start = DateTime.UtcNow;
            var stopwatchStart = Stopwatch.GetTimestamp();
            var error = false;
            IList<GetFromCacheResult<TK, TV>> results = null;
            
            try
            {
                results = await _cache.Get(keys);
            }
            catch (CacheException<TK> ex)
            {
                error = true;
                _onException?.Invoke(ex);
                TraceHandlerInternal.Mark(ex);

                throw;
            }
            finally
            {
                var notifyResult = _onCacheGetResult != null || TraceHandlerInternal.Enabled;
                if (notifyResult)
                {
                    var cacheGetResult = new CacheGetResult<TK, TV>(
                        CacheName,
                        CacheType,
                        results,
                        results == null || !results.Any()
                            ? keys
                            : keys.Except(results.Select(r => r.Key), _keyComparer).ToList(),
                        !error,
                        start,
                        StopwatchHelper.GetDuration(stopwatchStart));
                    
                    _onCacheGetResult?.Invoke(cacheGetResult);
                    TraceHandlerInternal.Mark(cacheGetResult);
                }
            }

            return results;
        }

        public async Task Set(ICollection<KeyValuePair<Key<TK>, TV>> values, TimeSpan timeToLive)
        {
            var start = DateTime.UtcNow;
            var stopwatchStart = Stopwatch.GetTimestamp();
            var error = false;

            try
            {
                await _cache.Set(values, timeToLive);
            }
            catch (CacheException<TK> ex)
            {
                error = true;
                _onException?.Invoke(ex);
                TraceHandlerInternal.Mark(ex);

                throw;
            }
            finally
            {
                var notifyResult = _onCacheSetResult != null || TraceHandlerInternal.Enabled;
                if (notifyResult)
                {
                    var cacheSetResult = new CacheSetResult<TK, TV>(
                        CacheName,
                        CacheType,
                        values,
                        !error,
                        start,
                        StopwatchHelper.GetDuration(stopwatchStart));

                    _onCacheSetResult?.Invoke(cacheSetResult);
                    TraceHandlerInternal.Mark(cacheSetResult);
                }
            }
        }

        public async Task<bool> Remove(Key<TK> key)
        {
            var start = DateTime.UtcNow;
            var stopwatchStart = Stopwatch.GetTimestamp();
            var error = false;
            var keyRemoved = false;

            try
            {
                keyRemoved = await _cache.Remove(key);

                return keyRemoved;
            }
            catch (CacheException<TK> ex)
            {
                error = true;
                _onException?.Invoke(ex);
                TraceHandlerInternal.Mark(ex);

                throw;
            }
            finally
            {
                var notifyResult = _onCacheRemoveResult != null || TraceHandlerInternal.Enabled;
                if (notifyResult)
                {
                    var cacheRemoveResult = new CacheRemoveResult<TK>(
                        CacheName,
                        CacheType,
                        key,
                        !error,
                        keyRemoved,
                        start,
                        StopwatchHelper.GetDuration(stopwatchStart));
                    
                    _onCacheRemoveResult?.Invoke(cacheRemoveResult);
                    TraceHandlerInternal.Mark(cacheRemoveResult);
                }
            }
        }
    }
}