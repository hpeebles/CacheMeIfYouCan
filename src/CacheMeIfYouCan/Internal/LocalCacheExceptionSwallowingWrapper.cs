using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CacheMeIfYouCan.Internal
{
    internal class LocalCacheExceptionSwallowingWrapper<TK, TV> : ILocalCache<TK, TV>
    {
        private readonly ILocalCache<TK, TV> _cache;
        private readonly Func<Exception, bool> _predicate;

        public LocalCacheExceptionSwallowingWrapper(
            ILocalCache<TK, TV> cache,
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

        public GetFromCacheResult<TK, TV> Get(Key<TK> key)
        {
            try
            {
                return _cache.Get(key);
            }
            catch (Exception ex) when (_predicate(ex))
            {
                return new GetFromCacheResult<TK, TV>();
            }
        }

        public void Set(Key<TK> key, TV value, TimeSpan timeToLive)
        {
            try
            {
                _cache.Set(key, value, timeToLive);
            }
            catch (Exception ex) when (_predicate(ex))
            { }
        }

        public IList<GetFromCacheResult<TK, TV>> Get(ICollection<Key<TK>> keys)
        {
            try
            {
                return _cache.Get(keys);
            }
            catch (Exception ex) when (_predicate(ex))
            {
                return new GetFromCacheResult<TK, TV>[0];
            }
        }

        public void Set(ICollection<KeyValuePair<Key<TK>, TV>> values, TimeSpan timeToLive)
        {
            try
            {
                _cache.Set(values, timeToLive);
            }
            catch (Exception ex) when (_predicate(ex))
            { }
        }

        public void Remove(Key<TK> key)
        {
            try
            {
                _cache.Remove(key);
            }
            catch (Exception ex) when (_predicate(ex))
            { }
        }
    }
}