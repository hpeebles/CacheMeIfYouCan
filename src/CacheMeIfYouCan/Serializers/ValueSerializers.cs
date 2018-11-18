using System;
using System.Collections.Generic;

namespace CacheMeIfYouCan.Serializers
{
    public class ValueSerializers
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