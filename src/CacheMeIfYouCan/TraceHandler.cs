#if ASYNCLOCAL
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

namespace CacheMeIfYouCan
{
    public class TraceHandler : IDisposable
    {
        private static readonly AsyncLocal<ImmutableList<TraceHandler>> Containers
            = new AsyncLocal<ImmutableList<TraceHandler>>();

        private readonly Action<TraceHandler> _onDispose;
        private readonly object _lock = new object();

        private TraceHandler(Action<TraceHandler> onDispose)
        {
            if (Containers.Value == null)
                Containers.Value = ImmutableList<TraceHandler>.Empty;
            
            Containers.Value = Containers.Value.Add(this);
            _onDispose = onDispose;
        }
        
        public static bool Enabled =>
            Containers.Value != null &&
            Containers.Value != ImmutableList<TraceHandler>.Empty;
        
        public static TraceHandler StartNew(Action<TraceHandler> onDispose = null)
        {
            return new TraceHandler(onDispose);
        }

        public List<CacheTrace> Traces { get; } = new List<CacheTrace>();
        public CacheTrace Trace => Traces.SingleOrDefault();

        public void Dispose()
        {
            Containers.Value = Containers.Value.Remove(this);
            _onDispose?.Invoke(this);
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