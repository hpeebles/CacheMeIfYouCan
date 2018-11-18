using System;
using System.Collections.Generic;
using System.Linq;

namespace CacheMeIfYouCan.Notifications
{
    public abstract class CacheException : Exception
    {
        public readonly string CacheName;
        public readonly string CacheType;
        public readonly long Timestamp;
        private readonly Lazy<ICollection<string>> _keys;

        internal CacheException(
            string cacheName,
            string cacheType,
            Lazy<ICollection<string>> keys,
            long timestamp,
            string message,
            Exception exception)
        : base(message, exception)
        {
            CacheName = cacheName;
            CacheType = cacheType;
            _keys = keys;
            Timestamp = timestamp;
        }

        public ICollection<string> Keys => _keys.Value;
    }

    public sealed class CacheException<TK> : CacheException
    {
        public new readonly ICollection<Key<TK>> Keys;

        internal CacheException(
            string cacheName,
            string cacheType,
            ICollection<Key<TK>> keys,
            long timestamp,
            string message,
            Exception exception)
            : base(cacheName, cacheType, new Lazy<ICollection<string>>(() => keys.Select(k => (string)k).ToArray()), timestamp, message, exception)
        {
            Keys = keys;
        }
    }
}