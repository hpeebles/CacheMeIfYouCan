using Newtonsoft.Json;

namespace CacheMeIfYouCan.Serializers.Json.Newtonsoft
{
    public class JsonSerializer : ISerializer
    {
        private readonly JsonSerializerSettings _settings;

        public JsonSerializer()
        {
            _settings = new JsonSerializerSettings();
        }
        
        public JsonSerializer(JsonSerializerSettings settings)
        {
            _settings = settings;
        }
        
        public string Serialize<T>(T value)
        {
            return JsonConvert.SerializeObject(value, _settings);
        }

        public T Deserialize<T>(string value)
        {
            return JsonConvert.DeserializeObject<T>(value, _settings);
        }
    }
}