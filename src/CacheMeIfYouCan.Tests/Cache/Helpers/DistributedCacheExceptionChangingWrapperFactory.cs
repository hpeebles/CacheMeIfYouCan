using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CacheMeIfYouCan.Tests.Cache.Helpers
{
    public class DistributedCacheExceptionChangingWrapperFactory : IDistributedCacheWrapperFactory
    {
        public IDistributedCache<TK, TV> Wrap<TK, TV>(IDistributedCache<TK, TV> cache)
        {
            return new Wrapper<TK, TV>(cache);
        }

        private class Wrapper<TK, TV> : IDistributedCache<TK, TV>
        {
            private readonly IDistributedCache<TK, TV> _cache;

            public Wrapper(IDistributedCache<TK, TV> cache)
            {
                _cache = cache;
                    
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
                    throw new CrazyException("test", ex);
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
                    throw new CrazyException("test", ex);
                }
            }

            public async Task<IList<GetFromCacheResult<TK, TV>>> Get(ICollection<Key<TK>> keys)
            {
                try
                {
                    return await _cache.Get(keys);
                }
                catch (Exception ex)
                {
                    throw new CrazyException("test", ex);
                }
            }

            public async Task Set(ICollection<KeyValuePair<Key<TK>, TV>> values, TimeSpan timeToLive)
            {
                try
                {
                    await _cache.Set(values, timeToLive);
                }
                catch (Exception ex)
                {
                    throw new CrazyException("test", ex);
                }
            }
        }
    }
}