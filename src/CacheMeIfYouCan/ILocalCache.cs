using System;
using System.Collections.Generic;
using System.Linq;

namespace CacheMeIfYouCan
{
    public interface ILocalCache<TK, TV>
    {
        string CacheName { get; }
        string CacheType { get; }
        IList<GetFromCacheResult<TK, TV>> Get(ICollection<Key<TK>> keys);
        void Set(ICollection<KeyValuePair<Key<TK>, TV>> values, TimeSpan timeToLive);
        void Remove(Key<TK> key);
    }

    public static class LocalCacheExtensions
    {
        public static GetFromCacheResult<TK, TV> Get<TK, TV>(this ILocalCache<TK, TV> cache, Key<TK> key)
        {
            var results = cache.Get(new[] { key });

            return results.FirstOrDefault();
        }

        public static void Set<TK, TV>(this ILocalCache<TK, TV> cache, Key<TK> key, TV value, TimeSpan timeToLive)
        {
            cache.Set(new[] { new KeyValuePair<Key<TK>, TV>(key, value) }, timeToLive);
        }
    }
}