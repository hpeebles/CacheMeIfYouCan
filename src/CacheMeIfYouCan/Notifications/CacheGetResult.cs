using System;
using System.Collections.Generic;
using System.Linq;

namespace CacheMeIfYouCan.Notifications
{
    public abstract class CacheGetResult
    {
        private readonly Lazy<IList<string>> _hits;
        private readonly Lazy<IList<string>> _misses;

        internal CacheGetResult(
            string cacheName,
            string cacheType,
            bool success,
            long start,
            TimeSpan duration,
            int hitsCount,
            int missesCount,
            Lazy<IList<string>> hits,
            Lazy<IList<string>> misses)
        {
            CacheName = cacheName;
            CacheType = cacheType;
            Success = success;
            Start = start;
            Duration = duration;
            HitsCount = hitsCount;
            MissesCount = missesCount;
            _hits = hits;
            _misses = misses;
        }

        public string CacheName { get; }
        public string CacheType { get; }
        public bool Success { get; }
        public long Start { get; }
        public TimeSpan Duration { get; }
        public int HitsCount { get; }
        public int MissesCount { get; }
        public IList<string> Hits => _hits.Value;
        public IList<string> Misses => _misses.Value;
    }
    
    public sealed class CacheGetResult<TK, TV> : CacheGetResult
    {
        internal CacheGetResult(
            string cacheName,
            string cacheType,
            ICollection<GetFromCacheResult<TK, TV>> hits,
            ICollection<Key<TK>> misses,
            bool success,
            long start,
            TimeSpan duration)
        : base(
            cacheName,
            cacheType,
            success,
            start,
            duration,
            hits.Count,
            misses.Count,
            new Lazy<IList<string>>(() => hits.Select(r => r.Key.AsStringSafe).ToArray()),
            new Lazy<IList<string>>(() => misses.Select(m => m.AsStringSafe).ToArray()))
        {
            Hits = hits;
            Misses = misses;
        }
        
        public new ICollection<GetFromCacheResult<TK, TV>> Hits { get; }
        public new ICollection<Key<TK>> Misses { get; }
    }
}