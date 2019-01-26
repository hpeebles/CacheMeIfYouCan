using System;
using CacheMeIfYouCan.Internal;

namespace CacheMeIfYouCan.Configuration
{
    public class DistributedCacheConfig<TK, TV>
    {
        public readonly string CacheName;
        public string KeyspacePrefix;
        public Func<string, TK> KeyDeserializer;
        public Func<TV, string> ValueSerializer;
        public Func<string, TV> ValueDeserializer;
        public KeyComparer<TK> KeyComparer;

        public DistributedCacheConfig(string cacheName = null)
        {
            CacheName = cacheName;
            
            if (DefaultSettings.Cache.KeySerializers.TryGetDeserializer<TK>(out var keyDeserializer) ||
                ProvidedSerializers.TryGetDeserializer(out keyDeserializer))
            {
                KeyDeserializer = keyDeserializer;
            }

            if (DefaultSettings.Cache.ValueSerializers.TryGetSerializer<TV>(out var valueSerializer) ||
                ProvidedSerializers.TryGetSerializer(out valueSerializer))
            {
                ValueSerializer = valueSerializer;
            }

            if (DefaultSettings.Cache.ValueSerializers.TryGetDeserializer<TV>(out var valueDeserializer) ||
                ProvidedSerializers.TryGetDeserializer(out valueDeserializer))
            {
                ValueDeserializer = valueDeserializer;
            }

            KeyComparer = KeyComparerResolver.Get<TK>(allowNull: true);
        }
    }
}