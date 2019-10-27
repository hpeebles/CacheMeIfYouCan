using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CacheMeIfYouCan.Internal.DistributedCache
{
    internal class WrappedDistributedCacheWithOriginal<TK, TV> : IDistributedCache<TK, TV>
    {
        private readonly IDistributedCache<TK, TV> _wrapped;

        public WrappedDistributedCacheWithOriginal(IDistributedCache<TK, TV> wrapped, IDistributedCache<TK, TV> original)
        {
            _wrapped = wrapped;

            Wrapped = wrapped;
            Original = original;
            CacheName = wrapped.CacheName;
            CacheType = wrapped.CacheType;
        }

        public IDistributedCache<TK, TV> Wrapped { get; }
        public IDistributedCache<TK, TV> Original { get; }
        public string CacheName { get; }
        public string CacheType { get; }

        public Task<GetFromCacheResult<TK, TV>> Get(Key<TK> key) => _wrapped.Get(key);

        public Task Set(Key<TK> key, TV value, TimeSpan timeToLive) => _wrapped.Set(key, value, timeToLive);

        public Task<IList<GetFromCacheResult<TK, TV>>> Get(IReadOnlyCollection<Key<TK>> keys) => _wrapped.Get(keys);

        public Task Set(IReadOnlyCollection<KeyValuePair<Key<TK>, TV>> values, TimeSpan timeToLive) => _wrapped.Set(values, timeToLive);

        public Task<bool> Remove(Key<TK> key) => _wrapped.Remove(key);

        public void Dispose() => _wrapped.Dispose();
    }
}
