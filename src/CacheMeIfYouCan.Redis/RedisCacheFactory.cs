using System;
using StackExchange.Redis;

namespace CacheMeIfYouCan.Redis
{
    internal class RedisCacheFactory : ICacheFactory
    {
        private readonly RedisConfig _config;

        public RedisCacheFactory(RedisConfig config)
        {
            _config = config;
        }

        public ICache<T> Build<T>(MemoryCache<T> memoryCache, Func<T, string> serializer, Func<string, T> deserializer)
        {
            var options = new ConfigurationOptions();
            
            foreach (var endpoint in _config.Endpoints)
                options.EndPoints.Add(endpoint);
            
            var multiplexer = ConnectionMultiplexer.Connect(options);

            return new RedisCache<T>(multiplexer, _config, memoryCache, serializer, deserializer);
        }
    }
}