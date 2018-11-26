using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CacheMeIfYouCan
{
    public interface IDistributedCache<TK, TV>
    {
        string CacheName { get; }
        string CacheType { get; }
        Task<GetFromCacheResult<TK, TV>> Get(Key<TK> key);
        Task Set(Key<TK> key, TV value, TimeSpan timeToLive);
        Task<IList<GetFromCacheResult<TK, TV>>> Get(ICollection<Key<TK>> keys);
        Task Set(ICollection<KeyValuePair<Key<TK>, TV>> values, TimeSpan timeToLive);
    }
}