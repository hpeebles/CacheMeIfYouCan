using System.IO;
using ProtoBuf;

namespace CacheMeIfYouCan.Serializers.Protobuf
{
    public class ProtobufSerializer : IByteSerializer
    {
        public byte[] Serialize<T>(T value)
        {
            using (var stream = new MemoryStream())
            {
                Serializer.Serialize(stream, value);
                return stream.ToArray();
            }
        }

        public T Deserialize<T>(byte[] value)
        {
            using (var stream = new MemoryStream(value))
                return Serializer.Deserialize<T>(stream);
        }
    }
}