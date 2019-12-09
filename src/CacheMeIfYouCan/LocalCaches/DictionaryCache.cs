using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Channels;

namespace CacheMeIfYouCan.LocalCaches
{
    public sealed class DictionaryCache<TKey, TValue> : ILocalCache<TKey, TValue>, IDisposable
    {
        private readonly ConcurrentDictionary<TKey, ValueAndExpiry> _values;
        private readonly SortedDictionary<int, LinkedList<KeyAndExpiry>> _keysByExpiryDateSeconds;
        private readonly ConcurrentQueue<ValueAndExpiry> _valuesToRecycle;
        private readonly ConcurrentQueue<KeyAndExpiry> _keysToRecycle;
        private readonly ChannelReader<KeyAndExpiry> _keysToBePutIntoExpiryDictionaryReader;
        private readonly ChannelWriter<KeyAndExpiry> _keysToBePutIntoExpiryDictionaryWriter;
        private readonly TimeSpan _keyExpiryProcessorInterval;
        private readonly Timer _keyExpiryProcessorTimer;
        private int _disposed;
        private static readonly DateTime UnixStartDate = new DateTime(1970, 1, 1);
        private const int MaxItemsToRecycleQueueLength = 1000;

        public DictionaryCache()
            : this(EqualityComparer<TKey>.Default)
        { }
        
        public DictionaryCache(IEqualityComparer<TKey> keyComparer)
            : this(keyComparer, TimeSpan.FromSeconds(10))
        { }
        
        internal DictionaryCache(IEqualityComparer<TKey> keyComparer, TimeSpan keyExpiryProcessorInterval)
        {
            _values = new ConcurrentDictionary<TKey, ValueAndExpiry>(keyComparer);
            _keysByExpiryDateSeconds = new SortedDictionary<int, LinkedList<KeyAndExpiry>>();
            _valuesToRecycle = new ConcurrentQueue<ValueAndExpiry>();
            _keysToRecycle = new ConcurrentQueue<KeyAndExpiry>();
            
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
                KeysByExpiryDateSeconds = _keysByExpiryDateSeconds,
                ValuesToRecycle = _valuesToRecycle,
                KeysToRecycle = _keysToRecycle
            };
        }
        
        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref _disposed, 1, 0) == 0)
                _keyExpiryProcessorTimer.Dispose();
        }
        
        public bool TryGet(TKey key, out TValue value)
        {
            CheckDisposed();
            
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

        public void Set(TKey key, TValue value, TimeSpan timeToLive)
        {
            CheckDisposed();

            if (!_valuesToRecycle.TryDequeue(out var valueAndExpiry))
                valueAndExpiry = new ValueAndExpiry();

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
                        QueueItemForReuse(_valuesToRecycle, existingValueAndExpiry);
                        break;
                    }
                }
                else if (_values.TryAdd(key, valueAndExpiry))
                {
                    break;
                }
            }

            if (!_keysToRecycle.TryDequeue(out var keyAndExpiry))
                keyAndExpiry = new KeyAndExpiry();

            keyAndExpiry.Key = key;
            keyAndExpiry.ExpiryTicks = expiryTicks;

            _keysToBePutIntoExpiryDictionaryWriter.TryWrite(keyAndExpiry);
        }

        public IReadOnlyCollection<KeyValuePair<TKey, TValue>> GetMany(IReadOnlyCollection<TKey> keys)
        {
            CheckDisposed();

            var results = new List<KeyValuePair<TKey, TValue>>();

            foreach (var key in keys)
            {
                if (TryGet(key, out var value))
                    results.Add(new KeyValuePair<TKey, TValue>(key, value));
            }

            return results;
        }

        public void SetMany(IReadOnlyCollection<KeyValuePair<TKey, TValue>> values, TimeSpan timeToLive)
        {
            CheckDisposed();

            foreach (var value in values)
                Set(value.Key, value.Value, timeToLive);
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
                    keysWithSimilarExpiry = new LinkedList<KeyAndExpiry>();
                    _keysByExpiryDateSeconds.Add(expiryTimestampSeconds, keysWithSimilarExpiry);
                }
                
                keysWithSimilarExpiry.AddLast(keyAndExpiry);
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
                    QueueItemForReuse(_valuesToRecycle, valueAndExpiry);
            }

            QueueItemForReuse(_keysToRecycle, keyAndExpiry);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CheckDisposed()
        {
            if (_disposed == 1)
                throw new ObjectDisposedException(nameof(DictionaryCache<TKey, TValue>));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void QueueItemForReuse<T>(ConcurrentQueue<T> queue, T item)
        {
            if (queue.Count >= MaxItemsToRecycleQueueLength)
                return;
            
            queue.Enqueue(item);
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
            public SortedDictionary<int, LinkedList<KeyAndExpiry>> KeysByExpiryDateSeconds;
            public ConcurrentQueue<ValueAndExpiry> ValuesToRecycle;
            public ConcurrentQueue<KeyAndExpiry> KeysToRecycle;
        }
    }
}