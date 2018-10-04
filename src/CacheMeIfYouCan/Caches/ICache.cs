using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CacheMeIfYouCan.Caches
{
    public interface ICache<TK, TV>
    {
        Task<IList<GetFromCacheResult<TK, TV>>> Get(ICollection<Key<TK>> keys);
        Task Set(ICollection<KeyValuePair<Key<TK>, TV>> values, TimeSpan timeToLive);
    }

    internal static class CacheExtensions
    {
        public static async Task<GetFromCacheResult<TK, TV>> Get<TK, TV>(this ICache<TK, TV> cache, Key<TK> key)
        {
            var result = await cache.Get(new[] { key });

            return result?.SingleOrDefault() ?? GetFromCacheResult<TK, TV>.NotFound(key);
        }
    }
}