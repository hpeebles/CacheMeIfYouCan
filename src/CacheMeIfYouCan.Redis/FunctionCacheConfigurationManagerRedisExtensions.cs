using System;

namespace CacheMeIfYouCan.Redis
{
    public static class FunctionCacheConfigurationManagerRedisExtensions
    {
        public static FunctionCacheConfigurationManager<TK, TV> WithRedis<TK, TV>(
            this FunctionCacheConfigurationManager<TK, TV> configManager,
            Action<RedisConfig<TV>> configAction)
        {
            Func<TV, string> serializer;
            if (typeof(TV) == typeof(string))
                serializer = obj => obj as string;
            else
                serializer = obj => obj.ToString();

            Func<string, TV> deserializer = str => (TV)(object)str;
            
            var config = new RedisConfig<TV>
            {
                Serializer = serializer,
                Deserializer = deserializer,
                MemoryCacheEnabled = true
            };

            configAction(config);
            
            configManager.WithCacheFactory(memoryCache => RedisCacheBuilder.Build(config, memoryCache));
            return configManager;
        }
    }
}