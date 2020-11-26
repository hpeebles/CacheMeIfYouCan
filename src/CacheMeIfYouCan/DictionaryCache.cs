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
            : this(null)
        { }
        
        public DictionaryCache(IEqualityComparer<TKey> keyComparer)
            : base(keyComparer, TimeSpan.FromSeconds(10))
        { }
        
        internal DictionaryCache(IEqualityComparer<TKey> keyComparer, TimeSpan keyExpiryProcessorInterval)
            : base(keyComparer, keyExpiryProcessorInterval)
        { }

        public bool TryGet(TKey key, out TValue value)
        {
            CheckDisposed();

            return TryGetImpl(key, TicksHelper.GetTicks64(), out value);
        }

        public void Set(TKey key, TValue value, TimeSpan timeToLive)
        {
            CheckDisposed();

            SetImpl(key, value, timeToLive, TicksHelper.GetTicks64());
        }

        public int GetMany(ReadOnlySpan<TKey> keys, Span<KeyValuePair<TKey, TValue>> destination)
        {
            CheckDisposed();

            if (destination.Length < keys.Length)
                throw Errors.LocalCache_DestinationArrayTooSmall(nameof(destination));

            var countFound = 0;
            var nowTicks = TicksHelper.GetTicks64();
            foreach (var key in keys)
            {
                if (TryGetImpl(key, nowTicks, out var value))
                    destination[countFound++] = new KeyValuePair<TKey, TValue>(key, value);
            }

            return countFound;
        }

        public void SetMany(ReadOnlySpan<KeyValuePair<TKey, TValue>> values, TimeSpan timeToLive)
        {
            CheckDisposed();

            var nowTicks = TicksHelper.GetTicks64();
            foreach (var value in values)
                SetImpl(value.Key, value.Value, timeToLive, nowTicks);
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
            IEqualityComparer<TOuterKey> outerKeyComparer = null,
            IEqualityComparer<TInnerKey> innerKeyComparer = null)
            : base(
                new TupleKeyComparer<TOuterKey, TInnerKey>(
                    outerKeyComparer,
                    innerKeyComparer),
                TimeSpan.FromSeconds(10))
        {
            _outerKeyComparer = outerKeyComparer ?? EqualityComparer<TOuterKey>.Default;
            _innerKeyComparer = innerKeyComparer ?? EqualityComparer<TInnerKey>.Default;
        }
        
        public int GetMany(
            TOuterKey outerKey,
            ReadOnlySpan<TInnerKey> innerKeys,
            Span<KeyValuePair<TInnerKey, TValue>> destination)
        {
            CheckDisposed();

            if (destination.Length < innerKeys.Length)
                throw Errors.LocalCache_DestinationArrayTooSmall(nameof(destination));
            
            var outerKeyHashCode = _outerKeyComparer.GetHashCode(outerKey);
            
            var countFound = 0;
            var nowTicks = TicksHelper.GetTicks64();
            foreach (var innerKey in innerKeys)
            {
                var innerKeyHashCode = _innerKeyComparer.GetHashCode(innerKey);

                var hashCode = GetCombinedHashCode(outerKeyHashCode, innerKeyHashCode);

                if (TryGetImpl(new TupleKey<TOuterKey, TInnerKey>(outerKey, innerKey, hashCode), nowTicks, out var value))
                    destination[countFound++] = new KeyValuePair<TInnerKey, TValue>(innerKey, value);
            }

            return countFound;
        }

        public void SetMany(
            TOuterKey outerKey,
            ReadOnlySpan<KeyValuePair<TInnerKey, TValue>> values,
            TimeSpan timeToLive)
        {
            CheckDisposed();

            var outerKeyHashCode = _outerKeyComparer.GetHashCode(outerKey);

            var nowTicks = TicksHelper.GetTicks64();
            foreach (var kv in values)
            {
                var innerKeyHashCode = _innerKeyComparer.GetHashCode(kv.Key);

                var key = new TupleKey<TOuterKey, TInnerKey>(
                    outerKey,
                    kv.Key,
                    GetCombinedHashCode(outerKeyHashCode, innerKeyHashCode));

                SetImpl(key, kv.Value, timeToLive, nowTicks);
            }
        }

        public void SetManyWithVaryingTimesToLive(
            TOuterKey outerKey,
            ReadOnlySpan<KeyValuePair<TInnerKey, ValueAndTimeToLive<TValue>>> values)
        {
            CheckDisposed();

            var outerKeyHashCode = _outerKeyComparer.GetHashCode(outerKey);

            var nowTicks = TicksHelper.GetTicks64();
            foreach (var value in values)
            {
                var innerKeyHashCode = _innerKeyComparer.GetHashCode(value.Key);

                var key = new TupleKey<TOuterKey, TInnerKey>(
                    outerKey,
                    value.Key,
                    GetCombinedHashCode(outerKeyHashCode, innerKeyHashCode));

                SetImpl(key, value.Value.Value, value.Value.TimeToLive, nowTicks);
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