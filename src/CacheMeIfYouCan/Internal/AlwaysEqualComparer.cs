using System.Collections.Generic;

namespace CacheMeIfYouCan.Internal
{
    public class AlwaysEqualComparer<T> : IEqualityComparer<T>
    {
        public bool Equals(T x, T y)
        {
            return true;
        }

        public int GetHashCode(T obj)
        {
            return 0;
        }
    }
}