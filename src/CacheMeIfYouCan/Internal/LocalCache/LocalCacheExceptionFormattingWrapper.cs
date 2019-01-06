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
                    Timestamp.Now,
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
                    Timestamp.Now,
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
                    Timestamp.Now,
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
                    Timestamp.Now,
                    CacheSetErrorMessage,
                    ex);
            }
        }

        public void Remove(Key<TK> key)
        {
            _cache.Remove(key);
        }
    }
}