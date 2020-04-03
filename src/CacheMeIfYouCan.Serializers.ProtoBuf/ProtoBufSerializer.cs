using System.IO;
using ProtoBuf;

namespace CacheMeIfYouCan.Serializers.ProtoBuf
{
    public sealed class ProtoBufSerializer<T> : ISerializer<T>
    {
        public void Serialize(Stream destination, T value)
        {
            Serializer.Serialize(destination, value);
        }

        public T Deserialize(Stream source)
        {
            return Serializer.Deserialize<T>(source);
        }

        public T Deserialize(byte[] bytes)
        {
            using var stream = new MemoryStream(bytes);
            
            return Deserialize(stream);
        }
    }
}