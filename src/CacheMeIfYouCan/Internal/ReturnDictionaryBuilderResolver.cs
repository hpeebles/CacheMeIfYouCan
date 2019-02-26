using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using CacheMeIfYouCan.Internal.ReturnDictionaryBuilders;

namespace CacheMeIfYouCan.Internal
{
    internal static class ReturnDictionaryBuilderResolver
    {
        public static IReturnDictionaryBuilder<TK, TV, TDictionary> Get<TDictionary, TK, TV>(IEqualityComparer<TK> keyComparer)
            where TDictionary : IDictionary<TK, TV>
        {
            var type = typeof(TDictionary);

            if (type == typeof(Dictionary<TK, TV>) || type == typeof(IDictionary<TK, TV>))
                return (IReturnDictionaryBuilder<TK, TV, TDictionary>)new DictionaryBuilder<TK, TV>(keyComparer);

            if (type == typeof(ReadOnlyDictionary<TK, TV>))
                return (IReturnDictionaryBuilder<TK, TV, TDictionary>)new ReadOnlyDictionaryBuilder<TK, TV>(keyComparer);

            if (type == typeof(SortedDictionary<TK, TV>))
                return (IReturnDictionaryBuilder<TK, TV, TDictionary>)new SortedDictionaryBuilder<TK, TV>(keyComparer);

            if (type == typeof(ConcurrentDictionary<TK, TV>))
                return (IReturnDictionaryBuilder<TK, TV, TDictionary>)new ConcurrentDictionaryBuilder<TK, TV>(keyComparer);
            
            throw new Exception($"Unsupported return type: '{type.Name}'");
        }
    }
}