using System;
using System.Collections.Generic;
using System.Linq;
using CacheMeIfYouCan.Caches;

namespace CacheMeIfYouCan.Notifications
{
    public abstract class CacheGetResult
    {
        public readonly string CacheName;
        public readonly string CacheType;
        public readonly bool Success;
        public readonly long Start;
        public readonly long Duration;
        public readonly int HitsCount;
        public readonly int MissesCount;
        
        private readonly Lazy<IList<string>> _hits;
        private readonly Lazy<IList<string>> _misses;

        internal CacheGetResult(
            string cacheName,
            string cacheType,
            bool success,
            long start,
            long duration,
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

        public IList<string> Hits => _hits.Value;
        public IList<string> Misses => _misses.Value;
    }
    
    public sealed class CacheGetResult<TK, TV> : CacheGetResult
    {
        public new readonly ICollection<GetFromCacheResult<TK, TV>> Hits;
        public new readonly ICollection<Key<TK>> Misses;

        internal CacheGetResult(
            string cacheName,
            string cacheType,
            ICollection<GetFromCacheResult<TK, TV>> hits,
            ICollection<Key<TK>> misses,
            bool success,
            long start,
            long duration)
        : base(
            cacheName,
            cacheType,
            success,
            start,
            duration,
            hits.Count,
            misses.Count,
            new Lazy<IList<string>>(() => hits.Select(r => r.Key.AsString).ToArray()),
            new Lazy<IList<string>>(() => misses.Select(m => m.AsString).ToArray()))
        {
            Hits = hits;
            Misses = misses;
        }
    }
}