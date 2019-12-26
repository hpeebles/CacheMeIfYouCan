using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CacheMeIfYouCan.Internal
{
    internal interface ICache<TKey, TValue>
    {
        ValueTask<(bool Success, TValue Value)> TryGet(TKey key);

        ValueTask Set(TKey key, TValue value, TimeSpan timeToLive);

        ValueTask<IReadOnlyCollection<KeyValuePair<TKey, TValue>>> GetMany(IReadOnlyCollection<TKey> keys);

        ValueTask SetMany(IReadOnlyCollection<KeyValuePair<TKey, TValue>> values, TimeSpan timeToLive);
    }

    internal interface ICache<in TOuterKey, TInnerKey, TValue>
    {
        ValueTask<IReadOnlyCollection<KeyValuePair<TInnerKey, TValue>>> GetMany(TOuterKey outerKey, IReadOnlyCollection<TInnerKey> innerKeys);

        ValueTask SetMany(TOuterKey outerKey, IReadOnlyCollection<KeyValuePair<TInnerKey, TValue>> values, TimeSpan timeToLive);
    }
}