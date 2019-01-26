using System.Collections.Generic;

namespace CacheMeIfYouCan.Tests
{
    public class TestEqualityComparer<T> : IEqualityComparer<T>
    {
        private readonly IEqualityComparer<T> _comparer = EqualityComparer<T>.Default;
        
        public bool Equals(T x, T y)
        {
            EqualsCount++;
            
            return _comparer.Equals(x, y);
        }

        public int GetHashCode(T obj)
        {
            GetHashCodeCount++;
            
            return _comparer.GetHashCode(obj);
        }

        public int EqualsCount { get; private set; }
        public int GetHashCodeCount { get; private set; }
    }
}