namespace CacheMeIfYouCan.Redis.Tests
{
    public class DistributedCacheConfig : IDistributedCacheConfig
    {
        public string CacheType { get; set; }
        public string Host { get; set; }
        public string CacheName { get; set; }
    }
}