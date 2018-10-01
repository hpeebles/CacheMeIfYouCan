using System;
using StackExchange.Redis;

namespace CacheMeIfYouCan.Redis
{
    public class RedisCacheFactoryConfig
    {
        public string ConnectionString;
        public ConfigurationOptions Configuration;
        public int Database;
        public Func<FunctionInfo, string> KeySpacePrefixFunc;
        public string KeySpacePrefix { set => KeySpacePrefixFunc = f => value; }
    }
}
