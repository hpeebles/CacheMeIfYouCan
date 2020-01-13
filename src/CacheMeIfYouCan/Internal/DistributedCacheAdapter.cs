using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CacheMeIfYouCan.Internal
{
    internal sealed class DistributedCacheAdapter<TKey, TValue> : ICache<TKey, TValue>
    {
        private readonly IDistributedCache<TKey, TValue> _innerCache;
        private static readonly IReadOnlyCollection<KeyValuePair<TKey, TValue>> EmptyResults = new List<KeyValuePair<TKey, TValue>>();

        public DistributedCacheAdapter(IDistributedCache<TKey, TValue> innerCache)
        {
            _innerCache = innerCache;
        }

        public async ValueTask<(bool Success, TValue Value)> TryGet(TKey key)
        {
            var (success, valueAndTimeToLive) = await _innerCache
                .TryGet(key)
                .ConfigureAwait(false);

            return success
                ? (true, valueAndTimeToLive.Value)
                : default;
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

            if (resultsWithTimeToLive is null)
                return EmptyResults;
            
            var results = new List<KeyValuePair<TKey, TValue>>(resultsWithTimeToLive.Count);
            foreach (var kv in resultsWithTimeToLive)
                results.Add(new KeyValuePair<TKey, TValue>(kv.Key, kv.Value));

            return results;
        }

        public async ValueTask SetMany(IReadOnlyCollection<KeyValuePair<TKey, TValue>> values, TimeSpan timeToLive)
        {
            var task = _innerCache.SetMany(values, timeToLive);

            if (!task.IsCompleted)
                await task.ConfigureAwait(false);
        }
    }
    
    internal sealed class DistributedCacheAdapter<TOuterKey, TInnerKey, TValue> : ICache<TOuterKey, TInnerKey, TValue>
    {
        private readonly IDistributedCache<TOuterKey, TInnerKey, TValue> _innerCache;
        private static readonly IReadOnlyCollection<KeyValuePair<TInnerKey, TValue>> EmptyResults = new List<KeyValuePair<TInnerKey, TValue>>();

        public DistributedCacheAdapter(IDistributedCache<TOuterKey, TInnerKey, TValue> innerCache)
        {
            _innerCache = innerCache;
        }
        
        public async ValueTask<IReadOnlyCollection<KeyValuePair<TInnerKey, TValue>>> GetMany(
            TOuterKey outerKey,
            IReadOnlyCollection<TInnerKey> innerKeys)
        {
            var resultsWithTimeToLive = await _innerCache
                .GetMany(outerKey, innerKeys)
                .ConfigureAwait(false);

            if (resultsWithTimeToLive is null)
                return EmptyResults;
            
            var results = new List<KeyValuePair<TInnerKey, TValue>>(resultsWithTimeToLive.Count);
            foreach (var kv in resultsWithTimeToLive)
                results.Add(new KeyValuePair<TInnerKey, TValue>(kv.Key, kv.Value));

            return results;
        }

        public async ValueTask SetMany(
            TOuterKey outerKey,
            IReadOnlyCollection<KeyValuePair<TInnerKey, TValue>> values,
            TimeSpan timeToLive)
        {
            var task = _innerCache.SetMany(outerKey, values, timeToLive);

            if (!task.IsCompleted)
                await task.ConfigureAwait(false);
        }
    }
}