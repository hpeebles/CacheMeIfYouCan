using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace CacheMeIfYouCan.Caches
{
    /// <summary>
    /// A cache which pushes the expiry of each key back when that key is accessed up until the overall time to live
    /// is reached.
    /// For example, if a key is set in the cache with a time to live of 1 hour and rollingTimeToLive is set to 1
    /// minute, then if there is ever a 1 minute gap where the key is not accessed, the key will be expired, otherwise
    /// it will remain cached until the hour is up, at which point it will be expired.
    /// </summary>
    public class RollingTimeToLiveDictionaryCache<TK, TV> : ILocalCache<TK, TV>, ICachedItemCounter, IDisposable
    {
        // Store keys along with their exact expiry times
        private readonly ConcurrentDictionary<TK, CacheItem> _values = new ConcurrentDictionary<TK, CacheItem>();
        
        // Store keys by expiry second so that we can remove them as they expire (grouping by second rather than tick for efficiency)
        private readonly SortedDictionary<int, List<TK>> _ttls = new SortedDictionary<int, List<TK>>();
        
        // Lock to ensure only 1 thread accesses the non-threadsafe dictionary of TTLs at a time
        private readonly object _lock = new object();

        // Each time a key is accessed, set the time to live to be the current timestamp plus the _rollingTimeToLive
        // value up to the MaxExpiry value.
        private readonly long _rollingTimeToLive;
        
        // Every 10 seconds we check for expired keys and remove them, disposing of this field is the only way to stop that process
        private readonly IDisposable _keyRemoverProcess;
        
        public RollingTimeToLiveDictionaryCache(string cacheName, TimeSpan rollingTimeToLive)
        {
            CacheName = cacheName;

            _rollingTimeToLive = rollingTimeToLive.Ticks;

            _keyRemoverProcess = Observable
                .Interval(TimeSpan.FromSeconds(10))
                .Subscribe(_ => RemoveExpiredKeys());
        }
        
        public string CacheName { get; }
        public string CacheType { get; } = "rolling-ttl-dictionary";

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
            
            _values[key] = new CacheItem(value, _rollingTimeToLive, expiryTicks);

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

                if (value.Expiry > currentTimestamp)
                {
                    results.Add(new GetFromCacheResult<TK, TV>(
                        key,
                        value.Value,
                        TimeSpan.FromTicks(value.Expiry - currentTimestamp),
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
                _values[kv.Key] = new CacheItem(kv.Value, _rollingTimeToLive, expiryTicks);

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

            var keysToCheck = new List<TK>();

            lock (_lock)
            {
                // Since _ttls is a dictionary sorted by expiry timestamp, the first item will be the next to expire
                var next = _ttls.FirstOrDefault();
                while (0 < next.Key && next.Key < timestampSeconds)
                {
                    keysToCheck.AddRange(next.Value);
                    _ttls.Remove(next.Key);

                    next = _ttls.FirstOrDefault();
                }
            }

            var keysToReAdd = new Dictionary<int, List<TK>>();

            var now = Timestamp.Now;
            
            foreach (var key in keysToCheck)
            {
                if (!_values.TryGetValue(key, out var value))
                    continue;

                if (value.Expiry < now)
                {
                    _values.TryRemove(key, out _);
                }
                else
                {
                    var expirySeconds = (int) (value.Expiry / TimeSpan.TicksPerSecond);

                    if (!keysToReAdd.TryGetValue(expirySeconds, out var keys))
                    {
                        keys = new List<TK>();
                        keysToReAdd[expirySeconds] = keys;
                    }

                    keys.Add(key);
                }
            }

            lock (_lock)
            {
                foreach (var kv in keysToReAdd)
                {
                    if (!_ttls.TryGetValue(kv.Key, out var keys))
                    {
                        keys = new List<TK>();
                        _ttls[kv.Key] = keys;
                    }
                    
                    keys.AddRange(kv.Value);
                }
            }
        }

        private class CacheItem
        {
            private readonly TV _value;
            private readonly long _rollingTimeToLive;
            private readonly long _maxExpiry;

            public CacheItem(TV value, long rollingTimeToLive, long maxExpiry)
            {
                _value = value;
                _rollingTimeToLive = rollingTimeToLive;
                _maxExpiry = maxExpiry;
                Expiry = Timestamp.Now + rollingTimeToLive;
            }

            public TV Value
            {
                get
                {
                    Expiry = Math.Min(Timestamp.Now + _rollingTimeToLive, _maxExpiry);
                    return _value;
                }
            }
            
            public long Expiry { get; private set; }
        }
    }
}