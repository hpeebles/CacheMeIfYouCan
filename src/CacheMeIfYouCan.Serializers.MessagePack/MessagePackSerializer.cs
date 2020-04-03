using System.IO;
using MessagePack;

namespace CacheMeIfYouCan.Serializers.MessagePack
{
    public sealed class MessagePackSerializer<T> : ISerializer<T>
    {
        private readonly MessagePackSerializerOptions _options;

        public MessagePackSerializer(MessagePackSerializerOptions options = null)
        {
            _options = options;
        }

        public void Serialize(Stream destination, T value)
        {
            MessagePackSerializer.Serialize(destination, value, _options);
        }

        public T Deserialize(Stream source)
        {
            return MessagePackSerializer.Deserialize<T>(source, _options);
        }

        public T Deserialize(byte[] bytes)
        {
            return MessagePackSerializer.Deserialize<T>(bytes, _options);
        }
    }
}