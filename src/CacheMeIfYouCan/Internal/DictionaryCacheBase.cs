using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Channels;

namespace CacheMeIfYouCan.Internal
{
    public abstract class DictionaryCacheBase<TKey, TValue>
    {
        private readonly ConcurrentDictionary<TKey, ValueAndExpiry> _values;
        private readonly MinHeap<KeyAndExpiry> _keysToExpireHeap;
        private readonly HighCapacityObjectPool<ValueAndExpiry> _valueAndExpiryPool;
        private readonly ChannelReader<KeyAndExpiry> _keysToBePutIntoExpiryHeapReader;
        private readonly ChannelWriter<KeyAndExpiry> _keysToBePutIntoExpiryHeapWriter;
        private readonly TimeSpan _keyExpiryProcessorInterval;
        private readonly Timer _keyExpiryProcessorTimer;
        private int _disposed;

        public int Count => _values.Count;

        public void Clear()
        {
            _keysToExpireHeap.Clear();
            _values.Clear();
        }

        private protected DictionaryCacheBase(IEqualityComparer<TKey> keyComparer, TimeSpan keyExpiryProcessorInterval)
        {
            _values = new ConcurrentDictionary<TKey, ValueAndExpiry>(keyComparer);
            _keysToExpireHeap = new MinHeap<KeyAndExpiry>(KeyAndExpiryComparer.Instance);
            _valueAndExpiryPool = new HighCapacityObjectPool<ValueAndExpiry>(() => new ValueAndExpiry(), 1000);
            
            var keysToBePutIntoExpiryHeapChannel = Channel.CreateUnbounded<KeyAndExpiry>(new UnboundedChannelOptions
            {
                SingleReader = true
            });

            _keysToBePutIntoExpiryHeapReader = keysToBePutIntoExpiryHeapChannel.Reader;
            _keysToBePutIntoExpiryHeapWriter = keysToBePutIntoExpiryHeapChannel.Writer;
            _keyExpiryProcessorInterval = keyExpiryProcessorInterval;
            _keyExpiryProcessorTimer = new Timer(
                _ => ProcessKeyExpiryDates(),
                null,
                (int)keyExpiryProcessorInterval.TotalMilliseconds,
                -1);
        }

        internal DebugInfo GetDebugInfo()
        {
            return new DebugInfo
            {
                Values = _values,
                ValueAndExpiryPool = _valueAndExpiryPool
            };
        }

        protected bool TryGetImpl(TKey key, long nowTicks, out TValue value)
        {
            while (true)
            {
                if (!_values.TryGetValue(key, out var valueAndExpiry))
                {
                    value = default;
                    return false;
                }

                // Due to valueAndExpiry references being recycled immediately after they are removed from the
                // dictionary we need to always check that the version number of each valueAndExpiry is the same before
                // and after reading the value.
                // The version number is incremented before updating the value, so if the version is the same before and
                // after reading the value, then the value is still valid, if the version number has changed, then that
                // value has been removed from the dictionary and reused, so restart the loop and try again.
                var version = valueAndExpiry.Version;

                if (valueAndExpiry.ExpiryTicks > nowTicks)
                {
                    value = valueAndExpiry.Value;
                    if (valueAndExpiry.Version == version)
                        return true;
                }
                else
                {
                    value = default;
                    if (valueAndExpiry.Version == version)
                        return false;
                }
            }
        }

        protected void SetImpl(TKey key, TValue value, TimeSpan timeToLive, long nowTicks)
        {
            var valueAndExpiryToAdd = _valueAndExpiryPool.Get();

            Interlocked.Increment(ref valueAndExpiryToAdd.Version);
            
            var expiryTicks = nowTicks + (long)timeToLive.TotalMilliseconds;
            
            valueAndExpiryToAdd.Value = value;
            valueAndExpiryToAdd.ExpiryTicks = expiryTicks;

            while (true)
            {
                var valueFromCache = _values.GetOrAdd(key, valueAndExpiryToAdd);

                // If the value was added, we are done
                if (valueFromCache == valueAndExpiryToAdd)
                    break;
                
                // Otherwise we need to update the value, but because we want to recycle the previous value, we need to
                // ensure the update only happens if the value stored is still the one we already have a reference to
                if (!_values.TryUpdate(key, valueAndExpiryToAdd, valueFromCache))
                    continue;
                
                // The key is already in the expiry queue, so we only need to re-add it if the new value expires before
                // the previous value. Otherwise, when the old expiry date is reached the key will be checked and a new
                // item will be put in the queue with the new expiry date
                var addKeyAndExpiry = expiryTicks < valueFromCache.ExpiryTicks;
                
                _valueAndExpiryPool.Return(valueFromCache);

                if (addKeyAndExpiry)
                    break;

                return;
            }

            var keyAndExpiry = new KeyAndExpiry(key, expiryTicks);

            _keysToBePutIntoExpiryHeapWriter.TryWrite(keyAndExpiry);
        }

        protected bool RemoveImpl(TKey key, out TValue value)
        {
            if (!_values.TryRemove(key, out var valueAndExpiry) ||
                valueAndExpiry.ExpiryTicks < TicksHelper.GetTicks64())
            {
                value = default;
                return false;
            }

            value = valueAndExpiry.Value;
            return true;
        }

        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref _disposed, 1, 0) == 0)
                _keyExpiryProcessorTimer.Dispose();
        }
        
        private void ProcessKeyExpiryDates()
        {
            var nowTicks = TicksHelper.GetTicks64();
            var nowTicksDividedBy1024 = (int)(nowTicks >> 10);
            
            while (_keysToBePutIntoExpiryHeapReader.TryRead(out var keyAndExpiry))
            {
                if (keyAndExpiry.ExpiryTicksDividedBy1024 < nowTicksDividedBy1024)
                    RemoveExpiredKey(keyAndExpiry, nowTicks);
                else
                    _keysToExpireHeap.Add(keyAndExpiry);
            }

            while (
                _keysToExpireHeap.TryPeek(out var nextPeek) &&
                nextPeek.ExpiryTicksDividedBy1024 < nowTicksDividedBy1024 &&
                _keysToExpireHeap.TryTake(out var next))
            {
                RemoveExpiredKey(next, nowTicks);
            }

            _keyExpiryProcessorTimer.Change((int)_keyExpiryProcessorInterval.TotalMilliseconds, -1);
        }

        private void RemoveExpiredKey(KeyAndExpiry keyAndExpiry, long nowTicks)
        {
            if (!_values.TryGetValue(keyAndExpiry.Key, out var valueAndExpiry))
                return;
            
            if (valueAndExpiry.ExpiryTicks < nowTicks)
            {
                var kvp = new KeyValuePair<TKey, ValueAndExpiry>(keyAndExpiry.Key, valueAndExpiry);

                if (((ICollection<KeyValuePair<TKey, ValueAndExpiry>>) _values).Remove(kvp))
                    _valueAndExpiryPool.Return(valueAndExpiry);
            }
            else
            {
                _keysToExpireHeap.Add(new KeyAndExpiry(keyAndExpiry.Key, valueAndExpiry.ExpiryTicks));
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void CheckDisposed()
        {
            if (_disposed == 1)
                throw new ObjectDisposedException(nameof(DictionaryCache<TKey, TValue>));
        }

        internal class ValueAndExpiry
        {
            public TValue Value;
            public long ExpiryTicks;
            public volatile int Version;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        internal readonly struct KeyAndExpiry
        {
            public KeyAndExpiry(TKey key, long expiryTicks)
                : this(key, (int)(expiryTicks >> 10) + 1)
            { }
            
            public KeyAndExpiry(TKey key, int expiryTicksDividedBy1024)
            {
                Key = key;
                ExpiryTicksDividedBy1024 = expiryTicksDividedBy1024;
            }
            
            public readonly TKey Key;
            public readonly int ExpiryTicksDividedBy1024;
        }

        private sealed class KeyAndExpiryComparer : IComparer<KeyAndExpiry>
        {
            private KeyAndExpiryComparer() { }
            
            public static KeyAndExpiryComparer Instance { get; } = new KeyAndExpiryComparer();

            public int Compare(KeyAndExpiry x, KeyAndExpiry y)
            {
                return x.ExpiryTicksDividedBy1024.CompareTo(y.ExpiryTicksDividedBy1024);
            }
        }

        internal class DebugInfo
        {
            public ConcurrentDictionary<TKey, ValueAndExpiry> Values;
            public HighCapacityObjectPool<ValueAndExpiry> ValueAndExpiryPool;
        }
    }
}