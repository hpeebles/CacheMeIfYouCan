using System;
using System.Collections.Generic;

namespace CacheMeIfYouCan.Serializers
{
    public class ValueSerializers
    {
        private readonly IDictionary<Type, object> _serializers = new Dictionary<Type, object>();
        private readonly IDictionary<Type, object> _deserializers = new Dictionary<Type, object>();
        private ISerializer _default;

        internal Func<T, string> GetSerializer<T>()
        {
            if (_serializers.TryGetValue(typeof(T), out var serializerObj) && serializerObj is Func<T, string> serializer)
                return serializer;

            if (_default != null)
                return x => _default.Serialize(x);

            return null;
        }

        internal Func<string, T> GetDeserializer<T>()
        {
            if (_deserializers.TryGetValue(typeof(T), out var deserializerObj) && deserializerObj is Func<string, T> deserializer)
                return deserializer;

            if (_default != null)
                return x => _default.Deserialize<T>(x);

            return null;
        }

        public ValueSerializers Set<T>(Func<T, string> serializer, Func<string, T> deserializer)
        {
            _serializers[typeof(T)] = serializer;
            _deserializers[typeof(T)] = deserializer;
            return this;
        }

        public ValueSerializers Set<T>(ISerializer serializer)
        {
            _serializers[typeof(T)] = (Func<T, string>)(serializer.Serialize);
            _deserializers[typeof(T)] = (Func<string, T>)(serializer.Deserialize<T>);
            return this;
        }

        public ValueSerializers Set<T>(ISerializer<T> serializer)
        {
            _serializers[typeof(T)] = (Func<T, string>)(serializer.Serialize);
            _deserializers[typeof(T)] = (Func<string, T>)(serializer.Deserialize);
            return this;
        }

        public ValueSerializers SetDefault(ISerializer serializer)
        {
            _default = serializer;
            return this;
        }
    }
}