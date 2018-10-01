using System;
using CacheMeIfYouCan.Caches;

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
            var keySpacePrefix = _redisConfig.KeySpacePrefixFunc?.Invoke(config.FunctionInfo);

            var connection = RedisConnectionManager.GetOrAdd(_redisConfig.ConnectionString ?? _redisConfig.Configuration.ToString());
            
            return new RedisCache<TK, TV>(
                connection,
                _redisConfig.Database,
                keySpacePrefix,
                config.KeyDeserializer,
                config.ValueSerializer,
                config.ValueDeserializer,
                removeFromLocalCacheAction);
        }
    }
}