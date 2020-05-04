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

        ValueTask<CacheGetManyStats> GetMany(ReadOnlyMemory<TKey> keys, int cacheKeysSkipped, Memory<KeyValuePair<TKey, TValue>> destination);

        ValueTask SetMany(ReadOnlyMemory<KeyValuePair<TKey, TValue>> values, TimeSpan timeToLive);
    }

    internal interface ICache<in TOuterKey, TInnerKey, TValue>
    {
        bool LocalCacheEnabled { get; }
        
        bool DistributedCacheEnabled { get; }
        
        ValueTask<CacheGetManyStats> GetMany(TOuterKey outerKey, ReadOnlyMemory<TInnerKey> innerKeys, int cacheKeysSkipped, Memory<KeyValuePair<TInnerKey, TValue>> destination);

        ValueTask SetMany(TOuterKey outerKey, ReadOnlyMemory<KeyValuePair<TInnerKey, TValue>> values, TimeSpan timeToLive);
    }

    internal static class ICacheExtensions
    {
        public static async ValueTask<KeyValuePair<TKey, TValue>[]> GetMany<TKey, TValue>(
            this ICache<TKey, TValue> cache,
            ReadOnlyMemory<TKey> keys)
        {
            using var memoryOwner = MemoryPool<KeyValuePair<TKey, TValue>>.Shared.Rent(keys.Length);
            var memory = memoryOwner.Memory;
            var countFoundTask = cache.GetMany(keys, 0, memory);

            if (!countFoundTask.IsCompleted)
                await countFoundTask.ConfigureAwait(false);

            return memory.Slice(0, countFoundTask.Result.CacheHits).ToArray();
        }

        public static async ValueTask<KeyValuePair<TInnerKey, TValue>[]> GetMany<TOuterKey, TInnerKey, TValue>(
            this ICache<TOuterKey, TInnerKey, TValue> cache,
            TOuterKey outerKey,
            ReadOnlyMemory<TInnerKey> innerKeys)
        {
            using var memoryOwner = MemoryPool<KeyValuePair<TInnerKey, TValue>>.Shared.Rent(innerKeys.Length);
            var memory = memoryOwner.Memory;

            var cacheStats = await cache
                .GetMany(outerKey, innerKeys, 0, memory)
                .ConfigureAwait(false);

            return memory.Slice(0, cacheStats.CacheHits).ToArray();
        }
    }
}