using System;
using System.Collections.Generic;
using System.Linq;
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
        
        public Task<IList<GetFromCacheResult<TK, TV>>> Get(ICollection<Key<TK>> keys)
        {
            var results = keys
                .Select(_cache.Get)
                .ToArray();
            
            return Task.FromResult<IList<GetFromCacheResult<TK, TV>>>(results);
        }

        public Task Set(ICollection<KeyValuePair<Key<TK>, TV>> values, TimeSpan timeToLive)
        {
            foreach (var kv in values)
                _cache.Set(kv.Key, kv.Value, timeToLive);

            return Task.CompletedTask;
        }
    }
}