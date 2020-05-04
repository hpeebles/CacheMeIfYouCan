using System;
using System.Collections.Generic;

namespace CacheMeIfYouCan.Internal.RequestConverters
{
    internal sealed class ListConverter<TKey> : IRequestConverter<TKey, List<TKey>>
    {
        public List<TKey> Convert(ReadOnlyMemory<TKey> keys)
        {
            var list = new List<TKey>(keys.Length);
            
            foreach (var key in keys.Span)
                list.Add(key);

            return list;
        }
    }
}