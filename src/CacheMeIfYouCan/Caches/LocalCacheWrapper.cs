using System;
using System.Threading.Tasks;

namespace CacheMeIfYouCan.Caches
{
    internal class LocalCacheWrapper<TK, TV> : ICache<TK, TV>
    {
        private readonly ILocalCache<TK, TV> _cache;

        public LocalCacheWrapper(ILocalCache<TK, TV> cache)
        {
            _cache = cache;
        }
        
        public Task<GetFromCacheResult<TV>> Get(Key<TK> key)
        {
            return Task.FromResult(_cache.Get(key));
        }

        public Task Set(Key<TK> key, TV value, TimeSpan timeToLive)
        {
            _cache.Set(key, value, timeToLive);

            return Task.FromResult<object>(null);
        }
    }
}