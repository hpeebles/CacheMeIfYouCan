using System.IO;
using ProtoBuf;

namespace CacheMeIfYouCan.Serializers.ProtoBuf
{
    public sealed class ProtoBufSerializer<T> : IStreamSerializer<T>
    {
        public void WriteToStream(Stream stream, T obj)
        {
            Serializer.Serialize(stream, obj);
        }

        public T Deserialize(Stream stream)
        {
            return Serializer.Deserialize<T>(stream);
        }

        public T Deserialize(byte[] bytes)
        {
            using var stream = new MemoryStream(bytes);
            
            return Deserialize(stream);
        }
    }
}