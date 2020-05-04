using System;
using System.Runtime.InteropServices;

namespace CacheMeIfYouCan.Internal.RequestConverters
{
    internal sealed class ArraySegmentConverter<TKey> : IRequestConverter<TKey, ArraySegment<TKey>>
    {
        public ArraySegment<TKey> Convert(ReadOnlyMemory<TKey> keys)
        {
            return MemoryMarshal.TryGetArray(keys, out var arraySegment)
                ? arraySegment
                : new ArraySegment<TKey>(keys.ToArray());
        }
    }
}