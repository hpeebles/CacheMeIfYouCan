#if ASYNCLOCAL
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;

namespace CacheMeIfYouCan
{
    public class CacheTraceContainer : IDisposable
    {
        private static readonly AsyncLocal<ImmutableList<CacheTraceContainer>> Containers
            = new AsyncLocal<ImmutableList<CacheTraceContainer>>();

        private readonly object _lock = new object();

        private CacheTraceContainer()
        {
            if (Containers.Value == null)
                Containers.Value = ImmutableList<CacheTraceContainer>.Empty;
            
            Containers.Value = Containers.Value.Add(this);
        }
        
        public static bool Enabled =>
            Containers.Value != null &&
            Containers.Value != ImmutableList<CacheTraceContainer>.Empty;
        
        public static CacheTraceContainer Create()
        {
            return new CacheTraceContainer();
        }

        public List<CacheTrace> Traces { get; } = new List<CacheTrace>();

        public void Dispose()
        {
            Containers.Value = Containers.Value.Remove(this);
        }

        internal static void AddTrace(CacheTrace trace)
        {
            foreach (var container in Containers.Value)
            {
                lock (container._lock)
                    container.Traces.Add(trace);
            }
        }
    }
}
#endif