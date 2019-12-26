using System.Collections.Generic;

namespace CacheMeIfYouCan
{
    public sealed class TupleKeyComparer<TOuterKey, TInnerKey> : IEqualityComparer<TupleKey<TOuterKey, TInnerKey>>
    {
        private readonly IEqualityComparer<TOuterKey> _outerKeyComparer;
        private readonly IEqualityComparer<TInnerKey> _innerKeyComparer;

        public TupleKeyComparer(
            IEqualityComparer<TOuterKey> outerKeyComparer,
            IEqualityComparer<TInnerKey> innerKeyComparer)
        {
            _outerKeyComparer = outerKeyComparer;
            _innerKeyComparer = innerKeyComparer;
        }
            
        public bool Equals(TupleKey<TOuterKey, TInnerKey> x, TupleKey<TOuterKey, TInnerKey> y)
        {
            return
                _outerKeyComparer.Equals(x.OuterKey, y.OuterKey) &&
                _innerKeyComparer.Equals(x.InnerKey, y.InnerKey);
        }

        public int GetHashCode(TupleKey<TOuterKey, TInnerKey> obj)
        {
            return obj.HashCode;
        }
    }
}