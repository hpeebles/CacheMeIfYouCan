using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CacheMeIfYouCan
{
    public interface ICache<TK, TV>
    {
        Task<TV> Get(TK key);
        Task Set(TK key, TV value, TimeSpan timeToLive);
        Task<IDictionary<TK, TV>> Get(ICollection<TK> keys);
        Task Set(ICollection<KeyValuePair<TK, TV>> values, TimeSpan timeToLive);
    }
}