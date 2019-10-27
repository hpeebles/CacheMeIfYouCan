using System;
using System.Collections.Generic;
using System.Linq;

namespace CacheMeIfYouCan.Notifications
{
    public abstract class CacheGetResult
    {
        private readonly Lazy<IReadOnlyCollection<string>> _hits;
        private readonly Lazy<IReadOnlyCollection<string>> _misses;
        private readonly Lazy<IReadOnlyCollection<StatusCodeCount>> _statusCodeCounts;

        internal CacheGetResult(
            string cacheName,
            string cacheType,
            bool success,
            DateTime start,
            TimeSpan duration,
            int hitsCount,
            int missesCount,
            Lazy<IReadOnlyCollection<string>> hits,
            Lazy<IReadOnlyCollection<string>> misses,
            Lazy<IReadOnlyCollection<StatusCodeCount>> statusCodeCounts)
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
        public IReadOnlyCollection<string> Hits => _hits.Value;
        public IReadOnlyCollection<string> Misses => _misses.Value;
        public IReadOnlyCollection<StatusCodeCount> StatusCodeCounts => _statusCodeCounts.Value;

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
            IReadOnlyCollection<GetFromCacheResult<TK, TV>> hits,
            IReadOnlyCollection<Key<TK>> misses,
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
            new Lazy<IReadOnlyCollection<string>>(() => hits.Select(r => r.Key.AsStringSafe).ToArray()),
            new Lazy<IReadOnlyCollection<string>>(() => misses.Select(m => m.AsStringSafe).ToArray()),
            new Lazy<IReadOnlyCollection<StatusCodeCount>>(() => hits
                .Where(h => h.StatusCode > 0)
                .GroupBy(h => h.StatusCode)
                .Select(g => new StatusCodeCount(g.Key, g.Count()))
                .ToArray()))
        {
            Hits = hits;
            Misses = misses;
        }
        
        public new IReadOnlyCollection<GetFromCacheResult<TK, TV>> Hits { get; }
        public new IReadOnlyCollection<Key<TK>> Misses { get; }
    }
}