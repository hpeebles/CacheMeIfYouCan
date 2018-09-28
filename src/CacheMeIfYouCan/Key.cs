using System;

namespace CacheMeIfYouCan
{
    public struct Key<TK>
    {
        public readonly TK AsObject;
        public readonly Lazy<string> AsString;

        public Key(TK keyObj, Lazy<string> keyString)
        {
            AsObject = keyObj;
            AsString = keyString;
        }

        public Key(TK keyObj, string keyString)
            : this(keyObj, new Lazy<string>(() => keyString))
        { }

        public static implicit operator TK(Key<TK> key)
        {
            return key.AsObject;
        }

        public static implicit operator string(Key<TK> key)
        {
            return key.AsString.Value;
        }
    }
}