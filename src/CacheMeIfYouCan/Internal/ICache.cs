using System;
using System.Buffers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CacheMeIfYouCan.Internal
{
    internal interface ICache<TKey, TValue>
    {
        bool LocalCacheEnabled { get; }
        
        bool DistributedCacheEnabled { get; }
        
        ValueTask<(bool Success, TValue Value, CacheGetStats Stats)> TryGet(TKey key);

        ValueTask Set(TKey key, TValue value, TimeSpan timeToLive);

        ValueTask<CacheGetManyStats> GetMany(IReadOnlyCollection<TKey> keys, int cacheKeysSkipped, Memory<KeyValuePair<TKey, TValue>> destination);

        ValueTask SetMany(IReadOnlyCollection<KeyValuePair<TKey, TValue>> values, TimeSpan timeToLive);
    }

    internal interface ICache<in TOuterKey, TInnerKey, TValue>
    {
        ValueTask<int> GetMany(TOuterKey outerKey, IReadOnlyCollection<TInnerKey> innerKeys, Memory<KeyValuePair<TInnerKey, TValue>> destination);

        ValueTask SetMany(TOuterKey outerKey, IReadOnlyCollection<KeyValuePair<TInnerKey, TValue>> values, TimeSpan timeToLive);
    }

    internal static class ICacheExtensions
    {
        public static async ValueTask<KeyValuePair<TKey, TValue>[]> GetMany<TKey, TValue>(
            this ICache<TKey, TValue> cache,
            IReadOnlyCollection<TKey> keys)
        {
            var pooledArray = ArrayPool<KeyValuePair<TKey, TValue>>.Shared.Rent(keys.Count);
            try
            {
                var memory = new Memory<KeyValuePair<TKey, TValue>>(pooledArray);
                
                var countFoundTask = cache.GetMany(keys, 0, memory);

                if (!countFoundTask.IsCompleted)
                    await countFoundTask.ConfigureAwait(false);

                return memory.Slice(0, countFoundTask.Result.CacheHits).ToArray();
            }
            finally
            {
                ArrayPool<KeyValuePair<TKey, TValue>>.Shared.Return(pooledArray);
            }
        }

        public static async ValueTask<KeyValuePair<TInnerKey, TValue>[]> GetMany<TOuterKey, TInnerKey, TValue>(
            this ICache<TOuterKey, TInnerKey, TValue> cache,
            TOuterKey outerKey,
            IReadOnlyCollection<TInnerKey> innerKeys)
        {
            var pooledArray = ArrayPool<KeyValuePair<TInnerKey, TValue>>.Shared.Rent(innerKeys.Count);
            try
            {
                var memory = new Memory<KeyValuePair<TInnerKey, TValue>>(pooledArray);

                var countFound = await cache
                    .GetMany(outerKey, innerKeys, memory)
                    .ConfigureAwait(false);

                return memory.Slice(0, countFound).ToArray();
            }
            finally
            {
                ArrayPool<KeyValuePair<TInnerKey, TValue>>.Shared.Return(pooledArray);
            }
        }
    }
}