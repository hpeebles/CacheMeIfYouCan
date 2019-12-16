using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CacheMeIfYouCan
{
    public interface IDistributedCache<TKey, TValue>
    {
        Task<(bool Success, ValueAndTimeToLive<TValue> Value)> TryGet(TKey key);

        Task Set(TKey key, TValue value, TimeSpan timeToLive);

        Task<IReadOnlyCollection<KeyValuePair<TKey, ValueAndTimeToLive<TValue>>>> GetMany(IReadOnlyCollection<TKey> keys);

        Task SetMany(IReadOnlyCollection<KeyValuePair<TKey, TValue>> values, TimeSpan timeToLive);
    }
}