using System.Collections.Generic;

namespace CacheMeIfYouCan.Internal
{
    public class StringKeyComparer<TK> : IEqualityComparer<Key<TK>>
    {
        public bool Equals(Key<TK> left, Key<TK> right)
        {
            return left.AsString.Value.Equals(right.AsString.Value);
        }

        public int GetHashCode(Key<TK> key)
        {
            return key.AsString.Value.GetHashCode();
        }
    }
}