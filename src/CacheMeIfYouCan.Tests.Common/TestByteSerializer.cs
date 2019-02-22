using System;
using System.ComponentModel;
using System.Text;
using CacheMeIfYouCan.Serializers;

namespace CacheMeIfYouCan.Tests.Common
{
    public class TestByteSerializer : IByteSerializer
    {
        public int SerializeCount = 0;
        public int DeserializeCount = 0;

        public void ResetCounts()
        {
            SerializeCount = 0;
            DeserializeCount = 0;
        }
        
        public byte[] Serialize<T>(T value)
        {
            SerializeCount++;
            return Encoding.UTF8.GetBytes(value.ToString());
        }

        public T Deserialize<T>(byte[] value)
        {
            DeserializeCount++;

            var str = Encoding.UTF8.GetString(value);

            var typeConverter = TypeDescriptor.GetConverter(typeof(T));

            return (T)typeConverter.ConvertFromString(str);
        }
    }
}
