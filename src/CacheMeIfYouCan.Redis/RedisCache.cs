﻿using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using CacheMeIfYouCan.Redis.Internal;
using Microsoft.IO;
using StackExchange.Redis;

namespace CacheMeIfYouCan.Redis
{
    public sealed class RedisCache<TKey, TValue> : IDistributedCache<TKey, TValue>, IDisposable
    {
        private readonly IRedisConnection _connection;
        private readonly Func<TKey, RedisKey> _keySerializer;
        private readonly int _dbIndex;
        private readonly CommandFlags _setValueFlags;
        private readonly RedisValueConverter<TValue> _redisValueConverter;
        private readonly bool _serializeValuesToStreams;
        private readonly Subject<(string Key, KeyEventType EventType)> _keysChangedRemotely;
        private readonly RecentlySetOrRemovedKeysManager _recentlySetKeysManager;
        private readonly RecentlySetOrRemovedKeysManager _recentlyRemovedKeysManager;
        private readonly int _keyPrefixLength;
        private bool _disposed;

        public RedisCache(
            IRedisConnection connection,
            Func<TKey, RedisKey> keySerializer,
            ISerializer<TValue> valueSerializer,
            int dbIndex = 0,
            bool useFireAndForgetWherePossible = false,
            RedisKey keyPrefix = default,
            RedisValue nullValue = default,
            IRedisSubscriber subscriber = null,
            KeyEventType keyEventTypesToSubscribeTo = KeyEventType.None,
            RecyclableMemoryStreamManager recyclableMemoryStreamManager = default)
            : this(
                connection,
                keySerializer,
                dbIndex,
                useFireAndForgetWherePossible,
                keyPrefix,
                subscriber,
                keyEventTypesToSubscribeTo)
        {
            _redisValueConverter = new RedisValueConverter<TValue>(
                valueSerializer,
                recyclableMemoryStreamManager,
                nullValue);
            
            _serializeValuesToStreams = true;
        }

        public RedisCache(
            IRedisConnection connection,
            Func<TKey, RedisKey> keySerializer,
            Func<TValue, RedisValue> valueSerializer,
            Func<RedisValue, TValue> valueDeserializer,
            int dbIndex = 0,
            bool useFireAndForgetWherePossible = false,
            RedisKey keyPrefix = default,
            RedisValue nullValue = default,
            IRedisSubscriber subscriber = null,
            KeyEventType keyEventTypesToSubscribeTo = KeyEventType.None)
            : this(
                connection,
                keySerializer,
                dbIndex,
                useFireAndForgetWherePossible,
                keyPrefix,
                subscriber,
                keyEventTypesToSubscribeTo)
        {
            _redisValueConverter = new RedisValueConverter<TValue>(
                valueSerializer,
                valueDeserializer,
                nullValue);
            
            _serializeValuesToStreams = false;
        }
        
        private RedisCache(
            IRedisConnection connection,
            Func<TKey, RedisKey> keySerializer,
            int dbIndex = 0,
            bool useFireAndForgetWherePossible = false,
            RedisKey keyPrefix = default,
            IRedisSubscriber subscriber = null,
            KeyEventType keyEventTypesToSubscribeTo = KeyEventType.None)
        {
            _connection = connection;
            _keySerializer = keyPrefix == default(RedisKey)
                ? keySerializer
                : k => keySerializer(k).Prepend(keyPrefix);
            _dbIndex = dbIndex;
            _setValueFlags = useFireAndForgetWherePossible
                ? CommandFlags.FireAndForget
                : CommandFlags.None;
            _keyPrefixLength = keyPrefix.ToString().Length;

            if (keyEventTypesToSubscribeTo != KeyEventType.None)
            {
                if (subscriber is null)
                    throw new ArgumentNullException(nameof(subscriber));
                
                _keysChangedRemotely = new Subject<(string, KeyEventType)>();
                
                if (keyEventTypesToSubscribeTo.HasFlag(KeyEventType.Set))
                    _recentlySetKeysManager = new RecentlySetOrRemovedKeysManager();
                
                _recentlyRemovedKeysManager = new RecentlySetOrRemovedKeysManager();
                
                subscriber.SubscribeToKeyChanges(dbIndex, keyEventTypesToSubscribeTo, OnKeyChanged, keyPrefix);
            }
        }

        public async Task<(bool Success, ValueAndTimeToLive<TValue> Value)> TryGet(TKey key)
        {
            CheckDisposed();
            
            var redisDb = GetDatabase();

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
            
            var redisDb = GetDatabase();

            var redisKey = _keySerializer(key);

            MemoryStream stream = null;
            try
            {
                var redisValue = _redisValueConverter.ConvertToRedisValue(value, out stream);

                _recentlySetKeysManager?.Mark(((string)redisKey).Substring(_keyPrefixLength));
                
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
            ReadOnlyMemory<TKey> keys,
            Memory<KeyValuePair<TKey, ValueAndTimeToLive<TValue>>> destination)
        {
            CheckDisposed();
            
            var redisDb = GetDatabase();

            var valuesFoundCount = 0;
            
            var tasks = CreateTasks(out var pooledTasksArray);

            try
            {
                await Task
                    .WhenAll(tasks)
                    .ConfigureAwait(false);
                
                return valuesFoundCount == 0
                    ? 0
                    : CopyResultsToDestinationArray();
            }
            finally
            {
                ArrayPool<Task<(TKey, RedisValueWithExpiry)>>.Shared.Return(pooledTasksArray);
            }
            
            IReadOnlyCollection<Task<(TKey, RedisValueWithExpiry)>> CreateTasks(out Task<(TKey, RedisValueWithExpiry)>[] pooledArray)
            {
                pooledArray = ArrayPool<Task<(TKey, RedisValueWithExpiry)>>.Shared.Rent(keys.Length);
                
                var i = 0;
                foreach (var innerKey in keys.Span)
                    pooledArray[i++] = GetSingle(innerKey);

                return new ArraySegment<Task<(TKey, RedisValueWithExpiry)>>(pooledArray, 0, keys.Length);
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

        public async Task SetMany(ReadOnlyMemory<KeyValuePair<TKey, TValue>> values, TimeSpan timeToLive)
        {
            CheckDisposed();
            
            var redisDb = GetDatabase();

            var pooledTasksArray = ArrayPool<Task>.Shared.Rent(values.Length);
            var toDispose = _serializeValuesToStreams
                ? ArrayPool<IDisposable>.Shared.Rent(values.Length)
                : null;

            var tasks = CreateTasks();
            
            try
            {
                var waitForAllTasksTask = Task.WhenAll(tasks);

                if (!waitForAllTasksTask.IsCompleted)
                    await waitForAllTasksTask.ConfigureAwait(false);
            }
            finally
            {
                ArrayPool<Task>.Shared.Return(pooledTasksArray);
                
                if (!(toDispose is null))
                {
                    for (var i = 0; i < values.Length; i++)
                        toDispose[i].Dispose();
                    
                    ArrayPool<IDisposable>.Shared.Return(toDispose);
                }
            }
            
            IReadOnlyCollection<Task> CreateTasks()
            {
                var index = 0;
                foreach (var kv in values.Span)
                {
                    var redisKey = _keySerializer(kv.Key);

                    var redisValue = _redisValueConverter.ConvertToRedisValue(kv.Value, out var stream);

                    if (!(toDispose is null))
                        toDispose[index] = stream;

                    _recentlySetKeysManager?.Mark(((string)redisKey).Substring(_keyPrefixLength));

                    pooledTasksArray[index++] = redisDb.StringSetAsync(redisKey, redisValue, timeToLive, flags: _setValueFlags);
                }
                
                return new ArraySegment<Task>(pooledTasksArray, 0, index);
            }
        }

        public async Task<bool> TryRemove(TKey key)
        {
            CheckDisposed();
            
            var redisDb = GetDatabase();
            
            var redisKey = _keySerializer(key);

            _recentlyRemovedKeysManager?.Mark(((string)redisKey).Substring(_keyPrefixLength));
            
            return await redisDb
                .KeyDeleteAsync(redisKey)
                .ConfigureAwait(false);
        }

        public IObservable<(string Key, KeyEventType EventType)> KeysChangedRemotely =>
            _keysChangedRemotely?.AsObservable() ?? Observable.Empty<(string, KeyEventType)>();
        
        public void Dispose()
        {
            if (_disposed)
                return;
            
            _keysChangedRemotely?.Dispose();
            _recentlySetKeysManager?.Dispose();
            _recentlyRemovedKeysManager?.Dispose();
            _disposed = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CheckDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(this.GetType().ToString());
        }
        
        private void OnKeyChanged(string redisKey, KeyEventType eventType)
        {
            bool wasLocalChange;
            if (eventType == KeyEventType.Set)
                _recentlySetKeysManager.HandleKeyChangedNotification(redisKey, out wasLocalChange);
            else if (eventType == KeyEventType.Del)
                _recentlyRemovedKeysManager.HandleKeyChangedNotification(redisKey, out wasLocalChange);
            else
                wasLocalChange = false;

            if (wasLocalChange)
                return;
            
            _keysChangedRemotely.OnNext((redisKey, eventType));
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private IDatabase GetDatabase() => _connection.Get().GetDatabase(_dbIndex);
    }
    
    public sealed class RedisCache<TOuterKey, TInnerKey, TValue> : IDistributedCache<TOuterKey, TInnerKey, TValue>, IDisposable
    {
        private readonly IRedisConnection _connection;
        private readonly Func<TOuterKey, RedisKey> _outerKeySerializer;
        private readonly Func<TInnerKey, RedisKey> _innerKeySerializer;
        private readonly int _dbIndex;
        private readonly CommandFlags _setValueFlags;
        private readonly RedisKey _keySeparator;
        private readonly RedisValueConverter<TValue> _redisValueConverter;
        private readonly bool _serializeValuesToStreams;
        private readonly Subject<(string OuterKey, string InnerKey, KeyEventType EventType)> _keysChangedRemotely;
        private readonly RecentlySetOrRemovedKeysManager _recentlySetKeysManager;
        private readonly RecentlySetOrRemovedKeysManager _recentlyRemovedKeysManager;
        private readonly int _keyPrefixLength;
        private bool _disposed;

        public RedisCache(
            IRedisConnection connection,
            Func<TOuterKey, RedisKey> outerKeySerializer,
            Func<TInnerKey, RedisKey> innerKeySerializer,
            ISerializer<TValue> valueSerializer,
            int dbIndex = 0,
            bool useFireAndForgetWherePossible = false,
            RedisKey keySeparator = default,
            RedisKey keyPrefix = default,
            RedisValue nullValue = default,
            IRedisSubscriber subscriber = null,
            KeyEventType keyEventTypesToSubscribeTo = KeyEventType.None,
            RecyclableMemoryStreamManager recyclableMemoryStreamManager = default)
            : this(
                connection,
                outerKeySerializer,
                innerKeySerializer,
                dbIndex,
                useFireAndForgetWherePossible,
                keySeparator,
                keyPrefix,
                subscriber,
                keyEventTypesToSubscribeTo)
        {
            _redisValueConverter = new RedisValueConverter<TValue>(
                valueSerializer,
                recyclableMemoryStreamManager,
                nullValue);

            _serializeValuesToStreams = true;
        }
        
        public RedisCache(
            IRedisConnection connection,
            Func<TOuterKey, RedisKey> outerKeySerializer,
            Func<TInnerKey, RedisKey> innerKeySerializer,
            Func<TValue, RedisValue> valueSerializer,
            Func<RedisValue, TValue> valueDeserializer,
            int dbIndex = 0,
            bool useFireAndForgetWherePossible = false,
            RedisKey keySeparator = default,
            RedisKey keyPrefix = default,
            RedisValue nullValue = default,
            IRedisSubscriber subscriber = null,
            KeyEventType keyEventTypesToSubscribeTo = KeyEventType.None)
            : this(
                connection,
                outerKeySerializer,
                innerKeySerializer,
                dbIndex,
                useFireAndForgetWherePossible,
                keySeparator,
                keyPrefix,
                subscriber,
                keyEventTypesToSubscribeTo)
        {
            _redisValueConverter = new RedisValueConverter<TValue>(
                valueSerializer,
                valueDeserializer,
                nullValue);

            _serializeValuesToStreams = false;
        }
        
        private RedisCache(
            IRedisConnection connection,
            Func<TOuterKey, RedisKey> outerKeySerializer,
            Func<TInnerKey, RedisKey> innerKeySerializer,
            int dbIndex = 0,
            bool useFireAndForgetWherePossible = false,
            RedisKey keySeparator = default,
            RedisKey keyPrefix = default,
            IRedisSubscriber subscriber = null,
            KeyEventType keyEventTypesToSubscribeTo = KeyEventType.None)
        {
            _connection = connection;
            _outerKeySerializer = keyPrefix == default(RedisKey)
                ? outerKeySerializer
                : k => outerKeySerializer(k).Prepend(keyPrefix);
            _innerKeySerializer = innerKeySerializer;
            _dbIndex = dbIndex;
            _setValueFlags = useFireAndForgetWherePossible ? CommandFlags.FireAndForget : CommandFlags.None;
            _keySeparator = keySeparator;
            _keyPrefixLength = keyPrefix.ToString().Length;
            
            if (keyEventTypesToSubscribeTo != KeyEventType.None)
            {
                if (subscriber is null)
                    throw new ArgumentNullException(nameof(subscriber));
                
                _keysChangedRemotely = new Subject<(string, string, KeyEventType)>();
                
                if (keyEventTypesToSubscribeTo.HasFlag(KeyEventType.Set))
                    _recentlySetKeysManager = new RecentlySetOrRemovedKeysManager();
                
                _recentlyRemovedKeysManager = new RecentlySetOrRemovedKeysManager();
                
                subscriber.SubscribeToKeyChanges(dbIndex, keyEventTypesToSubscribeTo, OnKeyChanged, keyPrefix);
            }
        }
        
        public async Task<int> GetMany(
            TOuterKey outerKey,
            ReadOnlyMemory<TInnerKey> innerKeys,
            Memory<KeyValuePair<TInnerKey, ValueAndTimeToLive<TValue>>> destination)
        {
            CheckDisposed();

            var redisDb = GetDatabase();

            var redisKeyPrefix = _outerKeySerializer(outerKey).Append(_keySeparator);

            var valuesFoundCount = 0;
            
            var tasks = CreateTasks(out var pooledTasksArray);

            try
            {
                await Task
                    .WhenAll(tasks)
                    .ConfigureAwait(false);
                
                return valuesFoundCount == 0
                    ? 0
                    : CopyResultsToDestinationArray();
            }
            finally
            {
                ArrayPool<Task<(TInnerKey, RedisValueWithExpiry)>>.Shared.Return(pooledTasksArray);
            }

            IReadOnlyCollection<Task<(TInnerKey, RedisValueWithExpiry)>> CreateTasks(out Task<(TInnerKey, RedisValueWithExpiry)>[] pooledArray)
            {
                pooledArray = ArrayPool<Task<(TInnerKey, RedisValueWithExpiry)>>.Shared.Rent(innerKeys.Length);
                
                var i = 0;
                foreach (var innerKey in innerKeys.Span)
                    pooledArray[i++] = GetSingle(innerKey);

                return new ArraySegment<Task<(TInnerKey, RedisValueWithExpiry)>>(pooledArray, 0, innerKeys.Length);
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
                for (var taskIndex = 0; taskIndex < innerKeys.Length; taskIndex++)
                {
                    var (key, fromRedis) = pooledTasksArray[taskIndex].Result;

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
            ReadOnlyMemory<KeyValuePair<TInnerKey, TValue>> values,
            TimeSpan timeToLive)
        {
            CheckDisposed();

            var redisDb = GetDatabase();

            var redisKeyPrefix = _outerKeySerializer(outerKey).Append(_keySeparator);
            
            var pooledTasksArray = ArrayPool<Task>.Shared.Rent(values.Length);
            var toDispose = _serializeValuesToStreams
                ? ArrayPool<IDisposable>.Shared.Rent(values.Length)
                : null;
            
            var streamIndex = 0;
            try
            {
                var tasks = CreateTasks();

                var waitForAllTasksTask = Task.WhenAll(tasks);

                if (!waitForAllTasksTask.IsCompleted)
                    await waitForAllTasksTask.ConfigureAwait(false);
            }
            finally
            {
                ArrayPool<Task>.Shared.Return(pooledTasksArray);
                
                if (!(toDispose is null))
                {
                    for (var i = 0; i < streamIndex; i++)
                        toDispose[i].Dispose();
                    
                    ArrayPool<IDisposable>.Shared.Return(toDispose);
                }
            }

            IReadOnlyCollection<Task> CreateTasks()
            {
                var index = 0;
                foreach (var kv in values.Span)
                {
                    var redisKey = redisKeyPrefix.Append(_innerKeySerializer(kv.Key));

                    var redisValue = _redisValueConverter.ConvertToRedisValue(kv.Value, out var stream);

                    if (!(toDispose is null))
                        toDispose[streamIndex++] = stream;

                    _recentlySetKeysManager?.Mark(((string)redisKey).Substring(_keyPrefixLength));
                    
                    pooledTasksArray[index++] = redisDb.StringSetAsync(redisKey, redisValue, timeToLive, flags: _setValueFlags);
                }
                
                return new ArraySegment<Task>(pooledTasksArray, 0, index);
            }
        }
        
        public async Task<bool> TryRemove(TOuterKey outerKey, TInnerKey innerKey)
        {
            CheckDisposed();
            
            var redisDb = GetDatabase();
            
            var redisKey = _outerKeySerializer(outerKey)
                .Append(_keySeparator)
                .Append(_innerKeySerializer(innerKey));

            _recentlyRemovedKeysManager?.Mark(((string)redisKey).Substring(_keyPrefixLength));
            
            return await redisDb
                .KeyDeleteAsync(redisKey)
                .ConfigureAwait(false);
        }

        public IObservable<(string OuterKey, string InnerKey, KeyEventType EventType)> KeysChangedRemotely =>
            _keysChangedRemotely?.AsObservable() ?? Observable.Empty<(string, string, KeyEventType)>();

        public void Dispose()
        {
            if (_disposed)
                return;
            
            _keysChangedRemotely?.Dispose();
            _recentlySetKeysManager?.Dispose();
            _recentlyRemovedKeysManager?.Dispose();
            _disposed = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CheckDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(this.GetType().ToString());
        }
        
        private void OnKeyChanged(string redisKey, KeyEventType eventType)
        {
            bool wasLocalChange;
            if (eventType == KeyEventType.Set)
                _recentlySetKeysManager.HandleKeyChangedNotification(redisKey, out wasLocalChange);
            else if (eventType == KeyEventType.Del)
                _recentlyRemovedKeysManager.HandleKeyChangedNotification(redisKey, out wasLocalChange);
            else
                wasLocalChange = false;

            if (wasLocalChange)
                return;

            string outerKey, innerKey;
            if (_keySeparator.Equals(default))
            {
                outerKey = redisKey;
                innerKey = null;
            }
            else
            {
                var index = redisKey.IndexOf(_keySeparator);
                outerKey = redisKey.Substring(0, index);
                innerKey = redisKey.Substring(index + 1);
            }

            _keysChangedRemotely.OnNext((outerKey, innerKey, eventType));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private IDatabase GetDatabase() => _connection.Get().GetDatabase(_dbIndex);
    }
}
