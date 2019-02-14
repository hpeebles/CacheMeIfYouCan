using System;
using System.Collections.Generic;
using System.Linq;

namespace CacheMeIfYouCan.Serializers
{
    public class KeySerializers
    {
        private readonly Dictionary<Type, object> _serializers;
        private readonly Dictionary<Type, object> _deserializers;
        private Func<Type, ISerializer> _defaultSerializerFactory;

        internal KeySerializers()
        {
            _serializers = new Dictionary<Type, object>();
            _deserializers = new Dictionary<Type, object>();
        }

        private KeySerializers(
            Dictionary<Type, object> serializers,
            Dictionary<Type, object> deserializers,
            Func<Type, ISerializer> defaultSerializerFactory)
        {
            _serializers = serializers;
            _deserializers = deserializers;
            _defaultSerializerFactory = defaultSerializerFactory;
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

        public KeySerializers Set<T>(Func<T, string> serializer, Func<string, T> deserializer = null)
        {
            _serializers[typeof(T)] = serializer;
            _deserializers[typeof(T)] = deserializer;
            return this;
        }

        public KeySerializers Set<T>(ISerializer serializer)
        {
            _serializers[typeof(T)] = (Func<T, string>)serializer.Serialize;
            _deserializers[typeof(T)] = (Func<string, T>)serializer.Deserialize<T>;
            return this;
        }

        public KeySerializers Set<T>(ISerializer<T> serializer)
        {
            _serializers[typeof(T)] = (Func<T, string>)serializer.Serialize;
            _deserializers[typeof(T)] = (Func<string, T>)serializer.Deserialize;
            return this;
        }

        public KeySerializers SetDefault(ISerializer serializer)
        {
            _defaultSerializerFactory = t => serializer;
            return this;
        }

        public KeySerializers SetDefault(Func<object, string> serializer, Func<string, object> deserializer = null)
        {
            _defaultSerializerFactory = t => new Wrapper(serializer, deserializer);
            return this;
        }

        public KeySerializers SetDefaultFactory(Func<Type, ISerializer> serializerFactory)
        {
            _defaultSerializerFactory = serializerFactory;
            return this;
        }

        internal KeySerializers Clone()
        {
            var serializersClone = _serializers.ToDictionary(kv => kv.Key, kv => kv.Value);
            var deserializersClone = _deserializers.ToDictionary(kv => kv.Key, kv => kv.Value);
            
            return new KeySerializers(serializersClone, deserializersClone, _defaultSerializerFactory);
        }
        
        private class Wrapper : ISerializer
        {
            private readonly Func<object, string> _serializer;
            private readonly Func<string, object> _deserializer;

            public Wrapper(Func<object, string> serializer, Func<string, object> deserializer)
            {
                _serializer = serializer;
                _deserializer = deserializer;
            }

            public string Serialize<T>(T value)
            {
                return _serializer(value);
            }

            public T Deserialize<T>(string value)
            {
                if (_deserializer == null)
                    throw new Exception($"No deserializer defined for type '{typeof(T).FullName}'");

                return (T)_deserializer(value);
            }
        }
    }
}