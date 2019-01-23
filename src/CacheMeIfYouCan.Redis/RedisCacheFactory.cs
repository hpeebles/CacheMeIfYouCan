using System;
using CacheMeIfYouCan.Configuration;

namespace CacheMeIfYouCan.Redis
{
    public class RedisCacheFactory : IDistributedCacheFactory
    {
        private readonly RedisCacheFactoryConfig _redisConfig;

        public RedisCacheFactory(RedisCacheFactoryConfig redisConfig)
        {
            _redisConfig = redisConfig;
        }

        public IDistributedCache<TK, TV> Build<TK, TV>(DistributedCacheConfig<TK, TV> config)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            if (config.KeyDeserializer == null) throw new ArgumentNullException(nameof(config.KeyDeserializer));
            if (config.ValueSerializer == null) throw new ArgumentNullException(nameof(config.ValueSerializer));
            if (config.ValueDeserializer == null) throw new ArgumentNullException(nameof(config.ValueDeserializer));
            
            var connection = RedisConnectionManager.GetOrAdd(_redisConfig.ConnectionString ?? _redisConfig.Configuration.ToString());
            
            return new RedisCache<TK, TV>(
                connection,
                config.CacheName,
                _redisConfig.Database,
                config.KeyspacePrefix,
                config.KeyDeserializer,
                config.ValueSerializer,
                config.ValueDeserializer,
                _redisConfig.SubscribeToKeyChanges);
        }
    }
}