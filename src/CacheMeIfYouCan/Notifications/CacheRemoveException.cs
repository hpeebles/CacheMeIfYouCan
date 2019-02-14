using System;
using System.Collections.Generic;

namespace CacheMeIfYouCan.Notifications
{
    public sealed class CacheRemoveException<TK> : CacheException<TK>
    {
        internal CacheRemoveException(
            string cacheName,
            string cacheType,
            Key<TK> key,
            long timestamp,
            string message,
            Exception exception)
            : base(
                cacheName,
                cacheType,
                new Lazy<IList<string>>(() => new[] { key.AsStringSafe }),
                timestamp,
                message,
                exception)
        {
            Keys = new[] { key };
        }

        public override ICollection<Key<TK>> Keys { get; }
    }
}