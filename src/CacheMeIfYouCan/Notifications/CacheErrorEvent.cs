using System;
using System.Collections.Generic;
using System.Linq;

namespace CacheMeIfYouCan.Notifications
{
    public abstract class CacheErrorEvent
    {
        public readonly string CacheName;
        public readonly string CacheType;
        public readonly long Timestamp;
        public readonly string Message;
        public readonly Exception Exception;
        private readonly Lazy<ICollection<string>> _keys;

        internal CacheErrorEvent(
            string cacheName,
            string cacheType,
            Lazy<ICollection<string>> keys,
            long timestamp,
            string message,
            Exception exception)
        {
            CacheName = cacheName;
            CacheType = cacheType;
            _keys = keys;
            Timestamp = timestamp;
            Message = message;
            Exception = exception;
        }

        public ICollection<string> Keys => _keys.Value;
    }

    public sealed class CacheErrorEvent<TK> : CacheErrorEvent
    {
        public new readonly ICollection<Key<TK>> Keys;

        internal CacheErrorEvent(
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