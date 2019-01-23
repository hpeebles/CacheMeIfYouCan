using StackExchange.Redis;

namespace CacheMeIfYouCan.Redis
{
    public class RedisCacheFactoryConfig
    {
        public string ConnectionString { get; set; }
        
        public ConfigurationOptions Configuration
        {
            get => ConfigurationOptions.Parse(ConnectionString);
            set => ConnectionString = value.ToString();
        }

        public int Database { get; set; }
    }
}
