using System.Collections.Generic;

namespace CacheMeIfYouCan.Internal
{
    internal class GenericKeyComparer<TK> : IEqualityComparer<Key<TK>>
    {
        public bool Equals(Key<TK> left, Key<TK> right)
        {
            return left.AsObject.Equals(right.AsObject);
        }

        public int GetHashCode(Key<TK> key)
        {
            return key.AsObject.GetHashCode();
        }
    }
}