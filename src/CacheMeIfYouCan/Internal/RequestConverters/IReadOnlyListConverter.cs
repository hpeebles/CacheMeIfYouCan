using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace CacheMeIfYouCan.Internal.RequestConverters
{
    internal sealed class IReadOnlyListConverter<TKey> : IRequestConverter<TKey, IReadOnlyList<TKey>>
    {
        public IReadOnlyList<TKey> Convert(ReadOnlyMemory<TKey> keys)
        {
            return MemoryMarshal.TryGetArray(keys, out var arraySegment)
                ? (IReadOnlyList<TKey>)arraySegment
                : keys.ToArray();
        }
    }
}