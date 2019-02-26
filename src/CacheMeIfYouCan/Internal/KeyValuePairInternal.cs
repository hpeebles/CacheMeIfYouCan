namespace CacheMeIfYouCan.Internal
{
    internal readonly struct KeyValuePairInternal<TK, TV> : IKeyValuePair<TK, TV>
    {
        public KeyValuePairInternal(TK key, TV value)
        {
            Key = key;
            Value = value;
        }
        
        public TK Key { get; }
        public TV Value { get; }
    }
}