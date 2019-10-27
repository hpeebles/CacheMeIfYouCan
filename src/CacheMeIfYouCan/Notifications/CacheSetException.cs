using System;
using System.Collections.Generic;
using System.Linq;

namespace CacheMeIfYouCan.Notifications
{
    public sealed class CacheSetException<TK, TV> : CacheException<TK>
    {
        private readonly Lazy<IReadOnlyCollection<Key<TK>>> _keys;

        internal CacheSetException(
            string cacheName,
            string cacheType,
            IReadOnlyCollection<KeyValuePair<Key<TK>, TV>> values,
            TimeSpan timeToLive,
            string message,
            Exception exception)
            : base(cacheName, cacheType, new Lazy<IReadOnlyCollection<string>>(() => values.Select(kv => kv.Key.AsStringSafe).ToList()), message, exception)
        {
            Values = values;
            TimeToLive = timeToLive;
            _keys = new Lazy<IReadOnlyCollection<Key<TK>>>(() => values.Select(kv => kv.Key).ToList());
        }

        public IReadOnlyCollection<KeyValuePair<Key<TK>, TV>> Values { get; }
        public TimeSpan TimeToLive { get; }
        public override IReadOnlyCollection<Key<TK>> Keys => _keys.Value;
    }
}