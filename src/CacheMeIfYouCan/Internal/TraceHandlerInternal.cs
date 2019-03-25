using System;
using System.Collections.Immutable;
using System.Threading;
using CacheMeIfYouCan.Notifications;

namespace CacheMeIfYouCan.Internal
{
    internal class TraceHandlerInternal : IDisposable
    {
#if ASYNCLOCAL
        private static readonly AsyncLocal<ImmutableStack<CacheTrace>> Traces = new AsyncLocal<ImmutableStack<CacheTrace>>();

        private TraceHandlerInternal()
        {
            if (Traces.Value == null)
                Traces.Value = ImmutableStack<CacheTrace>.Empty;
            
            Traces.Value = Traces.Value.Push(new CacheTrace());
        }

        public void Dispose()
        {
            Traces.Value = Traces.Value.Pop(out var trace);
            TraceHandler.AddTrace(trace);
        }

        public static IDisposable Start()
        {
            return Enabled ? new TraceHandlerInternal() : null;
        }
        
        public static bool Enabled => TraceHandler.Enabled;

        public static void Mark<TK, TV>(FunctionCacheGetResult<TK, TV> result)
        {
            if (Enabled)
                Current.Result = result;
        }
        
        public static void Mark<TK, TV>(FunctionCacheFetchResult<TK, TV> result)
        {
            if (Enabled)
                Current.Fetches.Add(result);
        }

        public static void Mark<TK, TV>(CacheGetResult<TK, TV> result)
        {
            if (Enabled)
                Current.CacheGetResults.Add(result);
        }

        public static void Mark<TK, TV>(CacheSetResult<TK, TV> result)
        {
            if (Enabled)
                Current.CacheSetResults.Add(result);
        }
        
        public static void Mark<TK>(CacheRemoveResult<TK> result)
        {
            if (Enabled)
                Current.CacheRemoveResults.Add(result);
        }

        public static void Mark<TK>(FunctionCacheGetException<TK> exception)
        {
            if (Enabled)
                Current.GetResultException = exception;
        }

        public static void Mark<TK>(FunctionCacheFetchException<TK> exception)
        {
            if (Enabled)
                Current.FetchExceptions.Add(exception);
        }

        public static void Mark<TK>(CacheException<TK> exception)
        {
            if (Enabled)
                Current.CacheExceptions.Add(exception);
        }

        private static CacheTrace Current => Traces.Value.Peek();
#else
        private TraceHandlerInternal() { }

        public static IDisposable Start() => null;
        public void Dispose() { }   
        public static bool Enabled => false;
        public static void Mark<TK, TV>(FunctionCacheGetResult<TK, TV> result) { }
        public static void Mark<TK, TV>(FunctionCacheFetchResult<TK, TV> result) { }
        public static void Mark<TK, TV>(CacheGetResult<TK, TV> result) { }
        public static void Mark<TK, TV>(CacheSetResult<TK, TV> result) { }
        public static void Mark<TK>(CacheRemoveResult<TK> result) { }
        public static void Mark<TK>(FunctionCacheGetException<TK> exception) { }
        public static void Mark<TK>(FunctionCacheFetchException<TK> exception) { }
        public static void Mark<TK>(CacheException<TK> exception) { }
#endif
    }
}