using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CacheMeIfYouCan.Internal
{
    internal sealed class DistributedCacheAdapter<TKey, TValue> : ICache<TKey, TValue>
    {
        private readonly IDistributedCache<TKey, TValue> _innerCache;

        public DistributedCacheAdapter(IDistributedCache<TKey, TValue> innerCache)
        {
            _innerCache = innerCache;
        }

        public async ValueTask<(bool Success, TValue Value)> TryGet(TKey key)
        {
            var (success, valueAndTimeToLive) = await _innerCache
                .TryGet(key)
                .ConfigureAwait(false);
            
            return success ? (true, valueAndTimeToLive.Value) : (false, default);
        }

        public ValueTask Set(TKey key, TValue value, TimeSpan timeToLive)
        {
            return new ValueTask(_innerCache.Set(key, value, timeToLive));
        }

        public async ValueTask<IReadOnlyCollection<KeyValuePair<TKey, TValue>>> GetMany(IReadOnlyCollection<TKey> keys)
        {
            var resultsWithTimeToLive = await _innerCache
                .GetMany(keys)
                .ConfigureAwait(false);

            var results = new List<KeyValuePair<TKey, TValue>>(resultsWithTimeToLive.Count);
            foreach (var kv in resultsWithTimeToLive)
                results.Add(new KeyValuePair<TKey, TValue>(kv.Key, kv.Value));

            return results;
        }

        public ValueTask SetMany(IReadOnlyCollection<KeyValuePair<TKey, TValue>> values, TimeSpan timeToLive)
        {
            return new ValueTask(_innerCache.SetMany(values, timeToLive));
        }
    }
}