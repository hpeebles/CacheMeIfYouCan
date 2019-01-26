using System;
using System.Collections.Generic;
using System.Linq;

namespace CacheMeIfYouCan
{
    public class EqualityComparers
    {
        private readonly Dictionary<Type, object> _comparers;

        internal EqualityComparers()
        {
            _comparers = new Dictionary<Type, object>();
        }
        
        private EqualityComparers(Dictionary<Type, object> comparers)
        {
            _comparers = comparers;
        }

        public EqualityComparers Set<T>(IEqualityComparer<T> comparer)
        {
            _comparers[typeof(T)] = comparer;
            return this;
        }

        internal bool TryGet<T>(out IEqualityComparer<T> comparer)
        {
            var type = typeof(T);

            if (_comparers.TryGetValue(type, out var comparerObj))
                comparer = (IEqualityComparer<T>) comparerObj;
            else
                comparer = null;

            return comparer != null;
        }

        internal EqualityComparers Clone()
        {
            var comparersClone = _comparers.ToDictionary(kv => kv.Key, kv => kv.Value);
            
            return new EqualityComparers(comparersClone);
        }
    }
}