using System;
using System.Collections.Generic;

namespace CacheMeIfYouCan.Internal
{
    internal static class ProvidedSerializers
    {
        private static readonly IDictionary<Type, object> Serializers = new Dictionary<Type, object>
        {
            { typeof(string), (Func<string, string>) (x => x) },
            { typeof(byte), (Func<byte, string>) (x => x.ToString()) },
            { typeof(short), (Func<short, string>) (x => x.ToString()) },
            { typeof(int), (Func<int, string>) (x => x.ToString()) },
            { typeof(long), (Func<long, string>) (x => x.ToString()) }
        };
        
        
        private static readonly IDictionary<Type, object> Deserializers = new Dictionary<Type, object>
        {
            { typeof(string), (Func<string, string>) (x => x) },
            { typeof(byte), (Func<string, byte>) (Byte.Parse) },
            { typeof(short), (Func<string, short>) (Int16.Parse) },
            { typeof(int), (Func<string, int>) (Int32.Parse) },
            { typeof(long), (Func<string, long>) (Int64.Parse) }
        };

        public static bool TryGetSerializer<T>(out Func<T, string> serializer)
        {
            if (Serializers.TryGetValue(typeof(T), out var serializerObj))
                serializer = (Func<T, string>) serializerObj;
            else
                serializer = null;

            return serializer != null;
        }
        
        public static bool TryGetDeserializer<T>(out Func<string, T> deserializer)
        {
            if (Deserializers.TryGetValue(typeof(T), out var deserializerObj))
                deserializer = (Func<string, T>) deserializerObj;
            else
                deserializer = null;

            return deserializer != null;
        }
    }
}