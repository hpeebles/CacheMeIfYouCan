using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Channels;
using CacheMeIfYouCan.Internal;

namespace CacheMeIfYouCan.LocalCaches
{
    public abstract class DictionaryCacheBase<TKey, TValue>
    {
        private readonly ConcurrentDictionary<TKey, ValueAndExpiry> _values;
        private readonly SortedDictionary<int, SinglyLinkedList<KeyAndExpiry>> _keysByExpiryDateSeconds;
        private readonly ObjectPool<ValueAndExpiry> _valueAndExpiryPool;
        private readonly ObjectPool<KeyAndExpiry> _keyAndExpiryPool;
        private readonly ChannelReader<KeyAndExpiry> _keysToBePutIntoExpiryDictionaryReader;
        private readonly ChannelWriter<KeyAndExpiry> _keysToBePutIntoExpiryDictionaryWriter;
        private readonly TimeSpan _keyExpiryProcessorInterval;
        private readonly Timer _keyExpiryProcessorTimer;
        private int _disposed;
        private static readonly DateTime UnixStartDate = new DateTime(1970, 1, 1);
        
        protected DictionaryCacheBase(IEqualityComparer<TKey> keyComparer, TimeSpan keyExpiryProcessorInterval)
        {
            _values = new ConcurrentDictionary<TKey, ValueAndExpiry>(keyComparer);
            _keysByExpiryDateSeconds = new SortedDictionary<int, SinglyLinkedList<KeyAndExpiry>>();
            _valueAndExpiryPool = new ObjectPool<ValueAndExpiry>(() => new ValueAndExpiry(), 1000);
            _keyAndExpiryPool = new ObjectPool<KeyAndExpiry>(() => new KeyAndExpiry(), 1000);
            
            var keysToBePutIntoExpiryDictionary = Channel.CreateUnbounded<KeyAndExpiry>(new UnboundedChannelOptions
            {
                SingleReader = true
            });

            _keysToBePutIntoExpiryDictionaryReader = keysToBePutIntoExpiryDictionary.Reader;
            _keysToBePutIntoExpiryDictionaryWriter = keysToBePutIntoExpiryDictionary.Writer;
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

            _keysToBePutIntoExpiryDictionaryWriter.TryWrite(keyAndExpiry);
        }
        
        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref _disposed, 1, 0) == 0)
                _keyExpiryProcessorTimer.Dispose();
        }
        
        private void ProcessKeyExpiryDates()
        {
            var timestampSecondsNow = GetTimestampSeconds(DateTime.UtcNow.Ticks);
            
            while (_keysToBePutIntoExpiryDictionaryReader.TryRead(out var keyAndExpiry))
            {
                var expiryTimestampSeconds = GetTimestampSeconds(keyAndExpiry.ExpiryTicks);

                if (expiryTimestampSeconds < timestampSecondsNow)
                {
                    RemoveExpiredKey(keyAndExpiry);
                    continue;
                }

                if (!_keysByExpiryDateSeconds.TryGetValue(expiryTimestampSeconds, out var keysWithSimilarExpiry))
                {
                    keysWithSimilarExpiry = new SinglyLinkedList<KeyAndExpiry>();
                    _keysByExpiryDateSeconds.Add(expiryTimestampSeconds, keysWithSimilarExpiry);
                }
                
                keysWithSimilarExpiry.Append(keyAndExpiry);
            }

            while (_keysByExpiryDateSeconds.Count > 0)
            {
                var next = _keysByExpiryDateSeconds.First();

                if (next.Key >= timestampSecondsNow)
                    break;

                foreach (var keyAndExpiry in next.Value)
                    RemoveExpiredKey(keyAndExpiry);

                _keysByExpiryDateSeconds.Remove(next.Key);
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetTimestampSeconds(long dateTimeTicks)
        {
            return (int)((dateTimeTicks - UnixStartDate.Ticks) / TimeSpan.TicksPerSecond);
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

        internal class DebugInfo
        {
            public ConcurrentDictionary<TKey, ValueAndExpiry> Values;
            public ObjectPool<ValueAndExpiry> ValueAndExpiryPool;
            public ObjectPool<KeyAndExpiry> KeyAndExpiryPool;
        }
    }
}