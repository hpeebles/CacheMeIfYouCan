using System;
using System.Collections.Generic;
using CacheMeIfYouCan.Notifications;

namespace CacheMeIfYouCan.Internal.LocalCache
{
    internal class LocalCacheExceptionFormattingWrapper<TK, TV> : ILocalCache<TK, TV>
    {
        private readonly ILocalCache<TK, TV> _cache;
        private const string CacheGetErrorMessage = "LocalCache.Get exception";
        private const string CacheSetErrorMessage = "LocalCache.Set exception";
        private const string CacheRemoveErrorMessage = "LocalCache.Remove exception";

        public LocalCacheExceptionFormattingWrapper(ILocalCache<TK, TV> cache)
        {
            _cache = cache;
            
            CacheName = cache.CacheName;
            CacheType = cache.CacheType;
        }

        public string CacheName { get; }
        public string CacheType { get; }

        public void Dispose() => _cache.Dispose();
        
        public GetFromCacheResult<TK, TV> Get(Key<TK> key)
        {
            try
            {
                return _cache.Get(key);
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

        public void Set(Key<TK> key, TV value, TimeSpan timeToLive)
        {
            try
            {
                _cache.Set(key, value, timeToLive);
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

        public IList<GetFromCacheResult<TK, TV>> Get(ICollection<Key<TK>> keys)
        {
            try
            {
                return _cache.Get(keys);
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

        public void Set(ICollection<KeyValuePair<Key<TK>, TV>> values, TimeSpan timeToLive)
        {
            try
            {
                _cache.Set(values, timeToLive);
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

        public bool Remove(Key<TK> key)
        {
            try
            {
                return _cache.Remove(key);
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