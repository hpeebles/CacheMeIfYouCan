using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CacheMeIfYouCan.Internal;

namespace CacheMeIfYouCan
{
    /// <summary>
    /// A two tier cache made up of a local cache backed by a distributed cache
    /// </summary>
    /// <typeparam name="TK">The type of the cache keys</typeparam>
    /// <typeparam name="TV">The type of the cache values</typeparam>
    public sealed class TwoTierCache<TK, TV> : ICache<TK, TV>
    {
        private readonly IDistributedCache<TK, TV> _distributedCache;
        private readonly ILocalCache<TK, TV> _localCache;
        private readonly IEqualityComparer<TK> _keyComparer;
        private readonly Func<TK, string> _keySerializer;

        public TwoTierCache(
            IDistributedCache<TK, TV> distributedCache,
            ILocalCache<TK, TV> localCache,
            IEqualityComparer<TK> keyComparer = null,
            Func<TK, string> keySerializer = null)
        {
            _distributedCache = distributedCache;
            _localCache = localCache;
            _keyComparer = keyComparer ?? KeyComparerResolver.GetInner<TK>();

            if (keySerializer != null)
            {
                _keySerializer = keySerializer;
            }
            else if (
                DefaultSettings.Cache.KeySerializers.TryGetSerializer<TK>(out var s) ||
                ProvidedSerializers.TryGetSerializer(out s))
            {
                _keySerializer = s;
            }
        }

        public async Task<TV> Get(TK key)
        {
            var k = new Key<TK>(key, _keySerializer);
            
            var fromLocal = _localCache.Get(k);

            if (fromLocal.Success)
                return fromLocal;

            var fromRemote = await _distributedCache.Get(k);

            if (!fromRemote.Success)
                return default;
            
            _localCache.Set(k, fromRemote.Value, fromRemote.TimeToLive);

            return fromRemote;
        }

        public async Task Set(TK key, TV value, TimeSpan timeToLive)
        {
            var k = new Key<TK>(key, _keySerializer);
            
            _localCache.Set(k, value, timeToLive);
            await _distributedCache.Set(k, value, timeToLive);
        }

        public async Task<IDictionary<TK, TV>> Get(ICollection<TK> keys)
        {
            var results = new Dictionary<TK, TV>(_keyComparer);

            var convertedKeys = keys
                .Select(k => new Key<TK>(k, _keySerializer))
                .ToArray();
            
            var fromLocal = _localCache.Get(convertedKeys);

            if (fromLocal != null)
            {
                foreach (var result in fromLocal)
                    results[result.Key] = result.Value;
            }
            
            var remaining = convertedKeys
                .Where(k => results.ContainsKey(k))
                .ToArray();

            if (remaining.Any())
            {
                var fromRemote = await _distributedCache.Get(remaining);

                if (fromRemote != null)
                {
                    foreach (var result in fromRemote)
                    {
                        results[result.Key] = result.Value;
                        _localCache.Set(result.Key, result.Value, result.TimeToLive);
                    }
                }
            }

            return results;
        }

        public async Task Set(ICollection<KeyValuePair<TK, TV>> values, TimeSpan timeToLive)
        {
            var convertedValues = values
                .Select(kv => new KeyValuePair<Key<TK>, TV>(new Key<TK>(kv.Key, _keySerializer), kv.Value))
                .ToArray();

            _localCache.Set(convertedValues, timeToLive);
            await _distributedCache.Set(convertedValues, timeToLive);
        }
    }
}