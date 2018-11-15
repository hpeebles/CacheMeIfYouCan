using System;
using CacheMeIfYouCan.Configuration;

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

        public ICache<TK, TV> Build<TK, TV>(CacheFactoryConfig<TK, TV> config)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            if (config.KeyDeserializer == null) throw new ArgumentNullException(nameof(config.KeyDeserializer));
            if (config.ValueSerializer == null) throw new ArgumentNullException(nameof(config.ValueSerializer));
            if (config.ValueDeserializer == null) throw new ArgumentNullException(nameof(config.ValueDeserializer));
            
            var connection = RedisConnectionManager.GetOrAdd(_redisConfig.ConnectionString ?? _redisConfig.Configuration.ToString());
            
            return new RedisCache<TK, TV>(
                connection,
                _redisConfig.Database,
                config.KeyspacePrefix,
                config.KeyDeserializer,
                config.ValueSerializer,
                config.ValueDeserializer);
        }
    }
}