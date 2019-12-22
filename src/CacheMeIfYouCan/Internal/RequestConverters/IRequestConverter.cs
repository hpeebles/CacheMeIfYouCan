using System.Collections.Generic;

namespace CacheMeIfYouCan.Internal.RequestConverters
{
    internal interface IRequestConverter<in TKey, out TRequest> where TRequest : IEnumerable<TKey>
    {
        TRequest Convert(IReadOnlyCollection<TKey> keys);
    }
}