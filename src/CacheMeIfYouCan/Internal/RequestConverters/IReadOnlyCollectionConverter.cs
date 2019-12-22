using System.Collections.Generic;

namespace CacheMeIfYouCan.Internal.RequestConverters
{
    internal sealed class IReadOnlyCollectionConverter<TKey> : IRequestConverter<TKey, IReadOnlyCollection<TKey>>
    {
        public IReadOnlyCollection<TKey> Convert(IReadOnlyCollection<TKey> keys) => keys;
    }
}