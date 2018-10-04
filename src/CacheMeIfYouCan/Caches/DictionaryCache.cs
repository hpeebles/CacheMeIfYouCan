using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using CacheMeIfYouCan.Internal;

namespace CacheMeIfYouCan.Caches
{
    public class DictionaryCache<TK, TV> : ILocalCache<TK, TV>, IDisposable
    {
        private const string CacheType = "dictionary";
        
        // Store keys along with their exact expiry times
        private readonly ConcurrentDictionary<TK, Tuple<TV, long>> _values = new ConcurrentDictionary<TK, Tuple<TV, long>>();
        
        // Store keys by expiry second so that we can remove them as they expire (grouping by second rather than tick for efficiency)
        private readonly SortedDictionary<int, List<TK>> _ttls = new SortedDictionary<int, List<TK>>();
        
        // Lock to ensure only 1 thread accesses the non-threadsafe dictionary of TTLs
        private readonly object _lock = new object();
        
        // Every 10 seconds we check for expired keys and remove them, disposing of this field is the only way to stop that process
        private readonly IDisposable _keyRemoverProcess;

        public DictionaryCache()
        {
            _keyRemoverProcess = Observable
                .Interval(TimeSpan.FromSeconds(10))
                .Subscribe(_ => RemoveExpiredKeys());
        }
        
        public GetFromCacheResult<TK, TV> Get(Key<TK> key)
        {
            if (!_values.TryGetValue(key, out var value))
                return GetFromCacheResult<TK, TV>.NotFound(key);
            
            var currentTimestamp = Timestamp.Now;

            if (value.Item2 > currentTimestamp)
                return new GetFromCacheResult<TK, TV>(key, value.Item1, TimeSpan.FromTicks(value.Item2 - currentTimestamp), CacheType);
            
            _values.TryRemove(key, out _);
            return GetFromCacheResult<TK, TV>.NotFound(key);
        }

        public void Set(Key<TK> key, TV value, TimeSpan timeToLive)
        {
            var expiryTicks = Timestamp.Now + timeToLive.Ticks;
            var expirySeconds = (int) (expiryTicks / TimeSpan.TicksPerSecond);
            
            _values[key] = Tuple.Create(value, expiryTicks);

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

        public void Remove(Key<TK> key)
        {
            _values.TryRemove(key, out _);
        }

        public void Dispose()
        {
            _keyRemoverProcess?.Dispose();
            _values.Clear();
            
            lock (_lock)
                _ttls.Clear();
        }

        private void RemoveExpiredKeys()
        {
            var timestamp = Timestamp.Now;

            var keysToExpire = new List<TK>();

            lock (_lock)
            {
                // Since _ttls is a sorted dictionary by expiry timestamp, the first item will be the next to expire
                var next = _ttls.FirstOrDefault();
                while (0 < next.Key && next.Key < timestamp)
                {
                    keysToExpire.AddRange(next.Value);
                    _ttls.Remove(next.Key);

                    next = _ttls.FirstOrDefault();
                }
            }
        
            foreach (var key in keysToExpire)
                _values.TryRemove(key, out _);
        }
    }
}