using System;
using CacheMeIfYouCan.Caches;
using StackExchange.Redis;

namespace CacheMeIfYouCan.Redis
{
    public class RedisCacheFactory : ICacheFactory
    {
        private readonly RedisCacheFactoryConfig _redisConfig;

        public RedisCacheFactory(RedisCacheFactoryConfig redisConfig)
        {
            _redisConfig = redisConfig;
        }

        public bool RequiresStringKeys => true;

        public ICache<TK, TV> Build<TK, TV>(CacheFactoryConfig<TK, TV> config, Action<Key<TK>> removeFromLocalCacheAction)
        {
            var options = new ConfigurationOptions();
            
            foreach (var endpoint in _redisConfig.Endpoints)
                options.EndPoints.Add(endpoint);
            
            var multiplexer = ConnectionMultiplexer.Connect(options);

            var keySpacePrefix = _redisConfig.KeySpacePrefixFunc?.Invoke(config.FunctionInfo);
            
            return new RedisCache<TK, TV>(
                multiplexer,
                _redisConfig.Database,
                keySpacePrefix,
                config.KeyDeserializer,
                config.ValueSerializer,
                config.ValueDeserializer,
                removeFromLocalCacheAction);
        }
    }
}