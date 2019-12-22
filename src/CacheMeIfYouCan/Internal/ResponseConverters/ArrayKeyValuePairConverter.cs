using System.Collections.Generic;
using System.Linq;

namespace CacheMeIfYouCan.Internal.ResponseConverters
{
    internal sealed class ArrayKeyValuePairConverter<TKey, TValue>
        : IResponseConverter<TKey, TValue, KeyValuePair<TKey, TValue>[]>
    {
        public KeyValuePair<TKey, TValue>[] Convert(Dictionary<TKey, TValue> values)
        {
            return values.ToArray();
        }
    }
}