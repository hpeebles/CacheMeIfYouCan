using StackExchange.Redis;

namespace CacheMeIfYouCan.Redis
{
    internal class RedisCacheFactory : ICacheFactory
    {
        private readonly RedisCacheFactoryConfig _redisConfig;

        public RedisCacheFactory(RedisCacheFactoryConfig redisConfig)
        {
            _redisConfig = redisConfig;
        }

        public ICache<T> Build<T>(CacheFactoryConfig<T> config)
        {
            var options = new ConfigurationOptions();
            
            foreach (var endpoint in _redisConfig.Endpoints)
                options.EndPoints.Add(endpoint);
            
            var multiplexer = ConnectionMultiplexer.Connect(options);

            var keySpacePrefix = _redisConfig.KeySpacePrefixFunc?.Invoke(config.FunctionInfo);
            
            return new RedisCache<T>(
                multiplexer,
                _redisConfig.Database,
                config.MemoryCache,
                keySpacePrefix,
                config.Serializer,
                config.Deserializer);
        }
    }
}