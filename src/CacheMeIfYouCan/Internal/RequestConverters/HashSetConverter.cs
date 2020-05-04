using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace CacheMeIfYouCan.Internal.RequestConverters
{
    internal sealed class HashSetConverter<TKey> : IRequestConverter<TKey, HashSet<TKey>>
    {
        private readonly IEqualityComparer<TKey> _keyComparer;

        public HashSetConverter(IEqualityComparer<TKey> keyComparer)
        {
            _keyComparer = keyComparer;
        }

        public HashSet<TKey> Convert(ReadOnlyMemory<TKey> keys)
        {
            if (MemoryMarshal.TryGetArray(keys, out var arraySegment))
                return new HashSet<TKey>(arraySegment, _keyComparer);
            
            var hashSet = new HashSet<TKey>(_keyComparer);

            foreach (var key in keys.Span)
                hashSet.Add(key);

            return hashSet;
        }
    }
}