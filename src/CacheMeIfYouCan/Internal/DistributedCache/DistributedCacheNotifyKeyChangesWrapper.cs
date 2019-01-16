using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CacheMeIfYouCan.Internal.DistributedCache
{
    internal class DistributedCacheNotifyKeyChangesWrapper<TK, TV> : IDistributedCache<TK, TV>, INotifyKeyChanges<TK>
    {
        private readonly IDistributedCache<TK, TV> _cache;
        private readonly INotifyKeyChanges<TK> _notifyKeyChanges;

        public DistributedCacheNotifyKeyChangesWrapper(IDistributedCache<TK, TV> cache, INotifyKeyChanges<TK> notifyKeyChanges)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _notifyKeyChanges = notifyKeyChanges ?? throw new ArgumentNullException(nameof(notifyKeyChanges));

            CacheName = cache.CacheName;
            CacheType = cache.CacheType;
        }

        public string CacheName { get; }
        public string CacheType { get; }
        public void Dispose() => _cache.Dispose();

        public Task<GetFromCacheResult<TK, TV>> Get(Key<TK> key)
        {
            return _cache.Get(key);
        }

        public Task Set(Key<TK> key, TV value, TimeSpan timeToLive)
        {
            return _cache.Set(key, value, timeToLive);
        }

        public Task<IList<GetFromCacheResult<TK, TV>>> Get(ICollection<Key<TK>> keys)
        {
            return _cache.Get(keys);
        }

        public Task Set(ICollection<KeyValuePair<Key<TK>, TV>> values, TimeSpan timeToLive)
        {
            return _cache.Set(values, timeToLive);
        }

        public Task Remove(Key<TK> key)
        {
            return _cache.Remove(key);
        }

        public IObservable<Key<TK>> KeyChanges => _notifyKeyChanges.KeyChanges;
    }
}