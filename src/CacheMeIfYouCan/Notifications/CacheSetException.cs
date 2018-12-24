using System;
using System.Collections.Generic;
using System.Linq;

namespace CacheMeIfYouCan.Notifications
{
    public sealed class CacheSetException<TK, TV> : CacheException<TK>
    {
        private readonly Lazy<IList<Key<TK>>> _keys;

        internal CacheSetException(
            string cacheName,
            string cacheType,
            ICollection<KeyValuePair<Key<TK>, TV>> values,
            TimeSpan timeToLive,
            long timestamp,
            string message,
            Exception exception)
            : base(cacheName, cacheType, new Lazy<IList<string>>(() => values.Select(kv => kv.Key.AsStringSafe).ToArray()), timestamp, message, exception)
        {
            Values = values;
            TimeToLive = timeToLive;
            _keys = new Lazy<IList<Key<TK>>>(() => values.Select(kv => kv.Key).ToArray());
        }

        public ICollection<KeyValuePair<Key<TK>, TV>> Values { get; }
        public TimeSpan TimeToLive { get; }
        public override ICollection<Key<TK>> Keys => _keys.Value;
    }
}