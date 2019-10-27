using System;
using System.Collections.Generic;

namespace CacheMeIfYouCan.Notifications
{
    public abstract class CacheException<TK> : CacheException
    {
        internal CacheException(
            string cacheName,
            string cacheType,
            Lazy<IReadOnlyCollection<string>> keysAsStrings,
            string message,
            Exception exception)
            : base(cacheName, cacheType, keysAsStrings, message, exception)
        { }
        
        public new abstract IReadOnlyCollection<Key<TK>> Keys { get; }
    }
    
    public abstract class CacheException : Exception
    {
        private readonly Lazy<IReadOnlyCollection<string>> _keys;

        internal CacheException(
            string cacheName,
            string cacheType,
            Lazy<IReadOnlyCollection<string>> keys,
            string message,
            Exception exception)
            : base(message, exception)
        {
            CacheName = cacheName;
            CacheType = cacheType;
            _keys = keys;
        }

        public string CacheName { get; }
        public string CacheType { get; }
        public IReadOnlyCollection<string> Keys => _keys.Value;
    }
}