using System;
using System.Collections.Generic;

namespace CacheMeIfYouCan
{
    public class Serializers
    {
        private readonly IDictionary<Type, object> _serializers = new Dictionary<Type, object>
        {
            { typeof(string), (Func<string, string>) (x => x) }
        };
        
        private readonly IDictionary<Type, object> _deserializers = new Dictionary<Type, object>
        {
            { typeof(string), (Func<string, string>) (x => x) }
        };

        private ISerializer _default;

        public Func<T, string> GetSerializer<T>()
        {
            if (_serializers.TryGetValue(typeof(T), out var serializerObj) && serializerObj is Func<T, string> serializer)
                return serializer;

            if (_default != null)
                return x => _default.Serialize(x);

            return null;
        }

        public Func<string, T> GetDeserializer<T>()
        {
            if (_deserializers.TryGetValue(typeof(T), out var deserializerObj) && deserializerObj is Func<string, T> deserializer)
                return deserializer;

            if (_default != null)
                return x => _default.Deserialize<T>(x);

            return null;
        }

        public void Set<T>(Func<T, string> serializer, Func<string, T> deserializer = null)
        {
            _serializers[typeof(T)] = serializer;

            if (deserializer != null)
                _deserializers[typeof(T)] = deserializer;
        }

        public void Set<T>(ISerializer serializer)
        {
            _serializers[typeof(T)] = (Func<T, string>)(serializer.Serialize);
            _deserializers[typeof(T)] = (Func<string, T>)(serializer.Deserialize<T>);
        }

        public void SetDefault(ISerializer serializer) => _default = serializer;
    }
}