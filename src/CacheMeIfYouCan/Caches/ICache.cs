using System;
using System.Threading.Tasks;

namespace CacheMeIfYouCan.Caches
{
    public interface ICache<TK, TV>
    {
        Task<GetFromCacheResult<TV>> Get(Key<TK> key);
        Task Set(Key<TK> key, TV value, TimeSpan timeToLive);
    }
}