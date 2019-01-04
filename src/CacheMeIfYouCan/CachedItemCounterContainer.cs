using System;
using System.Collections.Generic;
using System.Linq;

namespace CacheMeIfYouCan
{
    /// <summary>
    /// Singleton which holds a reference to each cache which implements <see cref="ICachedItemCounter"/>
    /// and returns on demand the current count of items in each one
    /// </summary>
    public static class CachedItemCounterContainer
    {
        private static readonly IList<ICachedItemCounter> CachedItemCounters = new List<ICachedItemCounter>();
        private static readonly object Lock = new Object();

        /// <summary>
        /// Returns the current count of items in each cache which implements <see cref="ICachedItemCounter"/>
        /// </summary>
        /// <returns>A list containing the current count of items in each cache</returns>
        public static IList<CachedItemCount> GetCounts()
        {
            lock (Lock)
            {
                return CachedItemCounters
                    .Select(c => new CachedItemCount(c.CacheName, c.CacheType, c.Count))
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