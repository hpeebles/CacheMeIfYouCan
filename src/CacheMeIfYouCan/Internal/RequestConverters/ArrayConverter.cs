using System.Collections.Generic;
using System.Linq;

namespace CacheMeIfYouCan.Internal.RequestConverters
{
    internal sealed class ArrayConverter<TKey> : IRequestConverter<TKey, TKey[]>
    {
        public TKey[] Convert(IReadOnlyCollection<TKey> keys)
        {
            return keys.ToArray();
        }
    }
}