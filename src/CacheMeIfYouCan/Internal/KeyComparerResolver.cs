using System;
using System.Collections.Generic;

namespace CacheMeIfYouCan.Internal
{
    public static class KeyComparerResolver
    {
        public static IEqualityComparer<Key<TK>> Get<TK>()
        {
            var type = typeof(TK);

            var useGenericComparer = type.IsPrimitive || OverridesGetHashCodeAndEquals(type);
                
            return useGenericComparer
                ? (IEqualityComparer<Key<TK>>)new GenericKeyComparer<TK>()
                : new StringKeyComparer<TK>();
        }

        private static bool OverridesGetHashCodeAndEquals(Type type)
        {
            var getHashCode = type.GetMethod("GetHashCode", new Type[0]);

            if (getHashCode == null || getHashCode.DeclaringType == typeof(object))
                return false;
            
            var equals = type.GetMethod("Equals", new[] { typeof(object) });

            return equals != null && equals.DeclaringType != typeof(object);
        }
    }
}