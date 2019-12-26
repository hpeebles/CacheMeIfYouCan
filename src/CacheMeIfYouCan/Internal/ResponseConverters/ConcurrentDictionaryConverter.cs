using System.Collections.Concurrent;
using System.Collections.Generic;

namespace CacheMeIfYouCan.Internal.ResponseConverters
{
    internal sealed class ConcurrentDictionaryConverter<TKey, TValue>
        : IResponseConverter<TKey, TValue, ConcurrentDictionary<TKey, TValue>>
    {
        private readonly IEqualityComparer<TKey> _keyComparer;

        public ConcurrentDictionaryConverter(IEqualityComparer<TKey> keyComparer)
        {
            _keyComparer = keyComparer;
        }

        public ConcurrentDictionary<TKey, TValue> Convert(Dictionary<TKey, TValue> values)
        {
            return new ConcurrentDictionary<TKey, TValue>(values, _keyComparer);
        }
    }
}