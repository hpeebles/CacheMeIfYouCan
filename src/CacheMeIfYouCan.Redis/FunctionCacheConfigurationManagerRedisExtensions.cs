namespace CacheMeIfYouCan.Redis
{
    public static class FunctionCacheConfigurationManagerRedisExtensions
    {
        public static FunctionCacheConfigurationManager<TK, TV> WithRedis<TK, TV>(this FunctionCacheConfigurationManager<TK, TV> configManager, RedisConfig<TV> config)
        {
            configManager.WithCacheFactory(() => RedisCacheBuilder.Build(config));
            return configManager;
        }
    }
}