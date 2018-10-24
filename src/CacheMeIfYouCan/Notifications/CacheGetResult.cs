using System;
using System.Collections.Generic;
using System.Linq;
using CacheMeIfYouCan.Caches;

namespace CacheMeIfYouCan.Notifications
{
    public abstract class CacheGetResult
    {
        public readonly FunctionInfo FunctionInfo;
        public readonly string CacheType;
        public readonly bool Success;
        public readonly long Start;
        public readonly long Duration;
        public readonly int HitsCount;
        public readonly int MissesCount;
        
        private readonly Lazy<IList<string>> _hits;
        private readonly Lazy<IList<string>> _misses;

        internal CacheGetResult(
            FunctionInfo functionInfo,
            string cacheType,
            bool success,
            long start,
            long duration,
            int hitsCount,
            int missesCount,
            Lazy<IList<string>> hits,
            Lazy<IList<string>> misses)
        {
            FunctionInfo = functionInfo;
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
            FunctionInfo functionInfo,
            string cacheType,
            ICollection<GetFromCacheResult<TK, TV>> hits,
            ICollection<Key<TK>> misses,
            bool success,
            long start,
            long duration)
        : base(
            functionInfo,
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