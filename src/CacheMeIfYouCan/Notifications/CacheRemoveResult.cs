using System;

namespace CacheMeIfYouCan.Notifications
{
    public abstract class CacheRemoveResult
    {
        internal CacheRemoveResult(
            string cacheName,
            string cacheType,
            bool success,
            bool keyRemoved,
            long start,
            TimeSpan duration,
            string key)
        {
            CacheName = cacheName;
            CacheType = cacheType;
            Success = success;
            KeyRemoved = keyRemoved;
            Start = start;
            Duration = duration;
            Key = key;
        }
        
        public string CacheName { get; }
        public string CacheType { get; }
        public bool Success { get; }
        public bool KeyRemoved { get; }
        public long Start { get; }
        public TimeSpan Duration { get; }
        public string Key { get; }
    }
    
    public sealed class CacheRemoveResult<TK> : CacheRemoveResult
    {
        internal CacheRemoveResult(
            string cacheName,
            string cacheType,
            Key<TK> key,
            bool success,
            bool keyRemoved,
            long start,
            TimeSpan duration)
            : base(
                cacheName,
                cacheType,
                success,
                keyRemoved,
                start,
                duration,
                key.AsStringSafe)
        {
            Key = key;
        }
        
        public new Key<TK> Key { get; }
    }
}