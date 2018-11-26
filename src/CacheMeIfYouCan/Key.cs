using System;
using System.Threading;

namespace CacheMeIfYouCan
{
    public struct Key<TK>
    {
        private readonly Func<TK, string> _serializer;
        private string _asString;

        public Key(TK keyObj, Func<TK, string> serializer)
        {
            _serializer = serializer;
            _asString = null;
            
            AsObject = keyObj;
        }

        public Key(TK keyObj, string keyString)
            : this(keyObj, k => keyString)
        { }

        public TK AsObject { get; }
        
        public string AsString
        {
            get
            {
                if (_asString != null)
                    return _asString;

                _asString = _serializer(AsObject);
                return _asString;
            }
        }

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