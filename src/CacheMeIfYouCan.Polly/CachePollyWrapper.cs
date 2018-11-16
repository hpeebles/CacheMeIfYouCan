using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Polly;

namespace CacheMeIfYouCan.Polly
{
    internal class CachePollyWrapper<TK, TV> : ICache<TK, TV>
    {
        private readonly ICache<TK, TV> _cache;
        private readonly Policy _policy;

        public CachePollyWrapper(ICache<TK, TV> cache, Policy policy)
        {
            _cache = cache;
            _policy = policy;
            CacheName = _cache.CacheName;
            CacheType = _cache.CacheType;
        }

        public string CacheName { get; }
        public string CacheType { get; }
        
        public Task<IList<GetFromCacheResult<TK, TV>>> Get(ICollection<Key<TK>> keys)
        {
            return _policy.ExecuteAsync(() => _cache.Get(keys));
        }

        public Task Set(ICollection<KeyValuePair<Key<TK>, TV>> values, TimeSpan timeToLive)
        {
            return _policy.ExecuteAsync(() => _cache.Set(values, timeToLive));
        }
    }
}