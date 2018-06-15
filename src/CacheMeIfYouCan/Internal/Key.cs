namespace CacheMeIfYouCan.Internal
{
    internal struct Key<TK>
    {
        public readonly TK AsObject;
        public readonly string AsString;

        public Key(TK key, string keyString)
        {
            AsObject = key;
            AsString = keyString;
        }
    }
}