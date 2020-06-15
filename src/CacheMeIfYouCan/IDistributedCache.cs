using System;
using System.Buffers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CacheMeIfYouCan
{
    public interface IDistributedCache<TKey, TValue>
    {
        Task<(bool Success, ValueAndTimeToLive<TValue> Value)> TryGet(TKey key);

        Task Set(TKey key, TValue value, TimeSpan timeToLive);

        Task<int> GetMany(ReadOnlyMemory<TKey> keys, Memory<KeyValuePair<TKey, ValueAndTimeToLive<TValue>>> destination);

        Task SetMany(ReadOnlyMemory<KeyValuePair<TKey, TValue>> values, TimeSpan timeToLive);

        Task<bool> TryRemove(TKey key);
    }
    
    public interface IDistributedCache<in TOuterKey, TInnerKey, TValue>
    {
        Task<int> GetMany(TOuterKey outerKey, ReadOnlyMemory<TInnerKey> innerKeys, Memory<KeyValuePair<TInnerKey, ValueAndTimeToLive<TValue>>> destination);
        
        Task SetMany(TOuterKey outerKey, ReadOnlyMemory<KeyValuePair<TInnerKey, TValue>> values, TimeSpan timeToLive);

        Task<bool> TryRemove(TOuterKey outerKey, TInnerKey innerKey);
    }
    
    public static class IDistributedCacheExtensions
    {
        public static async Task<KeyValuePair<TKey, ValueAndTimeToLive<TValue>>[]> GetMany<TKey, TValue>(
            this IDistributedCache<TKey, TValue> cache,
            ReadOnlyMemory<TKey> keys)
        {
            using var memoryOwner = MemoryPool<KeyValuePair<TKey, ValueAndTimeToLive<TValue>>>.Shared.Rent(keys.Length);
            var memory = memoryOwner.Memory;

            var countFound = await cache
                .GetMany(keys, memory)
                .ConfigureAwait(false);

            return memory.Slice(0, countFound).ToArray();
        }

        public static async Task<KeyValuePair<TInnerKey, ValueAndTimeToLive<TValue>>[]> GetMany<TOuterKey, TInnerKey, TValue>(
            this IDistributedCache<TOuterKey, TInnerKey, TValue> cache,
            TOuterKey outerKey,
            ReadOnlyMemory<TInnerKey> innerKeys)
        {
            using var memoryOwner = MemoryPool<KeyValuePair<TInnerKey, ValueAndTimeToLive<TValue>>>.Shared.Rent(innerKeys.Length);
            var memory = memoryOwner.Memory;

            var countFound = await cache
                .GetMany(outerKey, innerKeys, memory)
                .ConfigureAwait(false);

            return memory.Slice(0, countFound).ToArray();
        }

        public static Task Set<TOuterKey, TInnerKey, TValue>(
            this IDistributedCache<TOuterKey, TInnerKey, TValue> cache,
            TOuterKey outerKey,
            TInnerKey innerKey,
            TValue value,
            TimeSpan timeToLive)
        {
            return cache.SetMany(outerKey, new[] { new KeyValuePair<TInnerKey, TValue>(innerKey , value) }, timeToLive);
        }
    }
}