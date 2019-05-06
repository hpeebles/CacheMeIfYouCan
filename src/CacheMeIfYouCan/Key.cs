using System;

namespace CacheMeIfYouCan
{
    public readonly struct Key<TK>
    {
        private readonly Lazy<string> _asString;
        private readonly bool _canSerialize;

        public Key(TK keyObj, Func<TK, string> serializer)
        {
            AsObject = keyObj;
            _asString = new Lazy<string>(() => serializer(keyObj));
            _canSerialize = serializer != null;
        }

        public Key(TK keyObj, string keyString)
            : this(keyObj, k => keyString)
        { }

        public TK AsObject { get; }

        public string AsString => _asString.Value;

        public string AsStringSafe => _canSerialize ? _asString.Value : AsObject.ToString();

        public static implicit operator TK(in Key<TK> key)
        {
            return key.AsObject;
        }
    }
}