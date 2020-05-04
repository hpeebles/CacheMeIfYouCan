using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace CacheMeIfYouCan.Internal.RequestConverters
{
    internal sealed class IListConverter<TKey> : IRequestConverter<TKey, IList<TKey>>
    {
        public IList<TKey> Convert(ReadOnlyMemory<TKey> keys)
        {
            return MemoryMarshal.TryGetArray(keys, out var arraySegment)
                ? (IList<TKey>)arraySegment
                : keys.ToArray();
        }
    }
}