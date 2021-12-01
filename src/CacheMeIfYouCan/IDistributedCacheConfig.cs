namespace CacheMeIfYouCan
{
    public interface IDistributedCacheConfig
    {
        public string CacheType { get; }
        public string Host { get; }
        public string CacheName { get; }
    }
}