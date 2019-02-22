using System;
using CacheMeIfYouCan.Internal;

namespace CacheMeIfYouCan.Configuration
{
    public class DistributedCacheConfig<TK, TV>
    {
        public readonly string CacheName;
        public string KeyspacePrefix;
        public Func<TK, string> KeySerializer;
        public Func<string, TK> KeyDeserializer;
        public Func<TV, string> ValueSerializer;
        public Func<string, TV> ValueDeserializer;
        public Func<TV, byte[]> ValueByteSerializer;
        public Func<byte[], TV> ValueByteDeserializer;
        public KeyComparer<TK> KeyComparer;

        public DistributedCacheConfig(string cacheName = null, bool setDefaults = false)
        {
            CacheName = cacheName;

            if (!setDefaults)
                return;
            
            if (DefaultSettings.Cache.KeySerializers.TryGetSerializer<TK>(out var keySerializer) ||
                ProvidedSerializers.TryGetSerializer(out keySerializer))
            {
                KeySerializer = keySerializer;
            }
            
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

            KeyComparer = KeyComparerResolver.Get<TK>(allowNull: true);
        }

        public void Validate()
        {
            if (CacheName == null)
                throw new ArgumentNullException(nameof(CacheName));
            
            if (ValueSerializer == null)
                throw new ArgumentNullException(nameof(ValueSerializer));
            
            if (ValueDeserializer == null)
                throw new ArgumentNullException(nameof(ValueDeserializer));
            
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