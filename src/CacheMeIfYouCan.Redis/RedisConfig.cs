using System;
using System.Collections.Generic;

namespace CacheMeIfYouCan.Redis
{
    public class RedisConfig<T>
    {
        public IList<string> Endpoints;
        public Func<T, string> Serializer;
        public Func<string, T> Deserializer;
    }
}
