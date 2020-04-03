using System.IO;
using System.Text.Json;

namespace CacheMeIfYouCan.Serializers.Json
{
    public sealed class JsonSerializer<T> : ISerializer<T>
    {
        private readonly JsonSerializerOptions _options;

        public JsonSerializer(JsonSerializerOptions options = null)
        {
            _options = options;
        }
        
        public void Serialize(Stream destination, T value)
        {
            JsonSerializer.SerializeAsync(destination, value, _options).Wait();
        }

        public T Deserialize(Stream source)
        {
            if (source is MemoryStream m && m.TryGetBuffer(out var array))
                return JsonSerializer.Deserialize<T>(array);
            
            return JsonSerializer.DeserializeAsync<T>(source, _options).Result;
        }

        public T Deserialize(byte[] bytes)
        {
            return JsonSerializer.Deserialize<T>(bytes, _options);
        }
    }
}