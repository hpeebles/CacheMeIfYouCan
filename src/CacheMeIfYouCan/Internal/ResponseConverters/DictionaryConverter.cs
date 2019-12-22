using System.Collections.Generic;

namespace CacheMeIfYouCan.Internal.ResponseConverters
{
    internal sealed class DictionaryConverter<TKey, TValue> : IResponseConverter<TKey, TValue, Dictionary<TKey, TValue>>
    {
        public Dictionary<TKey, TValue> Convert(Dictionary<TKey, TValue> values) => values;
    }
}