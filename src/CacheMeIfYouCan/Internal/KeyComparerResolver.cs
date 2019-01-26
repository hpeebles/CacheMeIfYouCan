using System;
using System.Collections.Generic;

namespace CacheMeIfYouCan.Internal
{
    internal static class KeyComparerResolver
    {
        public static KeyComparer<T> Get<T>(EqualityComparers comparers = null, bool allowNull = false)
        {
            var inner = GetInner<T>(comparers);

            return inner == null
                ? null
                : new KeyComparer<T>(inner);
        }

        public static IEqualityComparer<T> GetInner<T>(EqualityComparers comparers = null, bool allowNull = false)
        {
            if (comparers != null && comparers.TryGet<T>(out var comparer))
                return comparer;

            if (DefaultSettings.Cache.KeyComparers.TryGet(out comparer))
                return comparer;

            var type = typeof(T);
            
            if (OverridesGetHashCodeAndEquals(type))
                return EqualityComparer<T>.Default;

            if (allowNull)
                return null;
            
            throw new Exception($"No equality comparer defined for type: '{typeof(T).FullName}'");
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