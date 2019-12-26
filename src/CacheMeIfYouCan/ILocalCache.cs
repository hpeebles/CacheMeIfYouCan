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

    public interface ILocalCache<in TOuterKey, TInnerKey, TValue>
    {
        IReadOnlyCollection<KeyValuePair<TInnerKey, TValue>> GetMany(TOuterKey outerKey, IReadOnlyCollection<TInnerKey> innerKeys);
        
        void SetMany(TOuterKey outerKey, IReadOnlyCollection<KeyValuePair<TInnerKey, TValue>> values, TimeSpan timeToLive);

        void SetMany(TOuterKey outerKey, IReadOnlyCollection<KeyValuePair<TInnerKey, ValueAndTimeToLive<TValue>>> values);
    }
}