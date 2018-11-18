using System;
using System.Collections.Generic;

namespace CacheMeIfYouCan.Serializers
{
    public class KeySerializers
    {
        private readonly IDictionary<Type, object> _serializers = new Dictionary<Type, object>();
        private readonly IDictionary<Type, object> _deserializers = new Dictionary<Type, object>();
        private ISerializer _default;

        internal bool TryGetSerializer<T>(out Func<T, string> serializer)
        {
            if (_serializers.TryGetValue(typeof(T), out var serializerObj))
                serializer = (Func<T, string>)serializerObj;
            else if (_default != null)
                serializer = _default.Serialize;
            else
                serializer = null;

            return serializer != null;
        }

        internal bool TryGetDeserializer<T>(out Func<string, T> deserializer)
        {
            if (_deserializers.TryGetValue(typeof(T), out var deserializerObj))
                deserializer = (Func<string, T>)deserializerObj;
            else if (_default != null)
                deserializer = _default.Deserialize<T>;
            else
                deserializer = null;

            return deserializer != null;
        }

        public KeySerializers Set<T>(Func<T, string> serializer, Func<string, T> deserializer = null)
        {
            _serializers[typeof(T)] = serializer;
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
            _default = serializer;
            return this;
        }

        public KeySerializers SetDefault(Func<object, string> serializer, Func<string, object> deserializer = null)
        {
            _default = new Wrapper(serializer, deserializer);
            return this;
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