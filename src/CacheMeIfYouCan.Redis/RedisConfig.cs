using System.Collections.Generic;

namespace CacheMeIfYouCan.Redis
{
    public class RedisConfig
    {
        public IList<string> Endpoints = new List<string>();
        public int Database;
        public string KeySpacePrefix;
        public bool MemoryCacheEnabled = true;
    }
}
