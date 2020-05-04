using System;
using System.Collections.Generic;

namespace CacheMeIfYouCan.Internal.RequestConverters
{
    internal interface IRequestConverter<TKey, out TRequest> where TRequest : IEnumerable<TKey>
    {
        TRequest Convert(ReadOnlyMemory<TKey> keys);
    }
}