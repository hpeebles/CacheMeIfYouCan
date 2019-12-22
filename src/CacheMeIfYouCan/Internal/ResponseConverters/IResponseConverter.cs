using System.Collections.Generic;

namespace CacheMeIfYouCan.Internal.ResponseConverters
{
    internal interface IResponseConverter<TKey, TValue, out TResponse>
        where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        TResponse Convert(Dictionary<TKey, TValue> values);
    }
}