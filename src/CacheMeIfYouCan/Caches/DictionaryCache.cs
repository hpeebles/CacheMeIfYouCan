using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace CacheMeIfYouCan.Caches
{
    public class DictionaryCache<TK, TV> : ILocalCache<TK, TV>, ICachedItemCounter, IDisposable
    {
        // Store keys along with their exact expiry times
        private readonly ConcurrentDictionary<TK, (TV Value, long Expiry)> _values = new ConcurrentDictionary<TK, (TV, long)>();
        
        // Store keys by expiry second so that we can remove them as they expire (grouping by second rather than tick for efficiency)
        private readonly SortedDictionary<int, List<TK>> _ttls = new SortedDictionary<int, List<TK>>();
        
        // Lock to ensure only 1 thread accesses the non-threadsafe dictionary of TTLs at a time
        private readonly object _lock = new object();

        // Max number of items to hold. Each time the key remover runs it will ensure the count stays below this value.
        private readonly int? _maxItems;
        
        // Every 10 seconds we check for expired keys and remove them, disposing of this field is the only way to stop that process
        private readonly IDisposable _keyRemoverProcess;
        
        public DictionaryCache(string cacheName, int? maxItems = null)
        {
            CacheName = cacheName;

            _maxItems = maxItems;

            _keyRemoverProcess = Observable
                .Interval(TimeSpan.FromSeconds(10))
                .Subscribe(_ => RemoveExpiredKeys());
        }
        
        public string CacheName { get; }
        public string CacheType { get; } = "dictionary";

        public GetFromCacheResult<TK, TV> Get(Key<TK> key)
        {
            if (!_values.TryGetValue(key, out var value))
                return new GetFromCacheResult<TK, TV>();
            
            var currentTimestamp = Timestamp.Now;

            if (value.Expiry < currentTimestamp)
            {
                _values.TryRemove(key, out _);
                return new GetFromCacheResult<TK, TV>();
            }
            
            return new GetFromCacheResult<TK, TV>(
                key,
                value.Value,
                TimeSpan.FromTicks(value.Expiry - currentTimestamp),
                CacheType);
        }

        public void Set(Key<TK> key, TV value, TimeSpan timeToLive)
        {
            var expiryTicks = Timestamp.Now + timeToLive.Ticks;
            var expirySeconds = (int) (expiryTicks / TimeSpan.TicksPerSecond);
            
            _values[key] = (value, expiryTicks);

            lock (_lock)
            {
                if (!_ttls.TryGetValue(expirySeconds, out var existing))
                {
                    existing = new List<TK>();
                    _ttls.Add(expirySeconds, existing);
                }
                
                existing.Add(key);
            }
        }
        
        public IList<GetFromCacheResult<TK, TV>> Get(ICollection<Key<TK>> keys)
        {
            var currentTimestamp = Timestamp.Now;

            var results = new List<GetFromCacheResult<TK, TV>>();

            foreach (var key in keys)
            {
                if (!_values.TryGetValue(key, out var value))
                    continue;

                if (value.Item2 > currentTimestamp)
                {
                    results.Add(new GetFromCacheResult<TK, TV>(
                        key,
                        value.Item1,
                        TimeSpan.FromTicks(value.Item2 - currentTimestamp),
                        CacheType));
                }
                else
                {
                    _values.TryRemove(key, out _);
                }
            }

            return results;
        }

        public void Set(ICollection<KeyValuePair<Key<TK>, TV>> values, TimeSpan timeToLive)
        {
            var expiryTicks = Timestamp.Now + timeToLive.Ticks;
            var expirySeconds = (int) (expiryTicks / TimeSpan.TicksPerSecond);
            
            foreach (var kv in values)
                _values[kv.Key] = (kv.Value, expiryTicks);

            lock (_lock)
            {
                if (!_ttls.TryGetValue(expirySeconds, out var existing))
                {
                    existing = new List<TK>();
                    _ttls.Add(expirySeconds, existing);
                }
                
                existing.AddRange(values.Select(kv => kv.Key.AsObject));
            }
        }

        public bool Remove(Key<TK> key)
        {
            return _values.TryRemove(key, out _);
        }

        public long Count => _values.Count;

        public void Dispose()
        {
            _keyRemoverProcess?.Dispose();
            _values.Clear();

            lock (_lock)
                _ttls.Clear();
        }

        private void RemoveExpiredKeys()
        {
            var timestampSeconds = (int) (Timestamp.Now / TimeSpan.TicksPerSecond);

            var keysToExpire = new List<TK>();

            lock (_lock)
            {
                // Since _ttls is a sorted dictionary by expiry timestamp, the first item will be the next to expire
                var next = _ttls.FirstOrDefault();
                while (0 < next.Key && next.Key < timestampSeconds)
                {
                    keysToExpire.AddRange(next.Value);
                    _ttls.Remove(next.Key);

                    next = _ttls.FirstOrDefault();
                }
            }
        
            foreach (var key in keysToExpire)
                _values.TryRemove(key, out _);

            if (!_maxItems.HasValue || _values.Count <= _maxItems.Value)
                return;

            var countToRemove = _values.Count - _maxItems.Value;

            var toRemove = new List<TK>();
            
            lock (_lock)
            {
                while (toRemove.Count < countToRemove)
                {
                    var next = _ttls.FirstOrDefault();
                    if (next.Key == 0)
                        break;
                    
                    toRemove.AddRange(next.Value);
                }
            }
            
            foreach (var key in toRemove.Take(countToRemove))
                _values.TryRemove(key, out _);
        }
    }
}