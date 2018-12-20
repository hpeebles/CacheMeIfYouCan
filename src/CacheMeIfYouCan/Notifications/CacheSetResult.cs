using System;
using System.Collections.Generic;
using System.Linq;

namespace CacheMeIfYouCan.Notifications
{
    public abstract class CacheSetResult
    {
        public readonly string CacheName;
        public readonly string CacheType;
        public readonly bool Success;
        public readonly long Start;
        public readonly long Duration;
        public readonly int KeysCount;
        private readonly Lazy<IList<string>> _keys;

        internal CacheSetResult(
            string cacheName,
            string cacheType,
            bool success,
            long start,
            long duration,
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

        public IList<string> Keys => _keys.Value;
    }
    
    public sealed class CacheSetResult<TK, TV> : CacheSetResult
    {
        public readonly ICollection<KeyValuePair<Key<TK>, TV>> Values;

        internal CacheSetResult(
            string cacheName,
            string cacheType,
            ICollection<KeyValuePair<Key<TK>, TV>> values,
            bool success,
            long start,
            long duration)
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
    }
}