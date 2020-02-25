using System;
using System.Buffers;
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

        public async ValueTask<int> GetMany(IReadOnlyCollection<TKey> keys, Memory<KeyValuePair<TKey, TValue>> destination)
        {
            var pooledArray = ArrayPool<KeyValuePair<TKey, ValueAndTimeToLive<TValue>>>.Shared.Rent(keys.Count);

            try
            {
                var countFound = await _innerCache
                    .GetMany(keys, pooledArray)
                    .ConfigureAwait(false);

                if (countFound > 0)
                    CopyResultsToDestination(countFound);

                return countFound;
            }
            finally
            {
                ArrayPool<KeyValuePair<TKey, ValueAndTimeToLive<TValue>>>.Shared.Return(pooledArray);
            }
            
            void CopyResultsToDestination(int countFound)
            {
                var span = destination.Span;
                for (var i = 0; i <= countFound; i++)
                {
                    var kv = pooledArray[i];
                    span[i] = new KeyValuePair<TKey, TValue>(kv.Key, kv.Value);
                }
            }
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
        
        public async ValueTask<int> GetMany(
            TOuterKey outerKey,
            IReadOnlyCollection<TInnerKey> innerKeys,
            Memory<KeyValuePair<TInnerKey, TValue>> destination)
        {
            var pooledArray = ArrayPool<KeyValuePair<TInnerKey, ValueAndTimeToLive<TValue>>>.Shared.Rent(innerKeys.Count);

            try
            {
                var countFound = await _innerCache
                    .GetMany(outerKey, innerKeys, pooledArray)
                    .ConfigureAwait(false);

                if (countFound > 0)
                    CopyResultsToDestination(countFound);
                
                return countFound;
            }
            finally
            {
                ArrayPool<KeyValuePair<TInnerKey, ValueAndTimeToLive<TValue>>>.Shared.Return(pooledArray);
            }

            void CopyResultsToDestination(int countFound)
            {
                var span = destination.Span;
                for (var i = 0; i <= countFound; i++)
                {
                    var kv = pooledArray[i];
                    span[i] = new KeyValuePair<TInnerKey, TValue>(kv.Key, kv.Value);
                }
            }
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