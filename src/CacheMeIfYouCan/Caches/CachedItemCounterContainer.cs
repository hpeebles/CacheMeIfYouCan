using System;
using System.Collections.Generic;
using System.Linq;

namespace CacheMeIfYouCan.Caches
{
    public static class CachedItemCounterContainer
    {
        private static readonly IList<ICachedItemCounter> CachedItemCounters = new List<ICachedItemCounter>();
        private static readonly object Lock = new Object();

        public static IList<CachedItemCount> GetCounts()
        {
            lock (Lock)
            {
                return CachedItemCounters
                    .Select(c => new CachedItemCount(c.CacheType, c.FunctionInfo, c.Count))
                    .ToArray();
            }
        }

        internal static void Register(ICachedItemCounter cachedItemCounter)
        {
            lock (Lock)
                CachedItemCounters.Add(cachedItemCounter);
        }
    }
}