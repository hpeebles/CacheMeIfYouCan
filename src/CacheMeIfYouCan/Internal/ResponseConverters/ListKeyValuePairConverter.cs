using System.Collections.Generic;
using System.Linq;

namespace CacheMeIfYouCan.Internal.ResponseConverters
{
    internal sealed class ListKeyValuePairConverter<TKey, TValue> :
        IResponseConverter<TKey, TValue, List<KeyValuePair<TKey, TValue>>>
    {
        public List<KeyValuePair<TKey, TValue>> Convert(Dictionary<TKey, TValue> values)
        {
            return new List<KeyValuePair<TKey, TValue>>(values);
        }
    }
}