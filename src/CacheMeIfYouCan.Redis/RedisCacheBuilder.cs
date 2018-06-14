using StackExchange.Redis;

namespace CacheMeIfYouCan.Redis
{
    internal static class RedisCacheBuilder
    {
        public static ICache<T> Build<T>(RedisConfig<T> config, MemoryCache<T> memoryCache)
        {
            var options = new ConfigurationOptions();
            
            foreach (var endpoint in config.Endpoints)
                options.EndPoints.Add(endpoint);
            
            var multiplexer = ConnectionMultiplexer.Connect(options);

            return new RedisCache<T>(multiplexer, config, memoryCache);
        }
    }
}