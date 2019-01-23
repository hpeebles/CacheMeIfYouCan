using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace CacheMeIfYouCan.Redis
{
    internal class RedisCache<TK, TV> : IDistributedCache<TK, TV>, INotifyKeyChanges<TK>
    {
        private readonly RedisConnection _connection;
        private readonly int _database;
        private readonly string _keySpacePrefix;
        private readonly Func<string, TK> _keyDeserializer;
        private readonly Func<TV, string> _serializer;
        private readonly Func<string, TV> _deserializer;
        private readonly Func<string, string> _toRedisKey;
        private readonly Func<string, string> _fromRedisKey;
        private readonly RecentlySetKeysManager _recentlySetKeysManager;
        private readonly Subject<Key<TK>> _keyChanges;

        public RedisCache(
            RedisConnection connection,
            string cacheName,
            int database,
            string keySpacePrefix,
            Func<string, TK> keyDeserializer,
            Func<TV, string> serializer,
            Func<string, TV> deserializer,
            bool subscribeToKeyChanges)
        {
            _connection = connection;
            _database = database;
            _keySpacePrefix = keySpacePrefix;
            _keyDeserializer = keyDeserializer;
            _serializer = serializer;
            _deserializer = deserializer;
            
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

            _recentlySetKeysManager = new RecentlySetKeysManager();
            _keyChanges = new Subject<Key<TK>>();

            CacheName = cacheName;

            if (subscribeToKeyChanges)
            {
                NotifyKeyChangesEnabled = true;

                connection.SubscribeToKeyChanges(_database, NotifyKeyChanged);
            }
        }

        public string CacheName { get; }
        public string CacheType { get; } = "redis";
        public bool NotifyKeyChangesEnabled { get; }
        
        public void Dispose() { }
        
        public async Task<GetFromCacheResult<TK, TV>> Get(Key<TK> key)
        {
            var redisDb = GetDatabase();

            var valueWithExpiry = await redisDb.StringGetWithExpiryAsync(_toRedisKey(key.AsString));

            if (!valueWithExpiry.Value.HasValue)
                return new GetFromCacheResult<TK, TV>();
                    
            return new GetFromCacheResult<TK, TV>(
                key,
                _deserializer(valueWithExpiry.Value),
                valueWithExpiry.Expiry.GetValueOrDefault(),
                CacheType);
        }

        public async Task Set(Key<TK> key, TV value, TimeSpan timeToLive)
        {
            var redisDb = GetDatabase();
            
            var redisKey = _toRedisKey(key.AsString);

            var serializedValue = _serializer(value);
            
            _recentlySetKeysManager?.Mark(key.AsString);
        
            await redisDb.StringSetAsync(redisKey, serializedValue, timeToLive);
        }

        // Must get keys separately since multi key operations will fail if running Redis in cluster mode
        public async Task<IList<GetFromCacheResult<TK, TV>>> Get(ICollection<Key<TK>> keys)
        {
            var redisDb = GetDatabase();

            var tasks = keys
                .Select(GetSingle)
                .ToArray();

            await Task.WhenAll(tasks);

            return tasks
                .Select(t => t.Result)
                .Where(kv => kv.Value.Value.HasValue)
                .Select(kv => new GetFromCacheResult<TK, TV>(
                    kv.Key,
                    _deserializer(kv.Value.Value),
                    kv.Value.Expiry.GetValueOrDefault(),
                    CacheType))
                .ToArray();
            
            async Task<KeyValuePair<Key<TK>, RedisValueWithExpiry>> GetSingle(Key<TK> key)
            {
                var valueWithExpiry = await redisDb.StringGetWithExpiryAsync(_toRedisKey(key.AsString));
                
                return new KeyValuePair<Key<TK>, RedisValueWithExpiry>(key, valueWithExpiry);
            }
        }

        // Must set keys separately since multi key operations will fail if running Redis in cluster mode
        public async Task Set(ICollection<KeyValuePair<Key<TK>, TV>> values, TimeSpan timeToLive)
        {
            var redisDb = GetDatabase();
            
            async Task SetSingle(string key, string value)
            {
                var redisKey = _toRedisKey(key);

                _recentlySetKeysManager?.Mark(key);
            
                await redisDb.StringSetAsync(redisKey, value, timeToLive);
            }
            
            var tasks = values
                .Select(kv => SetSingle(kv.Key.AsString, _serializer(kv.Value)))
                .ToArray();

            await Task.WhenAll(tasks);
        }

        public async Task Remove(Key<TK> key)
        {
            var redisDb = GetDatabase();
            
            var redisKey = _toRedisKey(key.AsString);

            await redisDb.KeyDeleteAsync(redisKey);
        }

        public IObservable<Key<TK>> KeyChanges => _keyChanges.AsObservable();

        private void NotifyKeyChanged(string redisKey)
        {
            // Ignore keys that are not from the same keyspace
            if (!String.IsNullOrWhiteSpace(_keySpacePrefix) && !redisKey.StartsWith(_keySpacePrefix))
                return;

            var stringKey = _fromRedisKey(redisKey);
            
            if (_recentlySetKeysManager.IsRecentlySet(stringKey))
                return;

            var key = new Key<TK>(_keyDeserializer(stringKey), stringKey);

            _keyChanges.OnNext(key);
        }

        private IDatabase GetDatabase()
        {
            return _connection.Get().GetDatabase(_database);
        }
    }
}