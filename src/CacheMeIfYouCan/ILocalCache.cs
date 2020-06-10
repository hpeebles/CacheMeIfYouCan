using System;
using System.Buffers;
using System.Collections.Generic;

namespace CacheMeIfYouCan
{
    public interface ILocalCache<TKey, TValue>
    {
        int Count { get; }
        void Clear();
        bool TryGet(TKey key, out TValue value);
        void Set(TKey key, TValue value, TimeSpan timeToLive);
        int GetMany(ReadOnlySpan<TKey> keys, Span<KeyValuePair<TKey, TValue>> destination);
        void SetMany(ReadOnlySpan<KeyValuePair<TKey, TValue>> values, TimeSpan timeToLive);
        bool TryRemove(TKey key, out TValue value);
    }

    public interface ILocalCache<in TOuterKey, TInnerKey, TValue>
    {
        int Count { get; }
        void Clear();
        int GetMany(TOuterKey outerKey, ReadOnlySpan<TInnerKey> innerKeys, Span<KeyValuePair<TInnerKey, TValue>> destination);
        void SetMany(TOuterKey outerKey, ReadOnlySpan<KeyValuePair<TInnerKey, TValue>> values, TimeSpan timeToLive);
        void SetManyWithVaryingTimesToLive(TOuterKey outerKey, ReadOnlySpan<KeyValuePair<TInnerKey, ValueAndTimeToLive<TValue>>> values);
        bool TryRemove(TOuterKey outerKey, TInnerKey innerKey, out TValue value);
    }

    public static class ILocalCacheExtensions
    {
        public static KeyValuePair<TKey, TValue>[] GetMany<TKey, TValue>(
            this ILocalCache<TKey, TValue> cache,
            ReadOnlySpan<TKey> keys)
        {
            using var memoryOwner = MemoryPool<KeyValuePair<TKey, TValue>>.Shared.Rent(keys.Length);
            var span = memoryOwner.Memory.Span;
            
            var countFound = cache.GetMany(keys, span);

            return span.Slice(0, countFound).ToArray();
        }
        
        public static KeyValuePair<TInnerKey, TValue>[] GetMany<TOuterKey, TInnerKey, TValue>(
            this ILocalCache<TOuterKey, TInnerKey, TValue> cache,
            TOuterKey outerKey,
            ReadOnlySpan<TInnerKey> innerKeys)
        {
            using var memoryOwner = MemoryPool<KeyValuePair<TInnerKey, TValue>>.Shared.Rent(innerKeys.Length);
            var span = memoryOwner.Memory.Span;
            
            var countFound = cache.GetMany(outerKey, innerKeys, span);

            return span.Slice(0, countFound).ToArray();
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