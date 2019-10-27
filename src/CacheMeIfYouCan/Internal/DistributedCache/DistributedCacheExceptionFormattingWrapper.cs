using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CacheMeIfYouCan.Notifications;

namespace CacheMeIfYouCan.Internal.DistributedCache
{
    internal class DistributedCacheExceptionFormattingWrapper<TK, TV> : IDistributedCache<TK, TV>
    {
        private readonly IDistributedCache<TK, TV> _cache;
        private const string CacheGetErrorMessage = "DistributedCache.Get exception";
        private const string CacheSetErrorMessage = "DistributedCache.Set exception";
        private const string CacheRemoveErrorMessage = "DistributedCache.Remove exception";

        public DistributedCacheExceptionFormattingWrapper(IDistributedCache<TK, TV> cache)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            
            CacheName = cache.CacheName;
            CacheType = cache.CacheType;
        }

        public string CacheName { get; }
        public string CacheType { get; }
        
        public void Dispose() => _cache.Dispose();
        
        public async Task<GetFromCacheResult<TK, TV>> Get(Key<TK> key)
        {
            try
            {
                return await _cache.Get(key);
            }
            catch (Exception ex)
            {
                throw new CacheGetException<TK>(
                    CacheName,
                    CacheType,
                    new[] { key },
                    CacheGetErrorMessage,
                    ex);
            }
        }

        public async Task Set(Key<TK> key, TV value, TimeSpan timeToLive)
        {
            try
            {
                await _cache.Set(key, value, timeToLive);
            }
            catch (Exception ex)
            {
                throw new CacheSetException<TK, TV>(
                    CacheName,
                    CacheType,
                    new[] { new KeyValuePair<Key<TK>, TV>(key, value) },
                    timeToLive,
                    CacheSetErrorMessage,
                    ex);
            }
        }

        public async Task<IList<GetFromCacheResult<TK, TV>>> Get(IReadOnlyCollection<Key<TK>> keys)
        {
            try
            {
                return await _cache.Get(keys);
            }
            catch (Exception ex)
            {
                throw new CacheGetException<TK>(
                    CacheName,
                    CacheType,
                    keys,
                    CacheGetErrorMessage,
                    ex);
            }
        }

        public async Task Set(IReadOnlyCollection<KeyValuePair<Key<TK>, TV>> values, TimeSpan timeToLive)
        {
            try
            {
                await _cache.Set(values, timeToLive);
            }
            catch (Exception ex)
            {
                throw new CacheSetException<TK, TV>(
                    CacheName,
                    CacheType,
                    values,
                    timeToLive,
                    CacheSetErrorMessage,
                    ex);
            }
        }

        public async Task<bool> Remove(Key<TK> key)
        {
            try
            {
                return await _cache.Remove(key);
            }
            catch (Exception ex)
            {
                throw new CacheRemoveException<TK>(
                    CacheName,
                    CacheType,
                    key,
                    CacheRemoveErrorMessage,
                    ex);
            }
        }
    }
}