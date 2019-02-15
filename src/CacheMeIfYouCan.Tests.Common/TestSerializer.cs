using System.ComponentModel;
using CacheMeIfYouCan.Serializers;

namespace CacheMeIfYouCan.Tests.Common
{
    public class TestSerializer : ISerializer
    {
        public int SerializeCount = 0;
        public int DeserializeCount = 0;

        public void ResetCounts()
        {
            SerializeCount = 0;
            DeserializeCount = 0;
        }
        
        public string Serialize<T>(T value)
        {
            SerializeCount++;
            return value.ToString();
        }

        public T Deserialize<T>(string value)
        {
            DeserializeCount++;

            var typeConverter = TypeDescriptor.GetConverter(typeof(T));

            return (T)typeConverter.ConvertFromString(value);
        }
    }
}
