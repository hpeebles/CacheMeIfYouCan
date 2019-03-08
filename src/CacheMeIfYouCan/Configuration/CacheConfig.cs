using System;
using CacheMeIfYouCan.Internal;

namespace CacheMeIfYouCan.Configuration
{
    public abstract class CacheConfig<TK> : ILocalCacheConfig<TK>
    {
        protected CacheConfig(string cacheName = null, bool setDefaults = false)
        {
            CacheName = cacheName;

            if (!setDefaults)
                return;
            
            if (DefaultSettings.Cache.KeySerializers.TryGetSerializer<TK>(out var keySerializer) ||
                ProvidedSerializers.TryGetSerializer(out keySerializer))
            {
                KeySerializer = keySerializer;
            }
            
            KeyComparer = KeyComparerResolver.Get<TK>();
        }

        public string CacheName { get; set; }
        public Func<TK, string> KeySerializer { get; set; }
        public KeyComparer<TK> KeyComparer { get; set; }
    }
}