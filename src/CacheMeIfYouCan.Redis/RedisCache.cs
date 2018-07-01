using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace CacheMeIfYouCan.Redis
{
    internal class RedisCache<T> : ICache<T>
    {
        private const string CacheType = "Redis";
        private readonly IConnectionMultiplexer _multiplexer;
        private readonly int _database;
        private readonly string _keySpacePrefix;
        private readonly MemoryCache<T> _memoryCache;
        private readonly Func<T, string> _serializer;
        private readonly Func<string, T> _deserializer;
        private readonly Func<string, string> _toRedisKey;
        private readonly Func<string, string> _fromRedisKey;
        private readonly RecentlySetKeysManager _recentlySetKeysManager;
        
        public RedisCache(
            IConnectionMultiplexer multiplexer,
            int database,
            MemoryCache<T> memoryCache,
            string keySpacePrefix,
            Func<T, string> serializer,
            Func<string, T> deserializer)
        {
            _multiplexer = multiplexer;
            _database = database;
            _memoryCache = memoryCache;
            _keySpacePrefix = keySpacePrefix;
            _serializer = serializer;
            _deserializer = deserializer;
            
            if (String.IsNullOrWhiteSpace(keySpacePrefix))
            {
                _toRedisKey = k => k;
                _fromRedisKey = k => k;
            }
            else
            {
                _toRedisKey = k => $"{keySpacePrefix}_{k}";
                _fromRedisKey = k => k.Substring(keySpacePrefix.Length + 1);
            }

            if (_memoryCache != null)
            {
                _recentlySetKeysManager = new RecentlySetKeysManager();

                // All Redis instances must have keyevent notifications enabled (eg. 'notify-keyspace-events AE')
                var subscriber = multiplexer.GetSubscriber();

                var keyEvents = new[]
                {
                    "set",
                    "del",
                    "expired",
                    "evicted"
                };

                foreach (var keyEvent in keyEvents)
                    subscriber.Subscribe($"__keyevent@{_database}__:{keyEvent}", (c, k) => RemoveKeyFromMemoryIfNotRecentlySet(k));
            }
        }

        public async Task<GetFromCacheResult<T>> Get(string key)
        {
            if (_memoryCache != null)
            {
                var fromMemoryCache = await _memoryCache.Get(key);
                
                if (fromMemoryCache.Success)
                    return fromMemoryCache;
            }

            var redisDb = GetDatabase();
            var redisKey = _toRedisKey(key);

            var fromRedis = await redisDb.StringGetWithExpiryAsync(redisKey);
            
            if (!fromRedis.Value.HasValue)
                return GetFromCacheResult<T>.NotFound;

            var value = _deserializer(fromRedis.Value);
            var timeToLive = fromRedis.Expiry.GetValueOrDefault();

            if (_memoryCache != null)
                await _memoryCache.Set(key, value, timeToLive);
            
            return new GetFromCacheResult<T>(value, timeToLive, CacheType);
        }

        public async Task Set(string key, T value, TimeSpan timeToLive)
        {
            if (_memoryCache != null)
                await _memoryCache.Set(key, value, timeToLive);

            var redisDb = GetDatabase();
            var redisKey = _toRedisKey(key);
            var serialized = _serializer(value);

            _recentlySetKeysManager?.Mark(key);
            
            await redisDb.StringSetAsync(redisKey, serialized, timeToLive);
        }

        private void RemoveKeyFromMemoryIfNotRecentlySet(string redisKey)
        {
            // Ignore keys that are not from the same keyspace
            if (!String.IsNullOrWhiteSpace(_keySpacePrefix) && !redisKey.StartsWith(_keySpacePrefix))
                return;

            var key = _fromRedisKey(redisKey);
            
            if (_recentlySetKeysManager.IsRecentlySet(key))
                return;

            _memoryCache.Remove(key);
        }

        private IDatabase GetDatabase()
        {
            return _multiplexer.GetDatabase(_database);
        }
    }
}