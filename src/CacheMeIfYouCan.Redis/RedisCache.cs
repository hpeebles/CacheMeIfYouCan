using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CacheMeIfYouCan.Caches;
using StackExchange.Redis;

namespace CacheMeIfYouCan.Redis
{
    internal class RedisCache<TK, TV> : ICache<TK, TV>
    {
        private const string CacheType = "redis";
        private readonly RedisConnection _connection;
        private readonly int _database;
        private readonly string _keySpacePrefix;
        private readonly Func<string, TK> _keyDeserializer;
        private readonly Func<TV, string> _serializer;
        private readonly Func<string, TV> _deserializer;
        private readonly Action<Key<TK>> _removeFromLocalCacheAction;
        private readonly Func<string, string> _toRedisKey;
        private readonly Func<string, string> _fromRedisKey;
        private readonly RecentlySetKeysManager _recentlySetKeysManager;

        public RedisCache(
            RedisConnection connection,
            int database,
            string keySpacePrefix,
            Func<string, TK> keyDeserializer,
            Func<TV, string> serializer,
            Func<string, TV> deserializer,
            Action<Key<TK>> removeFromLocalCacheAction = null)
        {
            _connection = connection;
            _database = database;
            _keySpacePrefix = keySpacePrefix;
            _keyDeserializer = keyDeserializer;
            _serializer = serializer;
            _deserializer = deserializer;
            _removeFromLocalCacheAction = removeFromLocalCacheAction;
            
            if (String.IsNullOrWhiteSpace(keySpacePrefix))
            {
                _toRedisKey = k => k;
                _fromRedisKey = k => k;
            }
            else
            {
                _toRedisKey = k => $"{keySpacePrefix}{k}";
                _fromRedisKey = k => k.Substring(keySpacePrefix.Length);
            }

            if (_removeFromLocalCacheAction != null)
            {
                _recentlySetKeysManager = new RecentlySetKeysManager();

                connection.RegisterOnKeyChangedCallback(_database, RemoveKeyFromMemoryIfNotRecentlySet);
            }
        }

        // Must get keys separately since multikey operations will fail if running Redis in cluster mode
        public async Task<IList<GetFromCacheResult<TK, TV>>> Get(ICollection<Key<TK>> keys)
        {
            var tasks = keys
                .Select(GetSingle)
                .ToArray();

            await Task.WhenAll(tasks);

            return tasks
                .Select(t => t.Result)
                .ToArray();
        }

        // Must set keys separately since multikey operations will fail if running Redis in cluster mode
        public async Task Set(ICollection<KeyValuePair<Key<TK>, TV>> values, TimeSpan timeToLive)
        {
            var tasks = values
                .Select(kv => SetSingle(kv.Key, kv.Value, timeToLive))
                .ToArray();

            await Task.WhenAll(tasks);
        }

        private async Task<GetFromCacheResult<TK, TV>> GetSingle(Key<TK> key)
        {
            var redisDb = GetDatabase();
            var stringKey = key.AsString;
            var redisKey = _toRedisKey(stringKey);

            var fromRedis = await redisDb.StringGetWithExpiryAsync(redisKey);
            
            if (!fromRedis.Value.HasValue)
                return GetFromCacheResult<TK, TV>.NotFound(key);

            var value = _deserializer(fromRedis.Value);
            var timeToLive = fromRedis.Expiry.GetValueOrDefault();

            return new GetFromCacheResult<TK, TV>(key, value, timeToLive, CacheType);
        }
        
        private async Task SetSingle(Key<TK> key, TV value, TimeSpan timeToLive)
        {
            var redisDb = GetDatabase();
            var stringKey = key.AsString;
            var redisKey = _toRedisKey(stringKey);

            var serializedValue = _serializer(value);

            _recentlySetKeysManager?.Mark(stringKey);
            
            await redisDb.StringSetAsync(redisKey, serializedValue, timeToLive);
        }

        private void RemoveKeyFromMemoryIfNotRecentlySet(string redisKey)
        {
            // Ignore keys that are not from the same keyspace
            if (!String.IsNullOrWhiteSpace(_keySpacePrefix) && !redisKey.StartsWith(_keySpacePrefix))
                return;

            var stringKey = _fromRedisKey(redisKey);
            
            if (_recentlySetKeysManager.IsRecentlySet(stringKey))
                return;

            var key = new Key<TK>(_keyDeserializer(stringKey), stringKey);

            _removeFromLocalCacheAction(key);
        }

        private IDatabase GetDatabase()
        {
            return _connection.Get().GetDatabase(_database);
        }
    }
}