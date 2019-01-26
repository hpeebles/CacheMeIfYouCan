using System.Collections.Generic;

namespace CacheMeIfYouCan.Internal
{
    internal class ValueTupleComparer<T1, T2> : IEqualityComparer<(T1, T2)>
    {
        private readonly IEqualityComparer<T1> _comparer1;
        private readonly IEqualityComparer<T2> _comparer2;

        public ValueTupleComparer(IEqualityComparer<T1> comparer1, IEqualityComparer<T2> comparer2)
        {
            _comparer1 = comparer1;
            _comparer2 = comparer2;
        }
        
        public bool Equals((T1, T2) x, (T1, T2) y)
        {
            return
                _comparer1.Equals(x.Item1, y.Item1) &&
                _comparer2.Equals(x.Item2, y.Item2);
        }

        public int GetHashCode((T1, T2) obj)
        {
            return
                _comparer1.GetHashCode(obj.Item1) ^
                _comparer2.GetHashCode(obj.Item2);
        }
    }
    
    internal class ValueTupleComparer<T1, T2, T3> : IEqualityComparer<(T1, T2, T3)>
    {
        private readonly IEqualityComparer<T1> _comparer1;
        private readonly IEqualityComparer<T2> _comparer2;
        private readonly IEqualityComparer<T3> _comparer3;

        public ValueTupleComparer(
            IEqualityComparer<T1> comparer1,
            IEqualityComparer<T2> comparer2,
            IEqualityComparer<T3> comparer3)
        {
            _comparer1 = comparer1;
            _comparer2 = comparer2;
            _comparer3 = comparer3;
        }
        
        public bool Equals((T1, T2, T3) x, (T1, T2, T3) y)
        {
            return
                _comparer1.Equals(x.Item1, y.Item1) &&
                _comparer2.Equals(x.Item2, y.Item2) &&
                _comparer3.Equals(x.Item3, y.Item3);
        }

        public int GetHashCode((T1, T2, T3) obj)
        {
            return
                _comparer1.GetHashCode(obj.Item1) ^
                _comparer2.GetHashCode(obj.Item2) ^
                _comparer3.GetHashCode(obj.Item3);
        }
    }
    
    internal class ValueTupleComparer<T1, T2, T3, T4> : IEqualityComparer<(T1, T2, T3, T4)>
    {
        private readonly IEqualityComparer<T1> _comparer1;
        private readonly IEqualityComparer<T2> _comparer2;
        private readonly IEqualityComparer<T3> _comparer3;
        private readonly IEqualityComparer<T4> _comparer4;

        public ValueTupleComparer(
            IEqualityComparer<T1> comparer1,
            IEqualityComparer<T2> comparer2,
            IEqualityComparer<T3> comparer3,
            IEqualityComparer<T4> comparer4)
        {
            _comparer1 = comparer1;
            _comparer2 = comparer2;
            _comparer3 = comparer3;
            _comparer4 = comparer4;
        }
        
        public bool Equals((T1, T2, T3, T4) x, (T1, T2, T3, T4) y)
        {
            return
                _comparer1.Equals(x.Item1, y.Item1) &&
                _comparer2.Equals(x.Item2, y.Item2) &&
                _comparer3.Equals(x.Item3, y.Item3) &&
                _comparer4.Equals(x.Item4, y.Item4);
        }

        public int GetHashCode((T1, T2, T3, T4) obj)
        {
            return
                _comparer1.GetHashCode(obj.Item1) ^
                _comparer2.GetHashCode(obj.Item2) ^
                _comparer3.GetHashCode(obj.Item3) ^
                _comparer4.GetHashCode(obj.Item4);
        }
    }
}