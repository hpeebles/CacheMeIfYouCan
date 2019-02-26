using System;
using System.Collections.Generic;
using System.Linq;

namespace CacheMeIfYouCan.Notifications
{
    public sealed class CacheGetException<TK> : CacheException<TK>
    {
        internal CacheGetException(
            string cacheName,
            string cacheType,
            ICollection<Key<TK>> keys,
            string message,
            Exception exception)
            : base(cacheName, cacheType, new Lazy<IList<string>>(() => keys.Select(k => k.AsStringSafe).ToArray()), message, exception)
        {
            Keys = keys;
        }

        public override ICollection<Key<TK>> Keys { get; }
    }
}