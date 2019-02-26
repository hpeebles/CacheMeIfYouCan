using System;
using System.IO;
using ProtoBuf;

namespace CacheMeIfYouCan.Serializers.Protobuf
{
    public sealed class ProtobufStringSerializer : ISerializer
    {
        public string Serialize<T>(T value)
        {
            using (var stream = new MemoryStream())
            {
                Serializer.Serialize(stream, value);
                return Convert.ToBase64String(stream.GetBuffer(), 0, (int)stream.Length);
            }
        }

        public T Deserialize<T>(string value)
        {
            using (var stream = new MemoryStream(Convert.FromBase64String(value)))
                return Serializer.Deserialize<T>(stream);
        }
    }
}