using System;
using System.Collections.Generic;

namespace CacheMeIfYouCan.Serializers
{
    public class KeySerializers
    {
        private readonly IDictionary<Type, object> _serializers = new Dictionary<Type, object>();
        private readonly IDictionary<Type, object> _deserializers = new Dictionary<Type, object>();
        private ISerializer _default;

        internal Func<T, string> GetSerializer<T>()
        {
            if (_serializers.TryGetValue(typeof(T), out var serializerObj))
                return (Func<T, string>)serializerObj;

            if (_default != null)
                return _default.Serialize;

            return null;
        }

        internal Func<string, T> GetDeserializer<T>()
        {
            if (_deserializers.TryGetValue(typeof(T), out var deserializerObj))
            {
                if (deserializerObj == null)
                    throw new Exception($"No deserializer defined for type '{typeof(T).FullName}'");

                return (Func<string, T>) deserializerObj;
            }

            if (_default != null)
                return _default.Deserialize<T>;

            return null;
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