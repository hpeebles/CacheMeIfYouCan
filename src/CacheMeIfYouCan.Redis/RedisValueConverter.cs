using System;
using System.IO;
using Microsoft.IO;
using StackExchange.Redis;

namespace CacheMeIfYouCan.Redis
{
    public sealed class RedisValueConverter<T>
    {
        private readonly Func<T, RedisValue> _serializerFunc;
        private readonly Func<RedisValue, T> _deserializerFunc;
        private readonly IStreamSerializer<T> _streamSerializer;
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
            IStreamSerializer<T> streamSerializer,
            RecyclableMemoryStreamManager recyclableMemoryStreamManager,
            RedisValue nullValue)
            : this(nullValue)
        {
            _streamSerializer = streamSerializer;
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
            _streamSerializer.WriteToStream(pooledStream, value);
            
            return new ReadOnlyMemory<byte>(pooledStream.GetBuffer(), 0, (int)pooledStream.Length);
        }

        public T ConvertFromRedisValue(RedisValue redisValue)
        {
            if (redisValue == _nullValue)
                return default;

            if (!(_deserializerFunc is null))
                return _deserializerFunc(redisValue);

            return _streamSerializer.Deserialize(redisValue);
        }
    }
}