using System.Collections.Generic;

namespace CacheMeIfYouCan.Internal.RequestConverters
{
    internal sealed class HashSetConverter<TKey> : IRequestConverter<TKey, HashSet<TKey>>
    {
        private readonly IEqualityComparer<TKey> _keyComparer;

        public HashSetConverter(IEqualityComparer<TKey> keyComparer)
        {
            _keyComparer = keyComparer;
        }

        public HashSet<TKey> Convert(IReadOnlyCollection<TKey> keys)
        {
            return new HashSet<TKey>(keys, _keyComparer);
        }
    }
}