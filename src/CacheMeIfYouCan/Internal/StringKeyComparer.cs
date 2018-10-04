using System.Collections.Generic;

namespace CacheMeIfYouCan.Internal
{
    public class StringKeyComparer<TK> : IEqualityComparer<Key<TK>>
    {
        public bool Equals(Key<TK> left, Key<TK> right)
        {
            return left.AsString.Equals(right.AsString);
        }

        public int GetHashCode(Key<TK> key)
        {
            return key.AsString.GetHashCode();
        }
    }
}