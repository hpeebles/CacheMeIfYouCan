using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CacheMeIfYouCan.Notifications;

namespace CacheMeIfYouCan.Internal.LocalCache
{
    internal class LocalCacheNotificationWrapper<TK, TV> : ILocalCache<TK, TV> 
    {
        private readonly ILocalCache<TK, TV> _cache;
        private readonly Action<CacheGetResult<TK, TV>> _onCacheGetResult;
        private readonly Action<CacheSetResult<TK, TV>> _onCacheSetResult;
        private readonly Action<CacheRemoveResult<TK>> _onCacheRemoveResult;
        private readonly Action<CacheException<TK>> _onException;

        public LocalCacheNotificationWrapper(
            ILocalCache<TK, TV> cache,
            Action<CacheGetResult<TK, TV>> onCacheGetResult,
            Action<CacheSetResult<TK, TV>> onCacheSetResult,
            Action<CacheRemoveResult<TK>> onCacheRemoveResult,
            Action<CacheException<TK>> onException)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _onCacheGetResult = onCacheGetResult;
            _onCacheSetResult = onCacheSetResult;
            _onCacheRemoveResult = onCacheRemoveResult;
            _onException = onException;
        }

        public string CacheName => _cache.CacheName;
        public string CacheType => _cache.CacheType;
        public bool RequiresKeySerializer => _cache.RequiresKeySerializer;
        public bool RequiresKeyComparer => _cache.RequiresKeyComparer;
        
        public void Dispose() => _cache.Dispose();
        
        public GetFromCacheResult<TK, TV> Get(Key<TK> key)
        {
            var start = DateTime.UtcNow;
            var stopwatchStart = Stopwatch.GetTimestamp();
            var error = false;
            var result = new GetFromCacheResult<TK, TV>();

            try
            {
                result = _cache.Get(key);
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

        public void Set(Key<TK> key, TV value, TimeSpan timeToLive)
        {
            var start = DateTime.UtcNow;
            var stopwatchStart = Stopwatch.GetTimestamp();
            var error = false;

            try
            {
                _cache.Set(key, value, timeToLive);
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

        public IList<GetFromCacheResult<TK, TV>> Get(ICollection<Key<TK>> keys)
        {
            var start = DateTime.UtcNow;
            var stopwatchStart = Stopwatch.GetTimestamp();
            var error = false;
            IList<GetFromCacheResult<TK, TV>> results = null;
            
            try
            {
                results = _cache.Get(keys);
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
                            : keys.Except(results.Select(r => r.Key)).ToArray(),
                        !error,
                        start,
                        StopwatchHelper.GetDuration(stopwatchStart));
                    
                    _onCacheGetResult?.Invoke(cacheGetResult);
                    TraceHandlerInternal.Mark(cacheGetResult);
                }
            }

            return results;
        }

        public void Set(ICollection<KeyValuePair<Key<TK>, TV>> values, TimeSpan timeToLive)
        {
            var start = DateTime.UtcNow;
            var stopwatchStart = Stopwatch.GetTimestamp();
            var error = false;

            try
            {
                _cache.Set(values, timeToLive);
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

        public bool Remove(Key<TK> key)
        {
            var start = DateTime.UtcNow;
            var stopwatchStart = Stopwatch.GetTimestamp();
            var error = false;
            var keyRemoved = false;

            try
            {
                keyRemoved = _cache.Remove(key);

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