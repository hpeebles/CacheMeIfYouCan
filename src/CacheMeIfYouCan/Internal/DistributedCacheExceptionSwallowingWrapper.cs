using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CacheMeIfYouCan.Internal
{
    internal class DistributedCacheExceptionSwallowingWrapper<TK, TV> : IDistributedCache<TK, TV>
    {
        private readonly IDistributedCache<TK, TV> _cache;
        private readonly Func<Exception, bool> _predicate;

        public DistributedCacheExceptionSwallowingWrapper(
            IDistributedCache<TK, TV> cache,
            Func<Exception, bool> predicate)
        {
            _cache = cache;
            _predicate = predicate;

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
            catch (Exception ex) when (_predicate(ex))
            {
                return new GetFromCacheResult<TK, TV>();
            }
        }

        public async Task Set(Key<TK> key, TV value, TimeSpan timeToLive)
        {
            try
            {
                await _cache.Set(key, value, timeToLive);
            }
            catch (Exception ex) when (_predicate(ex))
            { }
        }

        public async Task<IList<GetFromCacheResult<TK, TV>>> Get(ICollection<Key<TK>> keys)
        {
            try
            {
                return await _cache.Get(keys);
            }
            catch (Exception ex) when (_predicate(ex))
            {
                return new GetFromCacheResult<TK, TV>[0];
            }
        }

        public async Task Set(ICollection<KeyValuePair<Key<TK>, TV>> values, TimeSpan timeToLive)
        {
            try
            {
                await _cache.Set(values, timeToLive);
            }
            catch (Exception ex) when (_predicate(ex))
            { }
        }
    }
}