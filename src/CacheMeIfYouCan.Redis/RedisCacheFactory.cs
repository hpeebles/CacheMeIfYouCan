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

        public IDistributedCache<TK, TV> Build<TK, TV>(IDistributedCacheConfig<TK, TV> config)
        {
            config.Validate();
            
            var connection = _redisConfig.Connection ?? RedisConnectionContainer.GetOrAdd(_redisConfig.Configuration);
            
            var (serializer, deserializer) = GetValueSerializers(config);
            
            return new RedisCache<TK, TV>(
                connection,
                connection as IRedisSubscriber,
                config.CacheName,
                _redisConfig.Database,
                config.KeyspacePrefix,
                config.KeyDeserializer,
                serializer,
                deserializer,
                _redisConfig.KeyEventsToSubscribeTo,
                _redisConfig.UseFireAndForgetWherePossible);
        }

        private (Func<TV, RedisValue>, Func<RedisValue, TV>) GetValueSerializers<TK, TV>(
            IDistributedCacheConfig<TK, TV> config)
        {
            Func<TV, RedisValue> serializer;
            Func<RedisValue, TV> deserializer;
            if (config.HasValidValueByteSerializer)
            {
                serializer = v => config.ValueByteSerializer(v);
                deserializer = v => config.ValueByteDeserializer(v);
            }
            else if (config.HasValidValueStringSerializer)
            {
                serializer = v => config.ValueSerializer(v);
                deserializer = v => config.ValueDeserializer(v);
            }
            else
            {
                // This should be unreachable
                throw new Exception($"Value serializers are not set up correctly. CacheName: {config.CacheName}");
            }

            if (_redisConfig.NullValue != default && typeof(TV).IsClass)
            {
                var nullValue = _redisConfig.NullValue;
                var originalSerializer = serializer;
                var originalDeserializer = deserializer;
                serializer = v => v == null ? nullValue : originalSerializer(v);
                deserializer = v => v == nullValue ? default : originalDeserializer(v);
            }

            return (serializer, deserializer);
        }
    }
}