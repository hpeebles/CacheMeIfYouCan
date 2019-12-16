using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CacheMeIfYouCan.Internal
{
    internal sealed class TwoTierCache<TKey, TValue> : ICache<TKey, TValue>
    {
        private readonly ILocalCache<TKey, TValue> _localCache;
        private readonly IDistributedCache<TKey, TValue> _distributedCache;
        private readonly IEqualityComparer<TKey> _keyComparer;

        public TwoTierCache(
            ILocalCache<TKey, TValue> localCache,
            IDistributedCache<TKey, TValue> distributedCache,
            IEqualityComparer<TKey> keyComparer)
        {
            _localCache = localCache ?? throw new ArgumentNullException(nameof(localCache));
            _distributedCache = distributedCache ?? throw new ArgumentNullException(nameof(distributedCache));
            _keyComparer = keyComparer;
        }

        public ValueTask<(bool Success, TValue Value)> TryGet(TKey key)
        {
            if (_localCache.TryGet(key, out var value))
                return new ValueTask<(bool Success, TValue Value)>((true, value));

            var task = GetFromDistributedCache();
            
            return new ValueTask<(bool Success, TValue Value)>(task);

            async Task<(bool, TValue)> GetFromDistributedCache()
            {
                var (success, valueAndTimeToLive) = await _distributedCache
                    .TryGet(key)
                    .ConfigureAwait(false);

                if (success)
                    _localCache.Set(key, valueAndTimeToLive.Value, valueAndTimeToLive.TimeToLive);

                return (success, valueAndTimeToLive);
            }
        }

        public ValueTask Set(TKey key, TValue value, TimeSpan timeToLive)
        {
            _localCache.Set(key, value, timeToLive);
            
            return new ValueTask(_distributedCache.Set(key, value, timeToLive));
        }

        public ValueTask<IReadOnlyCollection<KeyValuePair<TKey, TValue>>> GetMany(IReadOnlyCollection<TKey> keys)
        {
            var fromLocalCache = _localCache.GetMany(keys);

            var resultsDictionary = fromLocalCache is null
                ? new Dictionary<TKey, TValue>(_keyComparer)
                : fromLocalCache.ToDictionary(kv => kv.Key, kv => kv.Value, _keyComparer);

            IReadOnlyCollection<TKey> missingKeys;
            if (resultsDictionary.Count == 0)
            {
                missingKeys = keys;
            }
            else if (resultsDictionary.Count < keys.Count)
            {
                var missingKeysList = new List<TKey>(keys.Count - resultsDictionary.Count);
                foreach (var key in keys)
                {
                    if (resultsDictionary.ContainsKey(key))
                        continue;
             
                    missingKeysList.Add(key);
                }
                
                missingKeys = missingKeysList;
            }
            else
            {
                List<TKey> missingKeysList = null;
                foreach (var key in keys)
                {
                    if (resultsDictionary.ContainsKey(key))
                        continue;
                    
                    if (missingKeysList == null)
                        missingKeysList = new List<TKey>();

                    missingKeysList.Add(key);
                }
                
                missingKeys = missingKeysList;
                if (missingKeys == null)
                    return new ValueTask<IReadOnlyCollection<KeyValuePair<TKey, TValue>>>(resultsDictionary);
            }

            var task = GetFromDistributedCache();
            
            return new ValueTask<IReadOnlyCollection<KeyValuePair<TKey, TValue>>>(task);

            async Task<IReadOnlyCollection<KeyValuePair<TKey, TValue>>> GetFromDistributedCache()
            {
                var fromDistributedCache = await _distributedCache
                    .GetMany(missingKeys)
                    .ConfigureAwait(false);

                foreach (var kv in fromDistributedCache)
                {
                    _localCache.Set(kv.Key, kv.Value.Value, kv.Value.TimeToLive);
                    resultsDictionary[kv.Key] = kv.Value.Value;
                }

                return resultsDictionary;
            }
        }

        public ValueTask SetMany(IReadOnlyCollection<KeyValuePair<TKey, TValue>> values, TimeSpan timeToLive)
        {
            _localCache.SetMany(values, timeToLive);
            
            return new ValueTask(_distributedCache.SetMany(values, timeToLive));
        }
    }
}