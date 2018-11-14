using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using CacheMeIfYouCan.Caches;
using StackExchange.Redis;

namespace CacheMeIfYouCan.Redis
{
    internal class RedisCache<TK, TV> : ICache<TK, TV>, IKeyChangeNotifier<TK>
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
            int database,
            string keySpacePrefix,
            Func<string, TK> keyDeserializer,
            Func<TV, string> serializer,
            Func<string, TV> deserializer)
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

            connection.SubscribeToKeyChanges(_database, NotifyKeyChanged);
        }

        public string CacheName { get; }
        public string CacheType { get; } = "redis";
        
        // Must get keys separately since multikey operations will fail if running Redis in cluster mode
        public async Task<IList<GetFromCacheResult<TK, TV>>> Get(ICollection<Key<TK>> keys)
        {
            var redisDb = GetDatabase();

            async Task<KeyValuePair<Key<TK>, RedisValueWithExpiry>> GetSingle(Key<TK> key)
            {
                var valueWithExpiry = await redisDb.StringGetWithExpiryAsync(_toRedisKey(key));
                
                return new KeyValuePair<Key<TK>, RedisValueWithExpiry>(key, valueWithExpiry);
            }

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
        }

        // Must set keys separately since multikey operations will fail if running Redis in cluster mode
        public async Task Set(ICollection<KeyValuePair<Key<TK>, TV>> values, TimeSpan timeToLive)
        {
            var redisDb = GetDatabase();
            
            async Task SetSingle(Key<TK> key, TV value)
            {
                var redisKey = _toRedisKey(key);

                var serializedValue = _serializer(value);

                _recentlySetKeysManager?.Mark(key);
            
                await redisDb.StringSetAsync(redisKey, serializedValue, timeToLive);
            }
            
            var tasks = values
                .Select(kv => SetSingle(kv.Key, kv.Value))
                .ToArray();

            await Task.WhenAll(tasks);
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