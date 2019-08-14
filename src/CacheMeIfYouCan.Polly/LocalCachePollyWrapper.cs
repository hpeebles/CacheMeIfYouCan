using System;
using System.Collections.Generic;
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
        }

        public string CacheName => _cache.CacheName;
        public string CacheType => _cache.CacheType;
        public bool RequiresKeySerializer => _cache.RequiresKeySerializer;
        public bool RequiresKeyComparer => _cache.RequiresKeyComparer;

        public void Dispose() => _cache.Dispose();
        
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

        public bool Remove(Key<TK> key)
        {
            return _policy.Execute(() => _cache.Remove(key));
        }
    }
}