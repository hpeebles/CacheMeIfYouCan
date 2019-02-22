using System;
using System.Collections.Generic;

namespace CacheMeIfYouCan.Serializers
{
    public class ValueSerializers
    {
        private readonly Dictionary<Type, object> _serializers;
        private readonly Dictionary<Type, object> _deserializers;
        private Func<Type, ISerializer> _defaultSerializerFactory;
        private Func<Type, IByteSerializer> _defaultByteSerializerFactory;

        internal ValueSerializers()
        {
            _serializers = new Dictionary<Type, object>();
            _deserializers = new Dictionary<Type, object>();
        }
        
        internal bool TryGetSerializer<T>(out Func<T, string> serializer)
        {
            if (_serializers.TryGetValue(typeof(T), out var serializerObj))
            {
                if (serializerObj is Func<T, string> s)
                    serializer = s;
                else
                    serializer = null;
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
                if (deserializerObj is Func<string, T> d)
                    deserializer = d;
                else
                    deserializer = null;
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
        
        internal bool TryGetByteSerializer<T>(out Func<T, byte[]> serializer)
        {
            if (_serializers.TryGetValue(typeof(T), out var serializerObj))
            {
                if (serializerObj is Func<T, byte[]> s)
                    serializer = s;
                else
                    serializer = null;
            }
            else if (_defaultByteSerializerFactory != null)
            {
                var defaultSerializer = _defaultByteSerializerFactory(typeof(T));
                serializer = defaultSerializer.Serialize;
            }
            else
            {
                serializer = null;
            }

            return serializer != null;
        }

        internal bool TryGetByteDeserializer<T>(out Func<byte[], T> deserializer)
        {
            if (_deserializers.TryGetValue(typeof(T), out var deserializerObj))
            {
                if (deserializerObj is Func<byte[], T> d)
                    deserializer = d;
                else
                    deserializer = null;
            }
            else if (_defaultByteSerializerFactory != null)
            {
                var defaultDeserializer = _defaultByteSerializerFactory(typeof(T));
                deserializer = defaultDeserializer.Deserialize<T>;
            }
            else
            {
                deserializer = null;
            }

            return deserializer != null;
        }

        public ValueSerializers Set<T>(ISerializer serializer)
        {
            return Set(serializer.Serialize, serializer.Deserialize<T>);
        }

        public ValueSerializers Set<T>(ISerializer<T> serializer)
        {
            return Set(serializer.Serialize, serializer.Deserialize);
        }

        public ValueSerializers Set<T>(Func<T, string> serializer, Func<string, T> deserializer)
        {
            _serializers[typeof(T)] = serializer;
            _deserializers[typeof(T)] = deserializer;
            return this;
        }

        public ValueSerializers SetDefault(ISerializer serializer)
        {
            return SetDefaultFactory(t => serializer);
        }

        public ValueSerializers SetDefaultFactory(Func<Type, ISerializer> serializerFactory)
        {
            _defaultSerializerFactory = serializerFactory;
            _defaultByteSerializerFactory = null;
            return this;
        }

        public ValueSerializers Set<T>(IByteSerializer serializer)
        {
            return Set(serializer.Serialize, serializer.Deserialize<T>);
        }
        
        public ValueSerializers Set<T>(IByteSerializer<T> serializer)
        {
            return Set(serializer.Serialize, serializer.Deserialize);
        }
        
        public ValueSerializers Set<T>(Func<T, byte[]> serializer, Func<byte[], T> deserializer)
        {
            _serializers[typeof(T)] = serializer;
            _deserializers[typeof(T)] = deserializer;
            return this;
        }

        public ValueSerializers SetDefault(IByteSerializer serializer)
        {
            return SetDefaultFactory(t => serializer);
        }

        public ValueSerializers SetDefaultFactory(Func<Type, IByteSerializer> serializerFactory)
        {
            _defaultByteSerializerFactory = serializerFactory;
            _defaultSerializerFactory = null;
            return this;
        }
    }
}