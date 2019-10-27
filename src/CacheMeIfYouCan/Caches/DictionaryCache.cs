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
        private readonly ConcurrentDictionary<TK, (TV Value, long Expiry)> _values;
        
        // Store keys by expiry second so that we can remove them as they expire (grouping by second rather than tick for efficiency)
        private readonly SortedDictionary<int, List<TK>> _ttls = new SortedDictionary<int, List<TK>>();
        
        // New keys are added here first, then every 10 seconds they are processed and put into the _ttls dictionary
        private readonly ConcurrentBag<(long expiry, TK[] keys)> _ttlsPendingProcessing = new ConcurrentBag<(long, TK[])>();

        // Max number of items to hold. Each time the key processor runs it will ensure the count stays below this value
        private readonly int? _maxItems;

        // Every 10 seconds we check for and remove any expired keys, then add pending (ttl, key) pairs to the _ttls
        // dictionary, then if we are above the _maxItems count we remove keys starting with those which are next to
        // expire. Disposing of this field is the only way to stop that process
        private readonly IDisposable _keyProcessor;
        
        public DictionaryCache(string cacheName, IEqualityComparer<TK> keyComparer = null, int? maxItems = null)
        {
            CacheName = cacheName;

            _values = new ConcurrentDictionary<TK, (TV Value, long Expiry)>(keyComparer ?? EqualityComparer<TK>.Default);
            _maxItems = maxItems;

            _keyProcessor = Observable
                .Interval(TimeSpan.FromSeconds(10))
                .Subscribe(_ => ProcessKeys());
        }
        
        public string CacheName { get; }
        public string CacheType { get; } = "dictionary";
        public bool RequiresKeySerializer { get; } = false;
        public bool RequiresKeyComparer { get; } = true;
        
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
            
            _values[key] = (value, expiryTicks);

            _ttlsPendingProcessing.Add((expiryTicks, new[] { key.AsObject }));
        }
        
        public IList<GetFromCacheResult<TK, TV>> Get(IReadOnlyCollection<Key<TK>> keys)
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

        public void Set(IReadOnlyCollection<KeyValuePair<Key<TK>, TV>> values, TimeSpan timeToLive)
        {
            var expiryTicks = Timestamp.Now + timeToLive.Ticks;
            
            foreach (var kv in values)
                _values[kv.Key] = (kv.Value, expiryTicks);

            _ttlsPendingProcessing.Add((expiryTicks, values.Select(v => v.Key.AsObject).ToArray()));
        }

        public bool Remove(Key<TK> key)
        {
            return _values.TryRemove(key, out _);
        }

        public long Count => _values.Count;

        public void Dispose()
        {
            _keyProcessor?.Dispose();
        }

        private void ProcessKeys()
        {
            RemoveExpiredKeys();
            ProcessNewKeys();
            TrimExcessKeys();
        }

        private void RemoveExpiredKeys()
        {
            var timestampSeconds = (int) (Timestamp.Now / TimeSpan.TicksPerSecond);

            // Since _ttls is a dictionary sorted by expiry timestamp, the first item will be the next to expire
            var next = _ttls.FirstOrDefault();
            while (0 < next.Key && next.Key < timestampSeconds)
            {
                foreach (var key in next.Value)
                    _values.TryRemove(key, out _);
                
                _ttls.Remove(next.Key);

                next = _ttls.FirstOrDefault();
            }
        }

        private void ProcessNewKeys()
        {
            var now = Timestamp.Now;

            while (_ttlsPendingProcessing.TryTake(out var next))
            {
                if (next.expiry < now)
                {
                    foreach (var key in next.keys)
                        _values.TryRemove(key, out _);
                }
                else
                {
                    var expirySeconds = (int) (next.expiry / TimeSpan.TicksPerSecond);
                    if (!_ttls.TryGetValue(expirySeconds, out var keys))
                    {
                        keys = new List<TK>();
                        _ttls[expirySeconds] = keys;
                    }

                    keys.AddRange(next.keys);
                }
            }
        }

        private void TrimExcessKeys()
        {
            if (!_maxItems.HasValue)
                return;

            var countToRemove = _values.Count - _maxItems.Value;

            while (countToRemove > 0)
            {
                var nextToRemove = _ttls.FirstOrDefault();

                if (nextToRemove.Value == null)
                    return;

                for (var i = nextToRemove.Value.Count - 1; i >= 0; i--)
                {
                    var key = nextToRemove.Value[i];
                    nextToRemove.Value.RemoveAt(i);
                    
                    if (!_values.TryRemove(key, out _))
                        continue;
                    
                    if (--countToRemove == 0)
                        return;
                }

                if (countToRemove > 0)
                    _ttls.Remove(nextToRemove.Key);
            }
        }
    }
}
