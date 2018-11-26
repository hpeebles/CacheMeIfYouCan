﻿using System;
using System.Threading;

namespace CacheMeIfYouCan
{
    public readonly struct Key<TK>
    {
        private readonly Lazy<string> _asString;
        public TK AsObject { get; }

        public Key(TK keyObj, Func<TK, string> serializer)
        {
            AsObject = keyObj;
            _asString = new Lazy<string>(() => serializer(keyObj), LazyThreadSafetyMode.None);
        }

        public Key(TK keyObj, string keyString)
            : this(keyObj, k => keyString)
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