using System;

namespace CacheMeIfYouCan.Serializers.ToString
{
    public sealed class ToStringSerializer : ISerializer
    {
        public string Serialize<T>(T value)
        {
            if (value is string str)
                return str;

            return value.ToString();
        }

        public T Deserialize<T>(string value)
        {
            throw new NotImplementedException();
        }
    }
}