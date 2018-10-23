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
}