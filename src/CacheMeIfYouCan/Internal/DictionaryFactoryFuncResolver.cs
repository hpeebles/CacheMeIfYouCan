using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace CacheMeIfYouCan.Internal
{
    internal static class DictionaryFactoryFuncResolver
    {
        public static Func<IDictionary<TK, TV>> Get<TDictionary, TK, TV>()
            where TDictionary : IDictionary<TK, TV>
        {
            var type = typeof(TDictionary);
            
            if (type.IsAssignableFrom(typeof(Dictionary<TK, TV>)))
                return () => new Dictionary<TK, TV>();
            
            if (type.IsAssignableFrom(typeof(SortedDictionary<TK, TV>)))
                return () => new SortedDictionary<TK, TV>();
            
            if (type.IsAssignableFrom(typeof(ConcurrentDictionary<TK, TV>)))
                return () => new ConcurrentDictionary<TK, TV>();
            
            throw new Exception($"Unsupported return type: '{type.Name}'");
        }
    }
}