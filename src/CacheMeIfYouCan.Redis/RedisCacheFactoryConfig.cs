using System;
using StackExchange.Redis;

namespace CacheMeIfYouCan.Redis
{
    public class RedisCacheFactoryConfig
    {
        private ConfigurationOptions _configurationOptions;

        public string ConnectionString
        {
            get => _configurationOptions.ToString();
            set => _configurationOptions = ConfigurationOptions.Parse(value);
        }

        public ConfigurationOptions Configuration
        {
            get => _configurationOptions;
            set => ConnectionString = value.ToString();
        }

        public int Database;
        public Func<CachedProxyFunctionInfo, string> KeySpacePrefixFunc;
        public string KeySpacePrefix { set => KeySpacePrefixFunc = f => value; }
    }
}
