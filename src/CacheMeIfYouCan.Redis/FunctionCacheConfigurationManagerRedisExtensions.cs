namespace CacheMeIfYouCan.Redis
{
    public static class FunctionCacheConfigurationManagerRedisExtensions
    {
        public static FunctionCacheConfigurationManager<T> WithRedis<T>(this FunctionCacheConfigurationManager<T> configManager, RedisConfig<T> config)
        {
            configManager.WithCacheFactory(() => RedisCacheBuilder.Build<T>(config));
            return configManager;
        }
    }
}