using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CacheMeIfYouCan.Caches
{
    public interface ICache<TK, TV>
    {
        string CacheType { get; }
        Task<IList<GetFromCacheResult<TK, TV>>> Get(ICollection<Key<TK>> keys);
        Task Set(ICollection<KeyValuePair<Key<TK>, TV>> values, TimeSpan timeToLive);
    }

    public static class CacheExtensions
    {
        public static async Task<GetFromCacheResult<TK, TV>> Get<TK, TV>(this ICache<TK, TV> cache, Key<TK> key)
        {
            var results = await cache.Get(new[] { key });

            return results.Any()
                ? results.First()
                : new GetFromCacheResult<TK, TV>();
        }

        public static Task Set<TK, TV>(this ICache<TK, TV> cache, Key<TK> key, TV value, TimeSpan timeToLive)
        {
            return cache.Set(new[] { new KeyValuePair<Key<TK>, TV>(key, value) }, timeToLive);
        }
    }
}