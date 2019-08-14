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
        
        private Func<TV, string> _valueSerializer;
        public Func<TV, string> ValueSerializer
        {
            get => _valueSerializer;
            set
            {
                if (value != null)
                    _valueByteSerializer = null;
                
                _valueSerializer = value;
            }
        }

        private Func<string, TV> _valueDeserializer;
        public Func<string, TV> ValueDeserializer
        {
            get => _valueDeserializer;
            set
            {
                if (value != null)
                    _valueByteDeserializer = null;
                
                _valueDeserializer = value;
            }
        }
        
        private Func<TV, byte[]> _valueByteSerializer;
        public Func<TV, byte[]> ValueByteSerializer
        {
            get => _valueByteSerializer;
            set
            {
                if (value != null)
                    _valueSerializer = null;
                
                _valueByteSerializer = value;
            }
        }
        
        private Func<byte[], TV> _valueByteDeserializer;
        public Func<byte[], TV> ValueByteDeserializer
        {
            get => _valueByteDeserializer;
            set
            {
                if (value != null)
                    _valueDeserializer = null;
                
                _valueByteDeserializer = value;
            }
        }
        
        public bool HasValidValueStringSerializer => _valueSerializer != null && _valueDeserializer != null;
        public bool HasValidValueByteSerializer => _valueByteSerializer != null && _valueByteDeserializer != null;

        public void Validate()
        {
            if (CacheName == null)
                throw new ArgumentNullException(nameof(CacheName));

            if (KeySerializer == null)
                throw new ArgumentNullException(nameof(KeySerializer));

            var validValueSerializers = HasValidValueStringSerializer || HasValidValueByteSerializer;

            if (!validValueSerializers)
                throw new Exception("Value serializers are not valid. Both a serializer and a deserializer must be set");
        }
    }
}