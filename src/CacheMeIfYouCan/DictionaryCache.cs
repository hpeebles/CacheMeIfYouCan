using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using CacheMeIfYouCan.Internal;

namespace CacheMeIfYouCan
{
    public sealed class DictionaryCache<TKey, TValue> : DictionaryCacheBase<TKey, TValue>,
        ILocalCache<TKey, TValue>, IDisposable
    {
        public DictionaryCache()
            : this(EqualityComparer<TKey>.Default)
        { }
        
        public DictionaryCache(IEqualityComparer<TKey> keyComparer)
            : base(keyComparer, TimeSpan.FromSeconds(10))
        { }
        
        public DictionaryCache(IEqualityComparer<TKey> keyComparer, TimeSpan keyExpiryProcessorInterval)
            : base(keyComparer, keyExpiryProcessorInterval)
        { }

        public bool TryGet(TKey key, out TValue value)
        {
            CheckDisposed();

            return TryGetImpl(key, out value);
        }

        public void Set(TKey key, TValue value, TimeSpan timeToLive)
        {
            CheckDisposed();

            SetImpl(key, value, timeToLive);
        }

        public int GetMany(IReadOnlyCollection<TKey> keys, Span<KeyValuePair<TKey, TValue>> destination)
        {
            CheckDisposed();

            if (destination.Length < keys.Count)
                throw Errors.LocalCache_DestinationArrayTooSmall(nameof(destination));

            var countFound = 0;
            foreach (var key in keys)
            {
                if (TryGetImpl(key, out var value))
                    destination[countFound++] = new KeyValuePair<TKey, TValue>(key, value);
            }

            return countFound;
        }

        public void SetMany(IReadOnlyCollection<KeyValuePair<TKey, TValue>> values, TimeSpan timeToLive)
        {
            CheckDisposed();

            foreach (var value in values)
                SetImpl(value.Key, value.Value, timeToLive);
        }

        public bool TryRemove(TKey key, out TValue value) => RemoveImpl(key, out value);
    }
    
    public sealed class DictionaryCache<TOuterKey, TInnerKey, TValue> : DictionaryCacheBase<TupleKey<TOuterKey, TInnerKey>, TValue>,
        ILocalCache<TOuterKey, TInnerKey, TValue>, IDisposable
    {
        private readonly IEqualityComparer<TOuterKey> _outerKeyComparer;
        private readonly IEqualityComparer<TInnerKey> _innerKeyComparer;

        public DictionaryCache()
            : this(EqualityComparer<TOuterKey>.Default, EqualityComparer<TInnerKey>.Default)
        { }
        
        public DictionaryCache(
            IEqualityComparer<TOuterKey> outerKeyComparer,
            IEqualityComparer<TInnerKey> innerKeyComparer)
            : base(
                new TupleKeyComparer<TOuterKey, TInnerKey>(outerKeyComparer, innerKeyComparer),
                TimeSpan.FromSeconds(10))
        {
            _outerKeyComparer = outerKeyComparer;
            _innerKeyComparer = innerKeyComparer;
        }
        
        public int GetMany(
            TOuterKey outerKey,
            IReadOnlyCollection<TInnerKey> innerKeys,
            Span<KeyValuePair<TInnerKey, TValue>> destination)
        {
            CheckDisposed();

            if (destination.Length < innerKeys.Count)
                throw Errors.LocalCache_DestinationArrayTooSmall(nameof(destination));
            
            var outerKeyHashCode = _outerKeyComparer.GetHashCode(outerKey);
            
            var countFound = 0;
            foreach (var innerKey in innerKeys)
            {
                var innerKeyHashCode = _innerKeyComparer.GetHashCode(innerKey);

                var hashCode = GetCombinedHashCode(outerKeyHashCode, innerKeyHashCode);

                if (TryGetImpl(new TupleKey<TOuterKey, TInnerKey>(outerKey, innerKey, hashCode), out var value))
                    destination[countFound++] = new KeyValuePair<TInnerKey, TValue>(innerKey, value);
            }

            return countFound;
        }

        public void SetMany(
            TOuterKey outerKey,
            IReadOnlyCollection<KeyValuePair<TInnerKey, TValue>> values,
            TimeSpan timeToLive)
        {
            CheckDisposed();

            var outerKeyHashCode = _outerKeyComparer.GetHashCode(outerKey);

            foreach (var kv in values)
            {
                var innerKeyHashCode = _innerKeyComparer.GetHashCode(kv.Key);

                var key = new TupleKey<TOuterKey, TInnerKey>(
                    outerKey,
                    kv.Key,
                    GetCombinedHashCode(outerKeyHashCode, innerKeyHashCode));

                SetImpl(key, kv.Value, timeToLive);
            }
        }

        public void SetManyWithVaryingTimesToLive(
            TOuterKey outerKey,
            ReadOnlySpan<KeyValuePair<TInnerKey, ValueAndTimeToLive<TValue>>> values)
        {
            CheckDisposed();

            var outerKeyHashCode = _outerKeyComparer.GetHashCode(outerKey);

            foreach (var value in values)
            {
                var innerKeyHashCode = _innerKeyComparer.GetHashCode(value.Key);

                var key = new TupleKey<TOuterKey, TInnerKey>(
                    outerKey,
                    value.Key,
                    GetCombinedHashCode(outerKeyHashCode, innerKeyHashCode));

                SetImpl(key, value.Value.Value, value.Value.TimeToLive);
            }
        }

        public bool TryRemove(TOuterKey outerKey, TInnerKey innerKey, out TValue value)
        {
            var outerKeyHashCode = _outerKeyComparer.GetHashCode(outerKey);
            var innerKeyHashCode = _innerKeyComparer.GetHashCode(innerKey);

            var key = new TupleKey<TOuterKey, TInnerKey>(
                outerKey,
                innerKey,
                GetCombinedHashCode(outerKeyHashCode, innerKeyHashCode));

            return RemoveImpl(key, out value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]        
        private static int GetCombinedHashCode(int outerKeyHashCode, int innerKeyHashCode)
        {
            return (outerKeyHashCode * 103) + innerKeyHashCode;
        }
    }
}