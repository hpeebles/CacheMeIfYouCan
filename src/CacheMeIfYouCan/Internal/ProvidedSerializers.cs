using System;
using System.Collections.Generic;

namespace CacheMeIfYouCan.Internal
{
    internal static class ProvidedSerializers
    {
        private static readonly IDictionary<Type, object> Serializers = new Dictionary<Type, object>
        {
            { typeof(string), (Func<string, string>) (x => x) },
            { typeof(char), (Func<char, string>) (x => x.ToString()) },
            { typeof(byte), (Func<byte, string>) (x => x.ToString()) },
            { typeof(sbyte), (Func<sbyte, string>) (x => x.ToString()) },
            { typeof(short), (Func<short, string>) (x => x.ToString()) },
            { typeof(ushort), (Func<ushort, string>) (x => x.ToString()) },
            { typeof(int), (Func<int, string>) (x => x.ToString()) },
            { typeof(uint), (Func<uint, string>) (x => x.ToString()) },
            { typeof(long), (Func<long, string>) (x => x.ToString()) },
            { typeof(ulong), (Func<ulong, string>) (x => x.ToString()) },
            { typeof(bool), (Func<bool, string>) (x => x.ToString()) }
        };
        
        private static readonly IDictionary<Type, object> Deserializers = new Dictionary<Type, object>
        {
            { typeof(string), (Func<string, string>) (x => x) },
            { typeof(char), (Func<string, char>) Char.Parse },
            { typeof(byte), (Func<string, byte>) Byte.Parse },
            { typeof(sbyte), (Func<string, sbyte>) SByte.Parse },
            { typeof(short), (Func<string, short>) Int16.Parse },
            { typeof(ushort), (Func<string, ushort>) UInt16.Parse },
            { typeof(int), (Func<string, int>) Int32.Parse },
            { typeof(uint), (Func<string, uint>) UInt32.Parse },
            { typeof(long), (Func<string, long>) Int64.Parse },
            { typeof(ulong), (Func<string, ulong>) UInt64.Parse },
            { typeof(bool), (Func<string, bool>) Boolean.Parse }
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