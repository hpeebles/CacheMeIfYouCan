using System;
using System.Collections.Generic;

namespace CacheMeIfYouCan.Caches
{
    public interface ILocalCache<TK, TV>
    {
        string CacheType { get; }
        IList<GetFromCacheResult<TK, TV>> Get(ICollection<Key<TK>> keys);
        void Set(ICollection<KeyValuePair<Key<TK>, TV>> values, TimeSpan timeToLive);
        void Remove(Key<TK> key);
    }

    public static class LocalCacheExtensions
    {
        public static void Set<TK, TV>(this ILocalCache<TK, TV> cache, Key<TK> key, TV value, TimeSpan timeToLive)
        {
            cache.Set(new[] { new  KeyValuePair<Key<TK>, TV>(key, value) }, timeToLive);
        }
    }
}