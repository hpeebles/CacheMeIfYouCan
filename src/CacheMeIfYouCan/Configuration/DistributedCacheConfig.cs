using System;
using CacheMeIfYouCan.Internal;

namespace CacheMeIfYouCan.Configuration
{
    public class DistributedCacheConfig<TK, TV> : CacheConfig<TK>, IDistributedCacheConfig<TK, TV>
    {
        public DistributedCacheConfig(string name = null, bool setDefaults = false)
            : base(name, setDefaults)
        {
            if (!setDefaults)
                return;

            if (DefaultSettings.Cache.KeySerializers.TryGetDeserializer<TK>(out var keyDeserializer) ||
                ProvidedSerializers.TryGetDeserializer(out keyDeserializer))
            {
                KeyDeserializer = keyDeserializer;
            }
            
            if (DefaultSettings.Cache.ValueSerializers.TryGetByteSerializer<TV>(out var valueByteSerializer))
            {
                ValueByteSerializer = valueByteSerializer;
            }
            else if (DefaultSettings.Cache.ValueSerializers.TryGetSerializer<TV>(out var valueSerializer) ||
                ProvidedSerializers.TryGetSerializer(out valueSerializer))
            {
                ValueSerializer = valueSerializer;
            }

            if (DefaultSettings.Cache.ValueSerializers.TryGetByteDeserializer<TV>(out var valueByteDeserializer))
            {
                ValueByteDeserializer = valueByteDeserializer;
            }
            else if (DefaultSettings.Cache.ValueSerializers.TryGetDeserializer<TV>(out var valueDeserializer) ||
                ProvidedSerializers.TryGetDeserializer(out valueDeserializer))
            {
                ValueDeserializer = valueDeserializer;
            }
        }

        public string KeyspacePrefix { get; set; }
        public Func<string, TK> KeyDeserializer { get; set; }
        public Func<TV, string> ValueSerializer { get; set; }
        public Func<string, TV> ValueDeserializer { get; set; }
        public Func<TV, byte[]> ValueByteSerializer { get; set; }
        public Func<byte[], TV> ValueByteDeserializer { get; set; }

        public void Validate()
        {
            if (CacheName == null)
                throw new ArgumentNullException(nameof(CacheName));

            if (KeySerializer == null)
                throw new ArgumentNullException(nameof(KeySerializer));

            if (ValueByteSerializer != null)
                ValueSerializer = null;

            if (ValueByteDeserializer != null)
                ValueDeserializer = null;

            var validValueSerializers =
                (ValueSerializer != null && ValueDeserializer != null) ||
                (ValueByteSerializer != null && ValueByteDeserializer != null);

            if (!validValueSerializers)
                throw new Exception("Value serializers are not valid. Both a serializer and a deserializer must be set");
        }
    }
}