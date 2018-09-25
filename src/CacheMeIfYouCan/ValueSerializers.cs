using System;
using System.Collections.Generic;
using CacheMeIfYouCan.Internal;

namespace CacheMeIfYouCan
{
    public class ValueSerializers
    {
        private readonly IDictionary<Type, object> _serializers = new Dictionary<Type, object>();
        private readonly IDictionary<Type, object> _deserializers = new Dictionary<Type, object>();
        private ISerializer _default;

        public Func<T, string> GetSerializer<T>()
        {
            if (_serializers.TryGetValue(typeof(T), out var serializerObj) && serializerObj is Func<T, string> serializer)
                return serializer;

            if (_default != null)
                return x => _default.Serialize(x);

            if (ProvidedSerializers.TryGetSerializer<T>(out serializer))
                return serializer;

            throw new Exception($"No serializer defined for type '{typeof(T).FullName}'");
        }

        public Func<string, T> GetDeserializer<T>()
        {
            if (_deserializers.TryGetValue(typeof(T), out var deserializerObj) && deserializerObj is Func<string, T> deserializer)
                return deserializer;

            if (_default != null)
                return x => _default.Deserialize<T>(x);

            if (ProvidedSerializers.TryGetDeserializer<T>(out deserializer))
                return deserializer;
            
            throw new Exception($"No serializer defined for type '{typeof(T).FullName}'");
        }

        public void Set<T>(Func<T, string> serializer, Func<string, T> deserializer)
        {
            _serializers[typeof(T)] = serializer;
            _deserializers[typeof(T)] = deserializer;
        }

        public void Set<T>(ISerializer serializer)
        {
            _serializers[typeof(T)] = (Func<T, string>)(serializer.Serialize);
            _deserializers[typeof(T)] = (Func<string, T>)(serializer.Deserialize<T>);
        }

        public void Set<T>(ISerializer<T> serializer)
        {
            _serializers[typeof(T)] = (Func<T, string>)(serializer.Serialize);
            _deserializers[typeof(T)] = (Func<string, T>)(serializer.Deserialize);
        }

        public void SetDefault(ISerializer serializer) => _default = serializer;
    }
}