using System;
using System.Buffers;
using System.Collections.Generic;

namespace CacheMeIfYouCan
{
    public interface ILocalCache<TKey, TValue>
    {
        bool TryGet(TKey key, out TValue value);

        void Set(TKey key, TValue value, TimeSpan timeToLive);

        int GetMany(IReadOnlyCollection<TKey> keys, Span<KeyValuePair<TKey, TValue>> destination);

        void SetMany(IReadOnlyCollection<KeyValuePair<TKey, TValue>> values, TimeSpan timeToLive);

        bool TryRemove(TKey key, out TValue value);
    }

    public interface ILocalCache<in TOuterKey, TInnerKey, TValue>
    {
        int GetMany(TOuterKey outerKey, IReadOnlyCollection<TInnerKey> innerKeys, Span<KeyValuePair<TInnerKey, TValue>> destination);
        
        void SetMany(TOuterKey outerKey, IReadOnlyCollection<KeyValuePair<TInnerKey, TValue>> values, TimeSpan timeToLive);

        void SetManyWithVaryingTimesToLive(TOuterKey outerKey, ReadOnlySpan<KeyValuePair<TInnerKey, ValueAndTimeToLive<TValue>>> values);

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
                var span = new Span<KeyValuePair<TKey, TValue>>(pooledArray);

                var countFound = cache.GetMany(keys, span);

                return span.Slice(0, countFound).ToArray();
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
                var span = new Span<KeyValuePair<TInnerKey, TValue>>(pooledArray);

                var countFound = cache.GetMany(outerKey, innerKeys, span);

                return span.Slice(0, countFound).ToArray();
            }
            finally
            {
                ArrayPool<KeyValuePair<TInnerKey, TValue>>.Shared.Return(pooledArray);
            }
        }
        
        public static void Set<TOuterKey, TInnerKey, TValue>(
            this ILocalCache<TOuterKey, TInnerKey, TValue> cache,
            TOuterKey outerKey,
            TInnerKey innerKey,
            TValue value,
            TimeSpan timeToLive)
        {
            cache.SetMany(outerKey, new[] { new KeyValuePair<TInnerKey, TValue>(innerKey , value) }, timeToLive);
        }
    }
}