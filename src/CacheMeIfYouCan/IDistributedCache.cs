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

        Task<int> GetMany(IReadOnlyCollection<TKey> keys, Memory<KeyValuePair<TKey, ValueAndTimeToLive<TValue>>> destination);

        Task SetMany(IReadOnlyCollection<KeyValuePair<TKey, TValue>> values, TimeSpan timeToLive);
    }
    
    public interface IDistributedCache<in TOuterKey, TInnerKey, TValue>
    {
        Task<int> GetMany(TOuterKey outerKey, IReadOnlyCollection<TInnerKey> innerKeys, Memory<KeyValuePair<TInnerKey, ValueAndTimeToLive<TValue>>> destination);
        
        Task SetMany(TOuterKey outerKey, IReadOnlyCollection<KeyValuePair<TInnerKey, TValue>> values, TimeSpan timeToLive);
    }
    
    public static class IDistributedCacheExtensions
    {
        public static async Task<KeyValuePair<TKey, ValueAndTimeToLive<TValue>>[]> GetMany<TKey, TValue>(
            this IDistributedCache<TKey, TValue> cache,
            IReadOnlyCollection<TKey> keys)
        {
            var pooledArray = ArrayPool<KeyValuePair<TKey, ValueAndTimeToLive<TValue>>>.Shared.Rent(keys.Count);
            try
            {
                var memory = new Memory<KeyValuePair<TKey, ValueAndTimeToLive<TValue>>>(pooledArray);
                
                var countFound = await cache
                    .GetMany(keys, memory)
                    .ConfigureAwait(false);

                return memory.Slice(0, countFound).ToArray();
            }
            finally
            {
                ArrayPool<KeyValuePair<TKey, ValueAndTimeToLive<TValue>>>.Shared.Return(pooledArray);
            }
        }
        
        public static async Task<KeyValuePair<TInnerKey, ValueAndTimeToLive<TValue>>[]> GetMany<TOuterKey, TInnerKey, TValue>(
            this IDistributedCache<TOuterKey, TInnerKey, TValue> cache,
            TOuterKey outerKey,
            IReadOnlyCollection<TInnerKey> innerKeys)
        {
            var pooledArray = ArrayPool<KeyValuePair<TInnerKey, ValueAndTimeToLive<TValue>>>.Shared.Rent(innerKeys.Count);
            try
            {
                var memory = new Memory<KeyValuePair<TInnerKey, ValueAndTimeToLive<TValue>>>(pooledArray);

                var countFound = await cache
                    .GetMany(outerKey, innerKeys, memory)
                    .ConfigureAwait(false);

                return memory.Slice(0, countFound).ToArray();
            }
            finally
            {
                ArrayPool<KeyValuePair<TInnerKey, ValueAndTimeToLive<TValue>>>.Shared.Return(pooledArray);
            }
        }
    }
}