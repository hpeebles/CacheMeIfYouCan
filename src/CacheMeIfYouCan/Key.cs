using System;

namespace CacheMeIfYouCan
{
    public readonly struct Key<TK>
    {
        private readonly Lazy<string> _asString;
        public TK AsObject { get; }

        public Key(TK keyObj, Lazy<string> keyString)
        {
            AsObject = keyObj;
            _asString = keyString;
        }

        public Key(TK keyObj, string keyString)
            : this(keyObj, new Lazy<string>(() => keyString))
        { }
        
        public string AsString => _asString.Value;

        public static implicit operator TK(Key<TK> key)
        {
            return key.AsObject;
        }

        public static implicit operator string(Key<TK> key)
        {
            return key.AsString;
        }
    }
}