using System.Collections.Generic;

namespace CacheMeIfYouCan
{
    public class KeyComparer<TK> : IEqualityComparer<Key<TK>>, IEqualityComparer<TK>
    {
        private readonly IEqualityComparer<TK> _innerComparer;

        internal KeyComparer(IEqualityComparer<TK> innerComparer)
        {
            _innerComparer = innerComparer;
        }

        public bool Equals(Key<TK> left, Key<TK> right)
        {
            return Equals(left.AsObject, right.AsObject);
        }

        public int GetHashCode(Key<TK> key)
        {
            return GetHashCode(key.AsObject);
        }

        public bool Equals(TK left, TK right)
        {
            return _innerComparer.Equals(left, right);
        }

        public int GetHashCode(TK key)
        {
            return _innerComparer.GetHashCode(key);
        }
    }
}