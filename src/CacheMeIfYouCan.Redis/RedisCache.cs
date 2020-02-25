﻿using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.IO;
using StackExchange.Redis;

namespace CacheMeIfYouCan.Redis
{
    public sealed class RedisCache<TKey, TValue> : IDistributedCache<TKey, TValue>, IDisposable
    {
        private readonly IConnectionMultiplexer _connectionMultiplexer;
        private readonly Func<TKey, RedisKey> _keySerializer;
        private readonly int _dbIndex;
        private readonly CommandFlags _setValueFlags;
        private readonly RedisValueConverter<TValue> _redisValueConverter;
        private bool _disposed;
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
            : this (connectionMultiplexer, keySerializer, dbIndex, useFireAndForgetWherePossible, keyPrefix)
        {
            _redisValueConverter = new RedisValueConverter<TValue>(
                valueSerializer,
                valueDeserializer,
                nullValue);
        }
        
        public RedisCache(
            IConnectionMultiplexer connectionMultiplexer,
            Func<TKey, RedisKey> keySerializer,
            IStreamSerializer<TValue> valueSerializer,
            int dbIndex = 0,
            bool useFireAndForgetWherePossible = false,
            RedisKey keyPrefix = default,
            RedisValue nullValue = default,
            RecyclableMemoryStreamManager recyclableMemoryStreamManager = default)
            : this (connectionMultiplexer, keySerializer, dbIndex, useFireAndForgetWherePossible, keyPrefix)
        {
            _redisValueConverter = new RedisValueConverter<TValue>(
                valueSerializer,
                recyclableMemoryStreamManager,
                nullValue);
        }
        
        private RedisCache(IConnectionMultiplexer connectionMultiplexer,
            Func<TKey, RedisKey> keySerializer,
            int dbIndex = 0,
            bool useFireAndForgetWherePossible = false,
            RedisKey keyPrefix = default)
        {
            _connectionMultiplexer = connectionMultiplexer;
            _keySerializer = keyPrefix == default(RedisKey)
                ? keySerializer
                : k => keySerializer(k).Prepend(keyPrefix);
            _dbIndex = dbIndex;
            _setValueFlags = useFireAndForgetWherePossible
                ? CommandFlags.FireAndForget
                : CommandFlags.None;
        }

        public async Task<(bool Success, ValueAndTimeToLive<TValue> Value)> TryGet(TKey key)
        {
            CheckDisposed();
            
            var redisDb = _connectionMultiplexer.GetDatabase(_dbIndex);

            var redisKey = _keySerializer(key);
            
            var fromRedis = await redisDb
                .StringGetWithExpiryAsync(redisKey)
                .ConfigureAwait(false);

            if (fromRedis.Value.IsNull)
                return default;

            var value = _redisValueConverter.ConvertFromRedisValue(fromRedis.Value);

            return (true, new ValueAndTimeToLive<TValue>(value, fromRedis.Expiry ?? TimeSpan.FromDays(1)));
        }

        public async Task Set(TKey key, TValue value, TimeSpan timeToLive)
        {
            CheckDisposed();
            
            var redisDb = _connectionMultiplexer.GetDatabase(_dbIndex);

            var redisKey = _keySerializer(key);

            MemoryStream stream = null;
            try
            {
                var redisValue = _redisValueConverter.ConvertToRedisValue(value, out stream);

                var task = redisDb.StringSetAsync(redisKey, redisValue, timeToLive, flags: _setValueFlags);

                if (!task.IsCompleted)
                    await task.ConfigureAwait(false);
            }
            finally
            {
                stream?.Dispose();
            }
        }

        public async Task<int> GetMany(
            IReadOnlyCollection<TKey> keys,
            Memory<KeyValuePair<TKey, ValueAndTimeToLive<TValue>>> destination)
        {
            CheckDisposed();
            
            var redisDb = _connectionMultiplexer.GetDatabase(_dbIndex);

            var valuesFoundCount = 0;
            var tasks = ArrayPool<Task<(TKey, RedisValueWithExpiry)>>.Shared.Rent(keys.Count);

            try
            {
                var i = 0;
                foreach (var key in keys)
                    tasks[i++] = GetSingle(key);

                await Task
                    .WhenAll(new ArraySegment<Task<(TKey, RedisValueWithExpiry)>>(tasks, 0, keys.Count))
                    .ConfigureAwait(false);

                return valuesFoundCount == 0
                    ? 0
                    : CopyResultsToDestinationArray();
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

            int CopyResultsToDestinationArray()
            {
                var span = destination.Span;
                var index = 0;
                foreach (var task in tasks)
                {
                    var (key, fromRedis) = task.Result;

                    if (fromRedis.Value.IsNull)
                        continue;

                    var value = _redisValueConverter.ConvertFromRedisValue(fromRedis.Value);
                    
                    span[index++] = new KeyValuePair<TKey, ValueAndTimeToLive<TValue>>(
                        key,
                        new ValueAndTimeToLive<TValue>(value, fromRedis.Expiry ?? TimeSpan.FromDays(1)));

                    if (index == valuesFoundCount)
                        break;
                }

                return index;
            }
        }

        public async Task SetMany(IReadOnlyCollection<KeyValuePair<TKey, TValue>> values, TimeSpan timeToLive)
        {
            CheckDisposed();
            
            var redisDb = _connectionMultiplexer.GetDatabase(_dbIndex);

            var tasks = ArrayPool<Task>.Shared.Rent(values.Count);
            var streams = ArrayPool<MemoryStream>.Shared.Rent(values.Count);
            var streamIndex = 0;
            try
            {
                var i = 0;
                foreach (var kv in values)
                {
                    var redisKey = _keySerializer(kv.Key);

                    var redisValue = _redisValueConverter.ConvertToRedisValue(kv.Value, out var stream);

                    if (!(stream is null))
                        streams[streamIndex++] = stream;
                    
                    tasks[i++] = redisDb.StringSetAsync(redisKey, redisValue, timeToLive, flags: _setValueFlags);
                }

                var waitForAllTasksTask = Task.WhenAll(new ArraySegment<Task>(tasks, 0, values.Count));

                if (!waitForAllTasksTask.IsCompleted)
                    await waitForAllTasksTask.ConfigureAwait(false);
            }
            finally
            {
                for (var i = 0; i < streamIndex; i++)
                    streams[i].Dispose();
                
                ArrayPool<Task>.Shared.Return(tasks);
                ArrayPool<MemoryStream>.Shared.Return(streams);
            }
        }

        public void Dispose()
        {
            _disposed = true;
            _connectionMultiplexer.Dispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CheckDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(this.GetType().ToString());
        }
    }
    
    public sealed class RedisCache<TOuterKey, TInnerKey, TValue> : IDistributedCache<TOuterKey, TInnerKey, TValue>, IDisposable
    {
        private readonly IConnectionMultiplexer _connectionMultiplexer;
        private readonly Func<TOuterKey, RedisKey> _outerKeySerializer;
        private readonly Func<TInnerKey, RedisKey> _innerKeySerializer;
        private readonly int _dbIndex;
        private readonly CommandFlags _setValueFlags;
        private readonly RedisKey _keySeparator;
        private readonly RedisValueConverter<TValue> _redisValueConverter;
        private bool _disposed;
        private static readonly IReadOnlyCollection<KeyValuePair<TInnerKey, ValueAndTimeToLive<TValue>>> EmptyResults =
            new KeyValuePair<TInnerKey, ValueAndTimeToLive<TValue>>[0];

        public RedisCache(
            IConnectionMultiplexer connectionMultiplexer,
            Func<TOuterKey, RedisKey> outerKeySerializer,
            Func<TInnerKey, RedisKey> innerKeySerializer,
            IStreamSerializer<TValue> valueSerializer,
            int dbIndex = 0,
            bool useFireAndForgetWherePossible = false,
            RedisKey keySeparator = default,
            RedisKey keyPrefix = default,
            RedisValue nullValue = default,
            RecyclableMemoryStreamManager recyclableMemoryStreamManager = default)
            : this(
                connectionMultiplexer,
                outerKeySerializer,
                innerKeySerializer,
                dbIndex,
                useFireAndForgetWherePossible,
                keySeparator,
                keyPrefix)
        {
            _redisValueConverter = new RedisValueConverter<TValue>(
                valueSerializer,
                recyclableMemoryStreamManager,
                nullValue);
        }
        
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
            : this(
                connectionMultiplexer,
                outerKeySerializer,
                innerKeySerializer,
                dbIndex,
                useFireAndForgetWherePossible,
                keySeparator,
                keyPrefix)
        {
            _redisValueConverter = new RedisValueConverter<TValue>(
                valueSerializer,
                valueDeserializer,
                nullValue);
        }
        
        private RedisCache(
            IConnectionMultiplexer connectionMultiplexer,
            Func<TOuterKey, RedisKey> outerKeySerializer,
            Func<TInnerKey, RedisKey> innerKeySerializer,
            int dbIndex = 0,
            bool useFireAndForgetWherePossible = false,
            RedisKey keySeparator = default,
            RedisKey keyPrefix = default)
        {
            _connectionMultiplexer = connectionMultiplexer;
            _outerKeySerializer = keyPrefix == default(RedisKey)
                ? outerKeySerializer
                : k => outerKeySerializer(k).Prepend(keyPrefix);
            _innerKeySerializer = innerKeySerializer;
            _dbIndex = dbIndex;
            _setValueFlags = useFireAndForgetWherePossible ? CommandFlags.FireAndForget : CommandFlags.None;
            _keySeparator = keySeparator;
        }
        
        public async Task<int> GetMany(
            TOuterKey outerKey,
            IReadOnlyCollection<TInnerKey> innerKeys,
            Memory<KeyValuePair<TInnerKey, ValueAndTimeToLive<TValue>>> destination)
        {
            CheckDisposed();
            
            var redisDb = _connectionMultiplexer.GetDatabase(_dbIndex);

            var redisKeyPrefix = _outerKeySerializer(outerKey).Append(_keySeparator);

            var valuesFoundCount = 0;
            var tasks = ArrayPool<Task<(TInnerKey, RedisValueWithExpiry)>>.Shared.Rent(innerKeys.Count);

            try
            {
                var i = 0;
                foreach (var innerKey in innerKeys)
                    tasks[i++] = GetSingle(innerKey);

                await Task
                    .WhenAll(new ArraySegment<Task<(TInnerKey, RedisValueWithExpiry)>>(tasks, 0, innerKeys.Count))
                    .ConfigureAwait(false);

                return valuesFoundCount == 0
                    ? 0
                    : CopyResultsToDestinationArray();
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
            
            int CopyResultsToDestinationArray()
            {
                var span = destination.Span;
                var index = 0;
                foreach (var task in tasks)
                {
                    var (key, fromRedis) = task.Result;

                    if (fromRedis.Value.IsNull)
                        continue;

                    var value = _redisValueConverter.ConvertFromRedisValue(fromRedis.Value);
                    
                    span[index++] = new KeyValuePair<TInnerKey, ValueAndTimeToLive<TValue>>(
                        key,
                        new ValueAndTimeToLive<TValue>(value, fromRedis.Expiry ?? TimeSpan.FromDays(1)));

                    if (index == valuesFoundCount)
                        break;
                }

                return index;
            }
        }

        public async Task SetMany(
            TOuterKey outerKey,
            IReadOnlyCollection<KeyValuePair<TInnerKey, TValue>> values,
            TimeSpan timeToLive)
        {
            CheckDisposed();
            
            var redisDb = _connectionMultiplexer.GetDatabase(_dbIndex);

            var redisKeyPrefix = _outerKeySerializer(outerKey).Append(_keySeparator);
            
            var tasks = ArrayPool<Task>.Shared.Rent(values.Count);
            var streams = ArrayPool<MemoryStream>.Shared.Rent(values.Count);
            var streamIndex = 0;
            try
            {
                var index = 0;
                foreach (var kv in values)
                {
                    var redisKey = redisKeyPrefix.Append(_innerKeySerializer(kv.Key));

                    var redisValue = _redisValueConverter.ConvertToRedisValue(kv.Value, out var stream);

                    if (!(stream is null))
                        streams[streamIndex++] = stream;

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

        public void Dispose()
        {
            _disposed = true;
            _connectionMultiplexer.Dispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CheckDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(this.GetType().ToString());
        }
    }
}