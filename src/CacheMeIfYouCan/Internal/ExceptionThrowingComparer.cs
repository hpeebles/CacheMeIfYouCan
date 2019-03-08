using System;
using System.Collections.Generic;

namespace CacheMeIfYouCan.Internal
{
    internal class ExceptionThrowingComparer<T> : IEqualityComparer<T>
    {
        public bool Equals(T x, T y)
        {
            throw Ex;
        }

        public int GetHashCode(T obj)
        {
            throw Ex;
        }
        
        private static Exception Ex => new Exception($"No EqualityComparer defined for type '{typeof(T).Name}'");
    }
}