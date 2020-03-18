using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Channels;
using CacheMeIfYouCan.Internal;

namespace CacheMeIfYouCan.LocalCaches
{
    public abstract class DictionaryCacheBase<TKey, TValue>
    {
        private readonly ConcurrentDictionary<TKey, ValueAndExpiry> _values;
        private readonly MinHeap<KeyAndExpiry> _keysToExpireHeap;
        private readonly ObjectPool<ValueAndExpiry> _valueAndExpiryPool;
        private readonly ObjectPool<KeyAndExpiry> _keyAndExpiryPool;
        private readonly ChannelReader<KeyAndExpiry> _keysToBePutIntoExpiryHeapReader;
        private readonly ChannelWriter<KeyAndExpiry> _keysToBePutIntoExpiryHeapWriter;
        private readonly TimeSpan _keyExpiryProcessorInterval;
        private readonly Timer _keyExpiryProcessorTimer;
        private int _disposed;
        
        protected DictionaryCacheBase(IEqualityComparer<TKey> keyComparer, TimeSpan keyExpiryProcessorInterval)
        {
            _values = new ConcurrentDictionary<TKey, ValueAndExpiry>(keyComparer);
            _keysToExpireHeap = new MinHeap<KeyAndExpiry>(KeyAndExpiryComparer.Instance);
            _valueAndExpiryPool = new ObjectPool<ValueAndExpiry>(() => new ValueAndExpiry(), 1000);
            _keyAndExpiryPool = new ObjectPool<KeyAndExpiry>(() => new KeyAndExpiry(), 1000);
            
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
                ValueAndExpiryPool = _valueAndExpiryPool,
                KeyAndExpiryPool = _keyAndExpiryPool
            };
        }

        protected bool TryGetImpl(TKey key, out TValue value)
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

                if (valueAndExpiry.ExpiryTicks > DateTime.UtcNow.Ticks)
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

        protected void SetImpl(TKey key, TValue value, TimeSpan timeToLive)
        {
            var valueAndExpiry = _valueAndExpiryPool.Rent();

            Interlocked.Increment(ref valueAndExpiry.Version);
            
            var expiryTicks = DateTime.UtcNow.Add(timeToLive).Ticks;
            
            valueAndExpiry.Value = value;
            valueAndExpiry.ExpiryTicks = expiryTicks;

            while (true)
            {
                // If a value already exists at this key then we want to update this value and recycle the old value.
                // This means we need to do a TryGetValue, if successful we then do a TryUpdate where the update only
                // succeeds if the value is still the same. We can then queue up the old value to be recycled. This is
                // so that the values we recycle are guaranteed to no longer be in the dictionary
                if (_values.TryGetValue(key, out var existingValueAndExpiry))
                {
                    if (_values.TryUpdate(key, valueAndExpiry, existingValueAndExpiry))
                    {
                        _valueAndExpiryPool.Return(existingValueAndExpiry);
                        break;
                    }
                }
                else if (_values.TryAdd(key, valueAndExpiry))
                {
                    break;
                }
            }

            var keyAndExpiry = _keyAndExpiryPool.Rent();

            keyAndExpiry.Key = key;
            keyAndExpiry.ExpiryTicks = expiryTicks;

            _keysToBePutIntoExpiryHeapWriter.TryWrite(keyAndExpiry);
        }

        protected bool RemoveImpl(TKey key, out TValue value)
        {
            if (!_values.TryRemove(key, out var valueAndExpiry) ||
                valueAndExpiry.ExpiryTicks < DateTime.UtcNow.Ticks)
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
            var nowTicks = DateTime.UtcNow.Ticks;
            
            while (_keysToBePutIntoExpiryHeapReader.TryRead(out var keyAndExpiry))
            {
                if (keyAndExpiry.ExpiryTicks < nowTicks)
                    RemoveExpiredKey(keyAndExpiry);
                else
                    _keysToExpireHeap.Add(keyAndExpiry);
            }

            while (
                _keysToExpireHeap.TryPeek(out var nextPeek) &&
                nextPeek.ExpiryTicks < nowTicks &&
                _keysToExpireHeap.TryTake(out var next))
            {
                RemoveExpiredKey(next);
            }

            _keyExpiryProcessorTimer.Change((int)_keyExpiryProcessorInterval.TotalMilliseconds, -1);
        }

        private void RemoveExpiredKey(KeyAndExpiry keyAndExpiry)
        {
            if (_values.TryGetValue(keyAndExpiry.Key, out var valueAndExpiry) &&
                valueAndExpiry.ExpiryTicks < DateTime.UtcNow.Ticks)
            {
                var kvp = new KeyValuePair<TKey, ValueAndExpiry>(keyAndExpiry.Key, valueAndExpiry);

                if (((ICollection<KeyValuePair<TKey, ValueAndExpiry>>) _values).Remove(kvp))
                    _valueAndExpiryPool.Return(valueAndExpiry);
            }

            _keyAndExpiryPool.Return(keyAndExpiry);
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

        internal class KeyAndExpiry
        {
            public TKey Key;
            public long ExpiryTicks;
        }

        private sealed class KeyAndExpiryComparer : IComparer<KeyAndExpiry>
        {
            private KeyAndExpiryComparer() { }
            
            public static KeyAndExpiryComparer Instance { get; } = new KeyAndExpiryComparer();

            public int Compare(KeyAndExpiry x, KeyAndExpiry y)
            {
                if (x is null || y is null)
                    return 0;

                return x.ExpiryTicks.CompareTo(y.ExpiryTicks);
            }
        }

        internal class DebugInfo
        {
            public ConcurrentDictionary<TKey, ValueAndExpiry> Values;
            public ObjectPool<ValueAndExpiry> ValueAndExpiryPool;
            public ObjectPool<KeyAndExpiry> KeyAndExpiryPool;
        }
    }
}