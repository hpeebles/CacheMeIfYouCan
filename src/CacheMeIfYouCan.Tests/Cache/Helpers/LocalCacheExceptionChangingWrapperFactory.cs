using System;
using System.Collections.Generic;

namespace CacheMeIfYouCan.Tests.Cache.Helpers
{
    public class LocalCacheExceptionChangingWrapperFactory : ILocalCacheWrapperFactory
    {
        public ILocalCache<TK, TV> Wrap<TK, TV>(ILocalCache<TK, TV> cache)
        {
            return new Wrapper<TK, TV>(cache);
        }

        private class Wrapper<TK, TV> : ILocalCache<TK, TV>
        {
            private readonly ILocalCache<TK, TV> _cache;

            public Wrapper(ILocalCache<TK, TV> cache)
            {
                _cache = cache;
                    
                CacheName = cache.CacheName;
                CacheType = cache.CacheType;
            }

            public string CacheName { get; }
            public string CacheType { get; }
                
            public GetFromCacheResult<TK, TV> Get(Key<TK> key)
            {
                try
                {
                    return _cache.Get(key);
                }
                catch (Exception ex)
                {
                    throw new CrazyException("test", ex);
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
                    throw new CrazyException("test", ex);
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
                    throw new CrazyException("test", ex);
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
                    throw new CrazyException("test", ex);
                }
            }

            public void Remove(Key<TK> key)
            {
                try
                {
                    _cache.Remove(key);
                }
                catch (Exception ex)
                {
                    throw new CrazyException("test", ex);
                }
            }
        }
    }
}