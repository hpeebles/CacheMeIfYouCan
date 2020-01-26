using System;
using System.Buffers;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace CacheMeIfYouCan.Redis
{
    public sealed class RedisCache<TKey, TValue> : IDistributedCache<TKey, TValue>
    {
        private readonly IConnectionMultiplexer _connectionMultiplexer;
        private readonly Func<TKey, RedisKey> _keySerializer;
        private readonly Func<TValue, RedisValue> _valueSerializer;
        private readonly Func<RedisValue, TValue> _valueDeserializer;
        private readonly int _dbIndex;
        private readonly CommandFlags _setValueFlags;
        private readonly RedisValue _nullValue;
        private static readonly IReadOnlyCollection<KeyValuePair<TKey, ValueAndTimeToLive<TValue>>> EmptyResults =
            new KeyValuePair<TKey, ValueAndTimeToLive<TValue>>[0];
        
        public RedisCache(
            IConnectionMultiplexer connectionMultiplexer,
            Func<TKey, RedisKey> keySerializer,
            Func<TValue, RedisValue> valueSerializer,
            Func<RedisValue, TValue> valueDeserializer,
            int dbIndex = 0,
            bool useFireAndForgetWherePossible = false,
            RedisKey keyPrefix = default,
            RedisValue nullValue = default)
        {
            _connectionMultiplexer = connectionMultiplexer;
            _keySerializer = keyPrefix == default(RedisKey)
                ? keySerializer
                : k => keySerializer(k).Prepend(keyPrefix);
            _valueSerializer = valueSerializer;
            _valueDeserializer = valueDeserializer;
            _dbIndex = dbIndex;
            _setValueFlags = useFireAndForgetWherePossible ? CommandFlags.FireAndForget : CommandFlags.None;
            _nullValue = nullValue.IsNull ? (RedisValue)"null" : nullValue;
        }

        public async Task<(bool Success, ValueAndTimeToLive<TValue> Value)> TryGet(TKey key)
        {
            var redisDb = _connectionMultiplexer.GetDatabase(_dbIndex);

            var redisKey = _keySerializer(key);
            
            var fromRedis = await redisDb
                .StringGetWithExpiryAsync(redisKey)
                .ConfigureAwait(false);

            if (fromRedis.Value.IsNull)
                return default;

            var value = fromRedis.Value == _nullValue
                ? default
                : _valueDeserializer(fromRedis.Value);

            return (true, new ValueAndTimeToLive<TValue>(value, fromRedis.Expiry ?? TimeSpan.FromDays(1)));
        }

        public async Task Set(TKey key, TValue value, TimeSpan timeToLive)
        {
            var redisDb = _connectionMultiplexer.GetDatabase(_dbIndex);

            var redisKey = _keySerializer(key);
            var redisValue = value is null
                ? _nullValue
                : _valueSerializer(value);

            var task = redisDb.StringSetAsync(redisKey, redisValue, timeToLive, flags: _setValueFlags);

            if (!task.IsCompleted)
                await task.ConfigureAwait(false);
        }

        public async Task<IReadOnlyCollection<KeyValuePair<TKey, ValueAndTimeToLive<TValue>>>> GetMany(IReadOnlyCollection<TKey> keys)
        {
            var redisDb = _connectionMultiplexer.GetDatabase(_dbIndex);

            var valuesFoundCount = 0;
            var tasks = ArrayPool<Task<(TKey, RedisValueWithExpiry)>>.Shared.Rent(keys.Count);

            try
            {
                var i = 0;
                foreach (var key in keys)
                    tasks[i++] = GetSingle(key);

                await Task.WhenAll(new ArraySegment<Task<(TKey, RedisValueWithExpiry)>>(tasks, 0, keys.Count)).ConfigureAwait(false);

                if (valuesFoundCount == 0)
                    return EmptyResults;

                var results = new KeyValuePair<TKey, ValueAndTimeToLive<TValue>>[valuesFoundCount];
                var resultsIndex = 0;
                foreach (var task in tasks)
                {
                    var (key, fromRedis) = task.Result;

                    if (fromRedis.Value.IsNull)
                        continue;

                    var value = fromRedis.Value == _nullValue
                        ? default
                        : _valueDeserializer(fromRedis.Value);

                    results[resultsIndex++] = new KeyValuePair<TKey, ValueAndTimeToLive<TValue>>(
                        key,
                        new ValueAndTimeToLive<TValue>(value, fromRedis.Expiry ?? TimeSpan.FromDays(1)));

                    if (resultsIndex == valuesFoundCount)
                        break;
                }

                return results;
            }
            finally
            {
                ArrayPool<Task<(TKey, RedisValueWithExpiry)>>.Shared.Return(tasks);
            }

            async Task<(TKey, RedisValueWithExpiry)> GetSingle(TKey key)
            {
                var fromRedis = await redisDb
                    .StringGetWithExpiryAsync(_keySerializer(key))
                    .ConfigureAwait(false);

                if (!fromRedis.Value.IsNull)
                    Interlocked.Increment(ref valuesFoundCount);

                return (key, fromRedis);
            }
        }

        public async Task SetMany(IReadOnlyCollection<KeyValuePair<TKey, TValue>> values, TimeSpan timeToLive)
        {
            var redisDb = _connectionMultiplexer.GetDatabase(_dbIndex);

            var tasks = ArrayPool<Task>.Shared.Rent(values.Count);

            try
            {
                var i = 0;
                foreach (var kv in values)
                {
                    var redisKey = _keySerializer(kv.Key);
                    var redisValue = kv.Value is null
                        ? _nullValue
                        : _valueSerializer(kv.Value);

                    tasks[i++] = redisDb.StringSetAsync(redisKey, redisValue, timeToLive, flags: _setValueFlags);
                }

                var waitForAllTasksTask = Task.WhenAll(new ArraySegment<Task>(tasks, 0, values.Count));

                if (!waitForAllTasksTask.IsCompleted)
                    await waitForAllTasksTask.ConfigureAwait(false);
            }
            finally
            {
                ArrayPool<Task>.Shared.Return(tasks);
            }
        }
    }
    
    public sealed class RedisCache<TOuterKey, TInnerKey, TValue> : IDistributedCache<TOuterKey, TInnerKey, TValue>
    {
        private readonly IConnectionMultiplexer _connectionMultiplexer;
        private readonly Func<TOuterKey, RedisKey> _outerKeySerializer;
        private readonly Func<TInnerKey, RedisKey> _innerKeySerializer;
        private readonly Func<TValue, RedisValue> _valueSerializer;
        private readonly Func<RedisValue, TValue> _valueDeserializer;
        private readonly int _dbIndex;
        private readonly CommandFlags _setValueFlags;
        private readonly RedisKey _keySeparator;
        private readonly RedisValue _nullValue;
        private static readonly IReadOnlyCollection<KeyValuePair<TInnerKey, ValueAndTimeToLive<TValue>>> EmptyResults =
            new KeyValuePair<TInnerKey, ValueAndTimeToLive<TValue>>[0];

        public RedisCache(
            IConnectionMultiplexer connectionMultiplexer,
            Func<TOuterKey, RedisKey> outerKeySerializer,
            Func<TInnerKey, RedisKey> innerKeySerializer,
            Func<TValue, RedisValue> valueSerializer,
            Func<RedisValue, TValue> valueDeserializer,
            int dbIndex = 0,
            bool useFireAndForgetWherePossible = false,
            RedisKey keySeparator = default,
            RedisKey keyPrefix = default,
            RedisValue nullValue = default)
        {
            _connectionMultiplexer = connectionMultiplexer;
            _outerKeySerializer = keyPrefix == default(RedisKey)
                ? outerKeySerializer
                : k => outerKeySerializer(k).Prepend(keyPrefix);
            _innerKeySerializer = innerKeySerializer;
            _valueSerializer = valueSerializer;
            _valueDeserializer = valueDeserializer;
            _dbIndex = dbIndex;
            _setValueFlags = useFireAndForgetWherePossible ? CommandFlags.FireAndForget : CommandFlags.None;
            _keySeparator = keySeparator;
            _nullValue = nullValue.IsNull ? (RedisValue)"null" : nullValue;
        }
        
        public async Task<IReadOnlyCollection<KeyValuePair<TInnerKey, ValueAndTimeToLive<TValue>>>> GetMany(
            TOuterKey outerKey,
            IReadOnlyCollection<TInnerKey> innerKeys)
        {
            var redisDb = _connectionMultiplexer.GetDatabase(_dbIndex);

            var redisKeyPrefix = _outerKeySerializer(outerKey).Append(_keySeparator);

            var valuesFoundCount = 0;
            var tasks = ArrayPool<Task<(TInnerKey, RedisValueWithExpiry)>>.Shared.Rent(innerKeys.Count);

            try
            {
                var i = 0;
                foreach (var innerKey in innerKeys)
                    tasks[i++] = GetSingle(innerKey);

                await Task.WhenAll(new ArraySegment<Task<(TInnerKey, RedisValueWithExpiry)>>(tasks, 0, innerKeys.Count)).ConfigureAwait(false);

                if (valuesFoundCount == 0)
                    return EmptyResults;

                var results = new KeyValuePair<TInnerKey, ValueAndTimeToLive<TValue>>[valuesFoundCount];
                var resultsIndex = 0;
                foreach (var task in tasks)
                {
                    var (key, fromRedis) = task.Result;

                    if (fromRedis.Value.IsNull)
                        continue;

                    var value = fromRedis.Value == _nullValue
                        ? default
                        : _valueDeserializer(fromRedis.Value);

                    results[resultsIndex++] = new KeyValuePair<TInnerKey, ValueAndTimeToLive<TValue>>(
                        key,
                        new ValueAndTimeToLive<TValue>(value, fromRedis.Expiry ?? TimeSpan.FromDays(1)));

                    if (resultsIndex == valuesFoundCount)
                        break;
                }

                return results;
            }
            finally
            {
                ArrayPool<Task<(TInnerKey, RedisValueWithExpiry)>>.Shared.Return(tasks);
            }

            async Task<(TInnerKey, RedisValueWithExpiry)> GetSingle(TInnerKey innerKey)
            {
                var redisKey = redisKeyPrefix.Append(_innerKeySerializer(innerKey));
                
                var fromRedis = await redisDb
                    .StringGetWithExpiryAsync(redisKey)
                    .ConfigureAwait(false);

                if (!fromRedis.Value.IsNull)
                    Interlocked.Increment(ref valuesFoundCount);

                return (innerKey, fromRedis);
            }
        }

        public async Task SetMany(
            TOuterKey outerKey,
            IReadOnlyCollection<KeyValuePair<TInnerKey, TValue>> values,
            TimeSpan timeToLive)
        {
            var redisDb = _connectionMultiplexer.GetDatabase(_dbIndex);

            var redisKeyPrefix = _outerKeySerializer(outerKey).Append(_keySeparator);
            
            var tasks = ArrayPool<Task>.Shared.Rent(values.Count);

            try
            {
                var index = 0;
                foreach (var kv in values)
                {
                    var redisKey = redisKeyPrefix.Append(_innerKeySerializer(kv.Key));

                    var redisValue = kv.Value is null
                        ? _nullValue
                        : _valueSerializer(kv.Value);

                    tasks[index++] = redisDb.StringSetAsync(redisKey, redisValue, timeToLive, flags: _setValueFlags);
                }

                var waitForAllTasksTask = Task.WhenAll(new ArraySegment<Task>(tasks, 0, values.Count));

                if (!waitForAllTasksTask.IsCompleted)
                    await waitForAllTasksTask.ConfigureAwait(false);
            }
            finally
            {
                ArrayPool<Task>.Shared.Return(tasks);
            }
        }
    }
}