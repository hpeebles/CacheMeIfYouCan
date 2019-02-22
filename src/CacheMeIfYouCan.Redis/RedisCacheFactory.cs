using System;
using CacheMeIfYouCan.Configuration;
using StackExchange.Redis;

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
            config.Validate();
            
            var connection = RedisConnectionManager.GetOrAdd(_redisConfig.Configuration);

            var (serializer, deserializer) = GetValueSerializers(config);
            
            return new RedisCache<TK, TV>(
                connection,
                config.CacheName,
                _redisConfig.Database,
                config.KeyspacePrefix,
                config.KeyDeserializer,
                serializer,
                deserializer,
                _redisConfig.KeyEventsToSubscribeTo);
        }

        private static (Func<TV, RedisValue>, Func<RedisValue, TV>) GetValueSerializers<TK, TV>(
            DistributedCacheConfig<TK, TV> config)
        {
            Func<TV, RedisValue> serializer;
            Func<RedisValue, TV> deserializer;
            if (config.ValueSerializer != null && config.ValueDeserializer != null)
            {
                serializer = v => config.ValueSerializer(v);
                deserializer = v => config.ValueDeserializer(v);
            }
            else if (config.ValueByteSerializer != null && config.ValueByteDeserializer != null)
            {
                serializer = v => config.ValueByteSerializer(v);
                deserializer = v => config.ValueByteDeserializer(v);
            }
            else
            {
                // This should be unreachable
                throw new Exception($"Value serializers are not set up correctly. CacheName: {config.CacheName}");
            }

            return (serializer, deserializer);
        }
    }
}