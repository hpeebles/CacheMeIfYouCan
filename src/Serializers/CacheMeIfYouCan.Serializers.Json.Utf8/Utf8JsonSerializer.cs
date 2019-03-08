using Utf8Json;

namespace CacheMeIfYouCan.Serializers.Json.Utf8
{
    public sealed class Utf8JsonSerializer : IByteSerializer
    {
        public byte[] Serialize<T>(T value)
        {
            return JsonSerializer.Serialize(value);
        }

        public T Deserialize<T>(byte[] value)
        {
            return JsonSerializer.Deserialize<T>(value);
        }
    }
}