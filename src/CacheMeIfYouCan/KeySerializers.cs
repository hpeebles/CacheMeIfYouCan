using System;
using System.Collections.Generic;
using CacheMeIfYouCan.Internal;

namespace CacheMeIfYouCan
{
    public class KeySerializers
    {
        private readonly IDictionary<Type, object> _serializers = new Dictionary<Type, object>();
        private IKeySerializer _default;

        public Func<T, string> Get<T>()
        {
            if (_serializers.TryGetValue(typeof(T), out var serializerObj) && serializerObj is Func<T, string> serializer)
                return serializer;

            if (_default != null)
                return x => _default.Serialize(x);

            if (ProvidedSerializers.TryGetSerializer<T>(out serializer))
                return serializer;
            
            throw new Exception($"No key serializer defined for type '{typeof(T).FullName}'");
        }

        public void Set<T>(Func<T, string> serializer) => _serializers[typeof(T)] = serializer;
        public void Set<T>(IKeySerializer serializer) => _serializers[typeof(T)] = (Func<T, string>)(serializer.Serialize);
        public void Set<T>(IKeySerializer<T> serializer) => _serializers[typeof(T)] = (Func<T, string>)(serializer.Serialize);
        public void SetDefault(IKeySerializer serializer) => _default = serializer;
        public void SetDefault(Func<object, string> serializer) => _default = new Wrapper(serializer);
        
        private class Wrapper : IKeySerializer
        {
            private readonly Func<object, string> _serializer;

            public Wrapper(Func<object, string> serializer)
            {
                _serializer = serializer;
            }

            public string Serialize<T>(T value)
            {
                return _serializer(value);
            }
        }
    }
}