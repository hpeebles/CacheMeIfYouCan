using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace CacheMeIfYouCan.Internal
{
    internal static class DictionaryFactoryFuncResolver
    {
        public static Func<IEqualityComparer<TK>, int, IDictionary<TK, TV>> Get<TDictionary, TK, TV>()
            where TDictionary : IDictionary<TK, TV>
        {
            var type = typeof(TDictionary);
            
            if (type.IsAssignableFrom(typeof(Dictionary<TK, TV>)))
                return (keyComparer, count) => new Dictionary<TK, TV>(count, keyComparer);

            if (type.IsAssignableFrom(typeof(SortedDictionary<TK, TV>)))
                return (keyComparer, count) => new SortedDictionary<TK, TV>();

            if (type.IsAssignableFrom(typeof(ConcurrentDictionary<TK, TV>)))
                return (keyComparer, count) => new ConcurrentDictionary<TK, TV>(keyComparer);

            if (type.GetConstructor(new Type[0]) != null)
                return (keyComparer, count) => Activator.CreateInstance<TDictionary>();
            
            throw new Exception($"Unsupported return type: '{type.Name}'");
        }
    }
}