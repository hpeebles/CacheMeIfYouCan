using System.Collections.Generic;
using System.Linq;

namespace CacheMeIfYouCan.Internal.RequestConverters
{
    internal sealed class ListConverter<TKey> : IRequestConverter<TKey, List<TKey>>
    {
        public List<TKey> Convert(IReadOnlyCollection<TKey> keys)
        {
            return keys.ToList();
        }
    }
}