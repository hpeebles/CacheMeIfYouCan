using System;
using System.Buffers;
using System.Collections.Generic;

namespace CacheMeIfYouCan
{
    public interface ILocalCache<TKey, TValue>
    {
        bool TryGet(TKey key, out TValue value);

        void Set(TKey key, TValue value, TimeSpan timeToLive);

        int GetMany(IReadOnlyCollection<TKey> keys, Memory<KeyValuePair<TKey, TValue>> destination);

        void SetMany(IReadOnlyCollection<KeyValuePair<TKey, TValue>> values, TimeSpan timeToLive);

        bool TryRemove(TKey key, out TValue value);
    }

    public interface ILocalCache<in TOuterKey, TInnerKey, TValue>
    {
        int GetMany(TOuterKey outerKey, IReadOnlyCollection<TInnerKey> innerKeys, Memory<KeyValuePair<TInnerKey, TValue>> destination);
        
        void SetMany(TOuterKey outerKey, IReadOnlyCollection<KeyValuePair<TInnerKey, TValue>> values, TimeSpan timeToLive);

        void SetManyWithVaryingTimesToLive(TOuterKey outerKey, ReadOnlyMemory<KeyValuePair<TInnerKey, ValueAndTimeToLive<TValue>>> values);

        bool TryRemove(TOuterKey outerKey, TInnerKey innerKey, out TValue value);
    }

    public static class ILocalCacheExtensions
    {
        public static KeyValuePair<TKey, TValue>[] GetMany<TKey, TValue>(
            this ILocalCache<TKey, TValue> cache,
            IReadOnlyCollection<TKey> keys)
        {
            var pooledArray = ArrayPool<KeyValuePair<TKey, TValue>>.Shared.Rent(keys.Count);
            try
            {
                var memory = new Memory<KeyValuePair<TKey, TValue>>(pooledArray);

                var countFound = cache.GetMany(keys, memory);

                return memory.Slice(0, countFound).ToArray();
            }
            finally
            {
                ArrayPool<KeyValuePair<TKey, TValue>>.Shared.Return(pooledArray);
            }
        }
        
        public static KeyValuePair<TInnerKey, TValue>[] GetMany<TOuterKey, TInnerKey, TValue>(
            this ILocalCache<TOuterKey, TInnerKey, TValue> cache,
            TOuterKey outerKey,
            IReadOnlyCollection<TInnerKey> innerKeys)
        {
            var pooledArray = ArrayPool<KeyValuePair<TInnerKey, TValue>>.Shared.Rent(innerKeys.Count);
            try
            {
                var memory = new Memory<KeyValuePair<TInnerKey, TValue>>(pooledArray);

                var countFound = cache.GetMany(outerKey, innerKeys, memory);

                return memory.Slice(0, countFound).ToArray();
            }
            finally
            {
                ArrayPool<KeyValuePair<TInnerKey, TValue>>.Shared.Return(pooledArray);
            }
        }
    }
}