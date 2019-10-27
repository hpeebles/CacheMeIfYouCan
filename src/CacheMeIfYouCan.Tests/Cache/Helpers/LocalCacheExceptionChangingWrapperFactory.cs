using System;
using System.Collections.Generic;
using System.Linq;
using CacheMeIfYouCan.Tests.Common;

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
            }
            
            public string CacheName => _cache.CacheName;
            public string CacheType => _cache.CacheType;
            public bool RequiresKeySerializer => _cache.RequiresKeySerializer;
            public bool RequiresKeyComparer => _cache.RequiresKeyComparer;

            public void Dispose() => _cache.Dispose();
                
            public GetFromCacheResult<TK, TV> Get(Key<TK> key)
            {
                try
                {
                    return _cache.Get(key);
                }
                catch (Exception ex)
                {
                    throw new TestException(key.AsStringSafe, ex);
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
                    throw new TestException(key.AsStringSafe, ex);
                }
            }

            public IList<GetFromCacheResult<TK, TV>> Get(IReadOnlyCollection<Key<TK>> keys)
            {
                try
                {
                    return _cache.Get(keys);
                }
                catch (Exception ex)
                {
                    throw new TestException(String.Join(",", keys.Select(k => k.AsStringSafe)), ex);
                }
            }

            public void Set(IReadOnlyCollection<KeyValuePair<Key<TK>, TV>> values, TimeSpan timeToLive)
            {
                try
                {
                    _cache.Set(values, timeToLive);
                }
                catch (Exception ex)
                {
                    throw new TestException(String.Join(",", values.Select(kv => kv.Key.AsStringSafe)), ex);
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
                    throw new TestException(key.AsStringSafe, ex);
                }
            }
        }
    }
}