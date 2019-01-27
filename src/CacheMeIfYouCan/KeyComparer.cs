using System.Collections.Generic;

namespace CacheMeIfYouCan
{
    public class KeyComparer<TK> : IEqualityComparer<Key<TK>>, IEqualityComparer<TK>
    {
        internal KeyComparer(IEqualityComparer<TK> innerComparer)
        {
            Inner = innerComparer;
        }

        public IEqualityComparer<TK> Inner { get; }

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
            return Inner.Equals(left, right);
        }

        public int GetHashCode(TK key)
        {
            return Inner.GetHashCode(key);
        }
    }
}