using System;
using CacheMeIfYouCan.Internal;

namespace CacheMeIfYouCan.Configuration
{
    public class CacheFactoryConfig<TK, TV>
    {
        public string KeyspacePrefix;
        public Func<string, TK> KeyDeserializer;
        public Func<TV, string> ValueSerializer;
        public Func<string, TV> ValueDeserializer;

        public CacheFactoryConfig()
        {
            if (DefaultCacheConfig.Configuration.KeySerializers.TryGetDeserializer<TK>(out var keyDeserializer) ||
                ProvidedSerializers.TryGetDeserializer<TK>(out keyDeserializer))
            {
                KeyDeserializer = keyDeserializer;
            }

            if (DefaultCacheConfig.Configuration.ValueSerializers.TryGetSerializer<TV>(out var valueSerializer) ||
                ProvidedSerializers.TryGetSerializer<TV>(out valueSerializer))
            {
                ValueSerializer = valueSerializer;
            }

            if (DefaultCacheConfig.Configuration.ValueSerializers.TryGetDeserializer<TV>(out var valueDeserializer) ||
                ProvidedSerializers.TryGetDeserializer<TV>(out valueDeserializer))
            {
                ValueDeserializer = valueDeserializer;
            }
        }
    }
}