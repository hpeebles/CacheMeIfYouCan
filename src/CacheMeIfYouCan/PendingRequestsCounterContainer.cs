using System;
using System.Collections.Generic;
using System.Linq;
using CacheMeIfYouCan.Internal;

namespace CacheMeIfYouCan
{
    public static class PendingRequestsCounterContainer
    {
        private static readonly IList<IPendingRequestsCounter> PendingRequestsCounters = new List<IPendingRequestsCounter>();
        private static readonly object Lock = new Object();

        public static IList<PendingRequestsCount> GetCounts()
        {
            lock (Lock)
            {
                return PendingRequestsCounters
                    .Select(c => new PendingRequestsCount(c.Name, c.Type, c.PendingRequestsCount))
                    .ToArray();
            }
        }

        internal static void Add(IPendingRequestsCounter pendingRequestsCounter)
        {
            lock (Lock)
                PendingRequestsCounters.Add(pendingRequestsCounter);
        }

        internal static void Remove(IPendingRequestsCounter pendingRequestsCounter)
        {
            lock (Lock)
                PendingRequestsCounters.Remove(pendingRequestsCounter);
        }
    }
}