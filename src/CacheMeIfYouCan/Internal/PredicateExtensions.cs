using System;

namespace CacheMeIfYouCan.Internal
{
    public static class PredicateExtensions
    {
        public static Func<T, bool> Or<T>(this Func<T, bool> left, Func<T, bool> right)
        {
            if (left == null)
                return right;

            if (right == null)
                return left;

            return x => left(x) || right(x);
        }
        
        public static Func<T1, T2, bool> Or<T1, T2>(this Func<T1, T2, bool> left, Func<T1, T2, bool> right)
        {
            if (left == null)
                return right;

            if (right == null)
                return left;

            return (x, y) => left(x, y) || right(x, y);
        }
        
        public static Func<T1, T2, T3, bool> Or<T1, T2, T3>(this Func<T1, T2, T3, bool> left, Func<T1, T2, T3, bool> right)
        {
            if (left == null)
                return right;

            if (right == null)
                return left;

            return (x, y, z) => left(x, y, z) || right(x, y, z);
        }
    }
}