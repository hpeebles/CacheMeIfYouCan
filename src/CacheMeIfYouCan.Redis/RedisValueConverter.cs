using System;
using System.IO;
using Microsoft.IO;
using StackExchange.Redis;

namespace CacheMeIfYouCan.Redis
{
    internal sealed class RedisValueConverter<T>
    {
        private readonly Func<T, RedisValue> _serializerFunc;
        private readonly Func<RedisValue, T> _deserializerFunc;
        private readonly ISerializer<T> _serializer;
        private readonly RecyclableMemoryStreamManager _recyclableMemoryStreamManager;
        private readonly RedisValue _nullValue;

        public RedisValueConverter(
            Func<T, RedisValue> serializerFunc,
            Func<RedisValue, T> deserializerFunc,
            RedisValue nullValue)
            : this(nullValue)
        {
            _serializerFunc = serializerFunc;
            _deserializerFunc = deserializerFunc;
        }

        public RedisValueConverter(
            ISerializer<T> serializer,
            RecyclableMemoryStreamManager recyclableMemoryStreamManager,
            RedisValue nullValue)
            : this(nullValue)
        {
            _serializer = serializer;
            _recyclableMemoryStreamManager = recyclableMemoryStreamManager ?? new RecyclableMemoryStreamManager();
        }

        private RedisValueConverter(RedisValue nullValue)
        {
            _nullValue = nullValue.IsNull ? (RedisValue)"null" : nullValue;
        }

        public RedisValue ConvertToRedisValue(T value, out MemoryStream pooledStream)
        {
            pooledStream = null;
            
            if (value is null)
                return _nullValue;

            if (!(_serializerFunc is null))
                return _serializerFunc(value);

            pooledStream = _recyclableMemoryStreamManager.GetStream();
            _serializer.Serialize(pooledStream, value);
            
            return new ReadOnlyMemory<byte>(pooledStream.GetBuffer(), 0, (int)pooledStream.Length);
        }

        public T ConvertFromRedisValue(RedisValue redisValue)
        {
            if (redisValue == _nullValue)
                return default;

            if (!(_deserializerFunc is null))
                return _deserializerFunc(redisValue);

            return _serializer.Deserialize(redisValue);
        }
    }
}