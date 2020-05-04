using System;

namespace CacheMeIfYouCan.Internal.RequestConverters
{
    internal sealed class ArrayConverter<TKey> : IRequestConverter<TKey, TKey[]>
    {
        public TKey[] Convert(ReadOnlyMemory<TKey> keys)
        {
            return keys.ToArray();
        }
    }
}