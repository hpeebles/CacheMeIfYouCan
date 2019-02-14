using System;
using System.Collections.Generic;

namespace CacheMeIfYouCan.Serializers
{
    public class ValueSerializers
    {
        private readonly Dictionary<Type, object> _serializers;
        private readonly Dictionary<Type, object> _deserializers;
        private Func<Type, ISerializer> _defaultSerializerFactory;

        internal ValueSerializers()
        {
            _serializers = new Dictionary<Type, object>();
            _deserializers = new Dictionary<Type, object>();
        }
        
        internal bool TryGetSerializer<T>(out Func<T, string> serializer)
        {
            if (_serializers.TryGetValue(typeof(T), out var serializerObj))
            {
                serializer = (Func<T, string>)serializerObj;
            }
            else if (_defaultSerializerFactory != null)
            {
                var defaultSerializer = _defaultSerializerFactory(typeof(T));
                serializer = defaultSerializer.Serialize;
            }
            else
            {
                serializer = null;
            }

            return serializer != null;
        }

        internal bool TryGetDeserializer<T>(out Func<string, T> deserializer)
        {
            if (_deserializers.TryGetValue(typeof(T), out var deserializerObj))
            {
                deserializer = (Func<string, T>)deserializerObj;
            }
            else if (_defaultSerializerFactory != null)
            {
                var defaultDeserializer = _defaultSerializerFactory(typeof(T));
                deserializer = defaultDeserializer.Deserialize<T>;
            }
            else
            {
                deserializer = null;
            }

            return deserializer != null;
        }

        public ValueSerializers Set<T>(Func<T, string> serializer, Func<string, T> deserializer)
        {
            _serializers[typeof(T)] = serializer;
            _deserializers[typeof(T)] = deserializer;
            return this;
        }

        public ValueSerializers Set<T>(ISerializer serializer)
        {
            _serializers[typeof(T)] = (Func<T, string>)serializer.Serialize;
            _deserializers[typeof(T)] = (Func<string, T>)serializer.Deserialize<T>;
            return this;
        }

        public ValueSerializers Set<T>(ISerializer<T> serializer)
        {
            _serializers[typeof(T)] = (Func<T, string>)serializer.Serialize;
            _deserializers[typeof(T)] = (Func<string, T>)serializer.Deserialize;
            return this;
        }

        public ValueSerializers SetDefault(ISerializer serializer)
        {
            _defaultSerializerFactory = t => serializer;
            return this;
        }

        public ValueSerializers SetDefaultFactory(Func<Type, ISerializer> serializerFactory)
        {
            _defaultSerializerFactory = serializerFactory;
            return this;
        }
    }
}