using MsgPack.Serialization;

namespace CacheMeIfYouCan.Serializers.MessagePack
{
    public sealed class MessagePackSerializer : IByteSerializer
    {
        public byte[] Serialize<T>(T value)
        {
            var serializer = GetSerializer<T>();

            return serializer.PackSingleObject(value);
        }

        public T Deserialize<T>(byte[] value)
        {
            var serializer = GetSerializer<T>();

            return serializer.UnpackSingleObject(value);
        }

        private static MessagePackSerializer<T> GetSerializer<T>()
        {
            return MsgPack.Serialization.MessagePackSerializer.Get<T>();
        }
    }
}