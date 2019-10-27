using System;
using System.Collections.Generic;
using System.Linq;

namespace CacheMeIfYouCan.Notifications
{
    public abstract class CacheSetResult
    {
        private readonly Lazy<IReadOnlyCollection<string>> _keys;

        internal CacheSetResult(
            string cacheName,
            string cacheType,
            bool success,
            DateTime start,
            TimeSpan duration,
            int keysCount,
            Lazy<IReadOnlyCollection<string>> keys)
        {
            CacheName = cacheName;
            CacheType = cacheType;
            Success = success;
            Start = start;
            Duration = duration;
            KeysCount = keysCount;
            _keys = keys;
        }
        
        public string CacheName { get; }
        public string CacheType { get; }
        public bool Success { get; }
        public DateTime Start { get; }
        public TimeSpan Duration { get; }
        public int KeysCount { get; }
        public IReadOnlyCollection<string> Keys => _keys.Value;
    }
    
    public sealed class CacheSetResult<TK, TV> : CacheSetResult
    {
        internal CacheSetResult(
            string cacheName,
            string cacheType,
            IReadOnlyCollection<KeyValuePair<Key<TK>, TV>> values,
            bool success,
            DateTime start,
            TimeSpan duration)
        : base(
            cacheName,
            cacheType,
            success,
            start,
            duration,
            values.Count,
            new Lazy<IReadOnlyCollection<string>>(() => values.Select(kv => kv.Key.AsStringSafe).ToList()))
        {
            Values = values;
        }
        
        public IReadOnlyCollection<KeyValuePair<Key<TK>, TV>> Values { get; }
    }
}