using System;
using System.Collections.Generic;

namespace CacheMeIfYouCan
{
    public interface ILocalCache<TKey, TValue>
    {
        bool TryGet(TKey key, out TValue value);

        void Set(TKey key, TValue value, TimeSpan timeToLive);

        IReadOnlyCollection<KeyValuePair<TKey, TValue>> GetMany(IReadOnlyCollection<TKey> keys);

        void SetMany(IReadOnlyCollection<KeyValuePair<TKey, TValue>> values, TimeSpan timeToLive);
    }
}