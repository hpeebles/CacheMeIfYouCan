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
            catch (CacheGetException<TK> ex)
            {
                error = true;
                _onError?.Invoke(ex);

                throw;
            }
            catch (Exception ex) when (ExceptionsHelper.TryGetInnerExceptionOfType<CacheGetException<TK>>(ex, out var innerException))
            {
                error = true;
                _onError?.Invoke(innerException);

                throw;
            }
            catch (Exception) // _onError will not be triggered in this scenario
            {
                error = true;
                throw;
            }
            finally
            {
                _onCacheGetResult?.Invoke(new CacheGetResult<TK, TV>(
                    CacheName,
                    CacheType,
                    result.Success ? new[] { result } : new GetFromCacheResult<TK, TV>[0],
                    result.Success ? new Key<TK>[0] : new[] { key },
                    !error,
                    timestamp,
                    StopwatchHelper.GetDuration(stopwatchStart)));
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
            catch (CacheSetException<TK, TV> ex)
            {
                error = true;
                _onError?.Invoke(ex);

                throw;
            }
            catch (Exception ex) when (ExceptionsHelper.TryGetInnerExceptionOfType<CacheSetException<TK, TV>>(ex, out var innerException))
            {
                error = true;
                _onError?.Invoke(innerException);

                throw;
            }
            catch (Exception) // _onError will not be triggered in this scenario
            {
                error = true;
                throw;
            }
            finally
            {
                _onCacheSetResult?.Invoke(new CacheSetResult<TK, TV>(
                    CacheName,
                    CacheType,
                    new[] { new KeyValuePair<Key<TK>, TV>(key, value) },
                    !error,
                    timestamp,
                    StopwatchHelper.GetDuration(stopwatchStart)));
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
            catch (CacheGetException<TK> ex)
            {
                error = true;
                _onError?.Invoke(ex);

                throw;
            }
            catch (Exception ex) when (ExceptionsHelper.TryGetInnerExceptionOfType<CacheGetException<TK>>(ex, out var innerException))
            {
                error = true;
                _onError?.Invoke(innerException);

                throw;
            }
            catch (Exception) // _onError will not be triggered in this scenario
            {
                error = true;
                throw;
            }
            finally
            {
                _onCacheGetResult?.Invoke(new CacheGetResult<TK, TV>(
                    CacheName,
                    CacheType,
                    results,
                    results == null || !results.Any()
                        ? keys
                        : keys.Except(results.Select(r => r.Key)).ToArray(),
                    !error,
                    timestamp,
                    StopwatchHelper.GetDuration(stopwatchStart)));
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
            catch (CacheSetException<TK, TV> ex)
            {
                error = true;
                _onError?.Invoke(ex);

                throw;
            }
            catch (Exception ex) when (ExceptionsHelper.TryGetInnerExceptionOfType<CacheSetException<TK, TV>>(ex, out var innerException))
            {
                error = true;
                _onError?.Invoke(innerException);

                throw;
            }
            catch (Exception) // _onError will not be triggered in this scenario
            {
                error = true;
                throw;
            }
            finally
            {
                _onCacheSetResult?.Invoke(new CacheSetResult<TK, TV>(
                    CacheName,
                    CacheType,
                    values,
                    !error,
                    timestamp,
                    StopwatchHelper.GetDuration(stopwatchStart)));
            }
        }

        public void Remove(Key<TK> key)
        {
            _cache.Remove(key);
        }
    }
}