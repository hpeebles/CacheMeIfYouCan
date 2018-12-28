using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Polly;

namespace CacheMeIfYouCan.Polly
{
    internal class LocalCachePollyWrapper<TK, TV> : ILocalCache<TK, TV>
    {
        private readonly ILocalCache<TK, TV> _cache;
        private readonly Policy _policy;

        public LocalCachePollyWrapper(ILocalCache<TK, TV> cache, Policy policy)
        {
            _cache = cache;
            _policy = policy;
            
            CacheName = _cache.CacheName;
            CacheType = _cache.CacheType;
        }

        public string CacheName { get; }
        public string CacheType { get; }
        
        public GetFromCacheResult<TK, TV> Get(Key<TK> key)
        {
            return _policy.Execute(() => _cache.Get(key));
        }

        public void Set(Key<TK> key, TV value, TimeSpan timeToLive)
        {
            _policy.Execute(() => _cache.Set(key, value, timeToLive));
        }

        public IList<GetFromCacheResult<TK, TV>> Get(ICollection<Key<TK>> keys)
        {
            return _policy.Execute(() => _cache.Get(keys));
        }

        public void Set(ICollection<KeyValuePair<Key<TK>, TV>> values, TimeSpan timeToLive)
        {
            _policy.Execute(() => _cache.Set(values, timeToLive));
        }

        public void Remove(Key<TK> key)
        {
            _policy.Execute(() => _cache.Remove(key));
        }
    }
}