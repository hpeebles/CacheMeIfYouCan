using CacheMeIfYouCan.Configuration;

namespace CacheMeIfYouCan
{
    public static class DefaultSettings
    {
        public static DefaultCacheConfiguration Cache { get; } = new DefaultCacheConfiguration();
        public static DefaultCachedObjectConfiguration CachedObject { get; } = new DefaultCachedObjectConfiguration();
    }
}