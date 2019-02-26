using System;
using System.Collections.Generic;
using System.Linq;

namespace CacheMeIfYouCan.Notifications
{
    public abstract class CacheGetResult
    {
        private readonly Lazy<IList<string>> _hits;
        private readonly Lazy<IList<string>> _misses;
        private readonly Lazy<IList<StatusCodeCount>> _statusCodeCounts;

        internal CacheGetResult(
            string cacheName,
            string cacheType,
            bool success,
            DateTime start,
            TimeSpan duration,
            int hitsCount,
            int missesCount,
            Lazy<IList<string>> hits,
            Lazy<IList<string>> misses,
            Lazy<IList<StatusCodeCount>> statusCodeCounts)
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
            _statusCodeCounts = statusCodeCounts;
        }

        public string CacheName { get; }
        public string CacheType { get; }
        public bool Success { get; }
        public DateTime Start { get; }
        public TimeSpan Duration { get; }
        public int HitsCount { get; }
        public int MissesCount { get; }
        public IList<string> Hits => _hits.Value;
        public IList<string> Misses => _misses.Value;
        public IList<StatusCodeCount> StatusCodeCounts => _statusCodeCounts.Value;

        public readonly struct StatusCodeCount
        {
            internal StatusCodeCount(int statusCode, int count)
            {
                StatusCode = statusCode;
                Count = count;
            }
            
            public int StatusCode { get; }
            public int Count { get; }

            public void Deconstruct(out int statusCode, out int count)
            {
                statusCode = StatusCode;
                count = Count;
            }

            public override string ToString()
            {
                return $"StatusCode: '{StatusCode}' Count: '{Count}";
            }
        }
    }
    
    public sealed class CacheGetResult<TK, TV> : CacheGetResult
    {
        internal CacheGetResult(
            string cacheName,
            string cacheType,
            ICollection<GetFromCacheResult<TK, TV>> hits,
            ICollection<Key<TK>> misses,
            bool success,
            DateTime start,
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
            new Lazy<IList<string>>(() => misses.Select(m => m.AsStringSafe).ToArray()),
            new Lazy<IList<StatusCodeCount>>(() => hits
                .Where(h => h.StatusCode > 0)
                .GroupBy(h => h.StatusCode)
                .Select(g => new StatusCodeCount(g.Key, g.Count()))
                .ToArray()))
        {
            Hits = hits;
            Misses = misses;
        }
        
        public new ICollection<GetFromCacheResult<TK, TV>> Hits { get; }
        public new ICollection<Key<TK>> Misses { get; }
    }
}