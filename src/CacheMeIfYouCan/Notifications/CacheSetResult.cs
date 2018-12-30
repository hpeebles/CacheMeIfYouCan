using System;
using System.Collections.Generic;
using System.Linq;

namespace CacheMeIfYouCan.Notifications
{
    public abstract class CacheSetResult
    {
        private readonly Lazy<IList<string>> _keys;

        internal CacheSetResult(
            string cacheName,
            string cacheType,
            bool success,
            long start,
            TimeSpan duration,
            int keysCount,
            Lazy<IList<string>> keys)
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
        public long Start { get; }
        public TimeSpan Duration { get; }
        public int KeysCount { get; }
        public IList<string> Keys => _keys.Value;
    }
    
    public sealed class CacheSetResult<TK, TV> : CacheSetResult
    {
        internal CacheSetResult(
            string cacheName,
            string cacheType,
            ICollection<KeyValuePair<Key<TK>, TV>> values,
            bool success,
            long start,
            TimeSpan duration)
        : base(
            cacheName,
            cacheType,
            success,
            start,
            duration,
            values.Count,
            new Lazy<IList<string>>(() => values.Select(kv => kv.Key.AsStringSafe).ToArray()))
        {
            Values = values;
        }
        
        public ICollection<KeyValuePair<Key<TK>, TV>> Values { get; }
    }
}