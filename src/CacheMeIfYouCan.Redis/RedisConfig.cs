using System;
using System.Collections.Generic;

namespace CacheMeIfYouCan.Redis
{
    public class RedisConfig<T>
    {
        public IList<string> Endpoints = new List<string>();
        public int Database;
        public Func<T, string> Serializer;
        public Func<string, T> Deserializer;
        public bool MemoryCacheEnabled;
        public string KeySpacePrefix;
    }
}
