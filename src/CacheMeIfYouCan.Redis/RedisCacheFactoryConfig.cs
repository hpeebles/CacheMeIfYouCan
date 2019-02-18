using StackExchange.Redis;

namespace CacheMeIfYouCan.Redis
{
    public class RedisCacheFactoryConfig
    {
        public string ConnectionString
        {
            get => Configuration.ToString();
            set => Configuration = ConfigurationOptions.Parse(value);
        }
        
        public ConfigurationOptions Configuration { get; set; }

        public int Database { get; set; }
        
        public KeyEvents KeyEventsToSubscribeTo { get; set; }
    }
}
