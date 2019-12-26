namespace CacheMeIfYouCan
{
    public readonly struct TupleKey<TOuterKey, TInnerKey>
    {
        public TupleKey(TOuterKey outerKey, TInnerKey innerKey, int hashCode)
        {
            OuterKey = outerKey;
            InnerKey = innerKey;
            HashCode = hashCode;
        }
        
        public TOuterKey OuterKey { get; }
        public TInnerKey InnerKey { get; }
        public int HashCode { get; }
    }
}