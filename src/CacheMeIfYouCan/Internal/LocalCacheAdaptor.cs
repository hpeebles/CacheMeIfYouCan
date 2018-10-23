using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CacheMeIfYouCan.Caches;

namespace CacheMeIfYouCan.Internal
{
    internal class LocalCacheAdaptor<TK, TV> : ICache<TK, TV>
    {
        private readonly ILocalCache<TK, TV> _cache;

        public LocalCacheAdaptor(ILocalCache<TK, TV> cache)
        {
            _cache = cache;
        }

        public string CacheType => _cache.CacheType;
        
        public Task<IList<GetFromCacheResult<TK, TV>>> Get(ICollection<Key<TK>> keys)
        {
            return Task.FromResult(_cache.Get(keys));
        }

        public Task Set(ICollection<KeyValuePair<Key<TK>, TV>> values, TimeSpan timeToLive)
        {
            _cache.Set(values, timeToLive);
            
            return Task.CompletedTask;
        }
    }
}