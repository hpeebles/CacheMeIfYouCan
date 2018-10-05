using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using CacheMeIfYouCan.Configuration;

namespace CacheMeIfYouCan
{
    public static class FunctionExtensions
    {
        // SingleKey
        public static FunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<TK, Task<TV>> func, string cacheName = null)
        {
            return new FunctionCacheConfigurationManager<TK, TV>(func, cacheName);
        }
        
        public static FunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<TK, TV> func, string cacheName = null)
        {
            return new FunctionCacheConfigurationManager<TK, TV>(key => Task.FromResult(func(key)), cacheName);
        }
        
        // MultiKey
        // IEnumerable input
        // Async
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<IEnumerable<TK>, Task<IDictionary<TK, TV>>> func, string cacheName = null)
        {
            return new MultiKeyFunctionCacheConfigurationManager<TK, TV>(func, cacheName);
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<IEnumerable<TK>, Task<Dictionary<TK, TV>>> func, string cacheName = null)
        {
            return new MultiKeyFunctionCacheConfigurationManager<TK, TV>(async keys => await func(keys), cacheName);
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<IEnumerable<TK>, Task<SortedDictionary<TK, TV>>> func, string cacheName = null)
        {
            return new MultiKeyFunctionCacheConfigurationManager<TK, TV>(async keys => await func(keys), cacheName);
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<IEnumerable<TK>, Task<ConcurrentDictionary<TK, TV>>> func, string cacheName = null)
        {
            return new MultiKeyFunctionCacheConfigurationManager<TK, TV>(async keys => await func(keys), cacheName);
        }
        
        // Sync
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<IEnumerable<TK>, IDictionary<TK, TV>> func, string cacheName = null)
        {
            return new MultiKeyFunctionCacheConfigurationManager<TK, TV>(key => Task.FromResult(func(key)), cacheName);
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<IEnumerable<TK>, Dictionary<TK, TV>> func, string cacheName = null)
        {
            return new MultiKeyFunctionCacheConfigurationManager<TK, TV>(key => Task.FromResult<IDictionary<TK, TV>>(func(key)), cacheName);
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<IEnumerable<TK>, SortedDictionary<TK, TV>> func, string cacheName = null)
        {
            return new MultiKeyFunctionCacheConfigurationManager<TK, TV>(key => Task.FromResult<IDictionary<TK, TV>>(func(key)), cacheName);
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<IEnumerable<TK>, ConcurrentDictionary<TK, TV>> func, string cacheName = null)
        {
            return new MultiKeyFunctionCacheConfigurationManager<TK, TV>(key => Task.FromResult<IDictionary<TK, TV>>(func(key)), cacheName);
        }
        
        // IList input
        // Async
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<IList<TK>, Task<IDictionary<TK, TV>>> func, string cacheName = null)
        {
            return new MultiKeyFunctionCacheConfigurationManager<TK, TV>(keys => func((IList<TK>)keys), cacheName);
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<IList<TK>, Task<Dictionary<TK, TV>>> func, string cacheName = null)
        {
            return new MultiKeyFunctionCacheConfigurationManager<TK, TV>(async keys => await func((IList<TK>)keys), cacheName);
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<IList<TK>, Task<SortedDictionary<TK, TV>>> func, string cacheName = null)
        {
            return new MultiKeyFunctionCacheConfigurationManager<TK, TV>(async keys => await func((IList<TK>)keys), cacheName);
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<IList<TK>, Task<ConcurrentDictionary<TK, TV>>> func, string cacheName = null)
        {
            return new MultiKeyFunctionCacheConfigurationManager<TK, TV>(async keys => await func((IList<TK>)keys), cacheName);
        }
        
        // Sync
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<IList<TK>, IDictionary<TK, TV>> func, string cacheName = null)
        {
            return new MultiKeyFunctionCacheConfigurationManager<TK, TV>(keys => Task.FromResult(func((IList<TK>)keys)), cacheName);
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<IList<TK>, Dictionary<TK, TV>> func, string cacheName = null)
        {
            return new MultiKeyFunctionCacheConfigurationManager<TK, TV>(keys => Task.FromResult<IDictionary<TK, TV>>(func((IList<TK>)keys)), cacheName);
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<IList<TK>, SortedDictionary<TK, TV>> func, string cacheName = null)
        {
            return new MultiKeyFunctionCacheConfigurationManager<TK, TV>(keys => Task.FromResult<IDictionary<TK, TV>>(func((IList<TK>)keys)), cacheName);
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<IList<TK>, ConcurrentDictionary<TK, TV>> func, string cacheName = null)
        {
            return new MultiKeyFunctionCacheConfigurationManager<TK, TV>(keys => Task.FromResult<IDictionary<TK, TV>>(func((IList<TK>)keys)), cacheName);
        }
        
        // List input
        // Async
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<List<TK>, Task<IDictionary<TK, TV>>> func, string cacheName = null)
        {
            return new MultiKeyFunctionCacheConfigurationManager<TK, TV>(keys => func((List<TK>)keys), cacheName);
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<List<TK>, Task<Dictionary<TK, TV>>> func, string cacheName = null)
        {
            return new MultiKeyFunctionCacheConfigurationManager<TK, TV>(async keys => await func((List<TK>)keys), cacheName);
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<List<TK>, Task<SortedDictionary<TK, TV>>> func, string cacheName = null)
        {
            return new MultiKeyFunctionCacheConfigurationManager<TK, TV>(async keys => await func((List<TK>)keys), cacheName);
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<List<TK>, Task<ConcurrentDictionary<TK, TV>>> func, string cacheName = null)
        {
            return new MultiKeyFunctionCacheConfigurationManager<TK, TV>(async keys => await func((List<TK>)keys), cacheName);
        }
        
        // Sync
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<List<TK>, IDictionary<TK, TV>> func, string cacheName = null)
        {
            return new MultiKeyFunctionCacheConfigurationManager<TK, TV>(keys => Task.FromResult(func((List<TK>)keys)), cacheName);
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<List<TK>, Dictionary<TK, TV>> func, string cacheName = null)
        {
            return new MultiKeyFunctionCacheConfigurationManager<TK, TV>(keys => Task.FromResult<IDictionary<TK, TV>>(func((List<TK>)keys)), cacheName);
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<List<TK>, SortedDictionary<TK, TV>> func, string cacheName = null)
        {
            return new MultiKeyFunctionCacheConfigurationManager<TK, TV>(keys => Task.FromResult<IDictionary<TK, TV>>(func((List<TK>)keys)), cacheName);
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<List<TK>, ConcurrentDictionary<TK, TV>> func, string cacheName = null)
        {
            return new MultiKeyFunctionCacheConfigurationManager<TK, TV>(keys => Task.FromResult<IDictionary<TK, TV>>(func((List<TK>)keys)), cacheName);
        }
        
        // Array input
        // Async
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<TK[], Task<IDictionary<TK, TV>>> func, string cacheName = null)
        {
            return new MultiKeyFunctionCacheConfigurationManager<TK, TV>(keys => func((TK[])keys), cacheName);
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<TK[], Task<Dictionary<TK, TV>>> func, string cacheName = null)
        {
            return new MultiKeyFunctionCacheConfigurationManager<TK, TV>(async keys => await func((TK[])keys), cacheName);
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<TK[], Task<SortedDictionary<TK, TV>>> func, string cacheName = null)
        {
            return new MultiKeyFunctionCacheConfigurationManager<TK, TV>(async keys => await func((TK[])keys), cacheName);
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<TK[], Task<ConcurrentDictionary<TK, TV>>> func, string cacheName = null)
        {
            return new MultiKeyFunctionCacheConfigurationManager<TK, TV>(async keys => await func((TK[])keys), cacheName);
        }
        
        // Sync
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<TK[], IDictionary<TK, TV>> func, string cacheName = null)
        {
            return new MultiKeyFunctionCacheConfigurationManager<TK, TV>(keys => Task.FromResult(func((TK[])keys)), cacheName);
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<TK[], Dictionary<TK, TV>> func, string cacheName = null)
        {
            return new MultiKeyFunctionCacheConfigurationManager<TK, TV>(keys => Task.FromResult<IDictionary<TK, TV>>(func((TK[])keys)), cacheName);
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<TK[], SortedDictionary<TK, TV>> func, string cacheName = null)
        {
            return new MultiKeyFunctionCacheConfigurationManager<TK, TV>(keys => Task.FromResult<IDictionary<TK, TV>>(func((TK[])keys)), cacheName);
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<TK[], ConcurrentDictionary<TK, TV>> func, string cacheName = null)
        {
            return new MultiKeyFunctionCacheConfigurationManager<TK, TV>(keys => Task.FromResult<IDictionary<TK, TV>>(func((TK[])keys)), cacheName);
        }
        
        // ICollection input
        // Async
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<ICollection<TK>, Task<IDictionary<TK, TV>>> func, string cacheName = null)
        {
            return new MultiKeyFunctionCacheConfigurationManager<TK, TV>(keys => func((ICollection<TK>)keys), cacheName);
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<ICollection<TK>, Task<Dictionary<TK, TV>>> func, string cacheName = null)
        {
            return new MultiKeyFunctionCacheConfigurationManager<TK, TV>(async keys => await func((ICollection<TK>)keys), cacheName);
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<ICollection<TK>, Task<SortedDictionary<TK, TV>>> func, string cacheName = null)
        {
            return new MultiKeyFunctionCacheConfigurationManager<TK, TV>(async keys => await func((ICollection<TK>)keys), cacheName);
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<ICollection<TK>, Task<ConcurrentDictionary<TK, TV>>> func, string cacheName = null)
        {
            return new MultiKeyFunctionCacheConfigurationManager<TK, TV>(async keys => await func((ICollection<TK>)keys), cacheName);
        }
        
        // Sync
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<ICollection<TK>, IDictionary<TK, TV>> func, string cacheName = null)
        {
            return new MultiKeyFunctionCacheConfigurationManager<TK, TV>(keys => Task.FromResult(func((ICollection<TK>)keys)), cacheName);
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<ICollection<TK>, Dictionary<TK, TV>> func, string cacheName = null)
        {
            return new MultiKeyFunctionCacheConfigurationManager<TK, TV>(keys => Task.FromResult<IDictionary<TK, TV>>(func((ICollection<TK>)keys)), cacheName);
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<ICollection<TK>, SortedDictionary<TK, TV>> func, string cacheName = null)
        {
            return new MultiKeyFunctionCacheConfigurationManager<TK, TV>(keys => Task.FromResult<IDictionary<TK, TV>>(func((ICollection<TK>)keys)), cacheName);
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<ICollection<TK>, ConcurrentDictionary<TK, TV>> func, string cacheName = null)
        {
            return new MultiKeyFunctionCacheConfigurationManager<TK, TV>(keys => Task.FromResult<IDictionary<TK, TV>>(func((ICollection<TK>)keys)), cacheName);
        }
        
        // ISet input
        // Async
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<ISet<TK>, Task<IDictionary<TK, TV>>> func, string cacheName = null)
        {
            return new MultiKeyFunctionCacheConfigurationManager<TK, TV>(keys => func((ISet<TK>)keys), cacheName);
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<ISet<TK>, Task<Dictionary<TK, TV>>> func, string cacheName = null)
        {
            return new MultiKeyFunctionCacheConfigurationManager<TK, TV>(async keys => await func((ISet<TK>)keys), cacheName);
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<ISet<TK>, Task<SortedDictionary<TK, TV>>> func, string cacheName = null)
        {
            return new MultiKeyFunctionCacheConfigurationManager<TK, TV>(async keys => await func((ISet<TK>)keys), cacheName);
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<ISet<TK>, Task<ConcurrentDictionary<TK, TV>>> func, string cacheName = null)
        {
            return new MultiKeyFunctionCacheConfigurationManager<TK, TV>(async keys => await func((ISet<TK>)keys), cacheName);
        }
        
        // Sync
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<ISet<TK>, IDictionary<TK, TV>> func, string cacheName = null)
        {
            return new MultiKeyFunctionCacheConfigurationManager<TK, TV>(keys => Task.FromResult(func((ISet<TK>)keys)), cacheName);
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<ISet<TK>, Dictionary<TK, TV>> func, string cacheName = null)
        {
            return new MultiKeyFunctionCacheConfigurationManager<TK, TV>(keys => Task.FromResult<IDictionary<TK, TV>>(func((ISet<TK>)keys)), cacheName);
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<ISet<TK>, SortedDictionary<TK, TV>> func, string cacheName = null)
        {
            return new MultiKeyFunctionCacheConfigurationManager<TK, TV>(keys => Task.FromResult<IDictionary<TK, TV>>(func((ISet<TK>)keys)), cacheName);
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<ISet<TK>, ConcurrentDictionary<TK, TV>> func, string cacheName = null)
        {
            return new MultiKeyFunctionCacheConfigurationManager<TK, TV>(keys => Task.FromResult<IDictionary<TK, TV>>(func((ISet<TK>)keys)), cacheName);
        }
        
        // HashSet input
        // Async
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<HashSet<TK>, Task<IDictionary<TK, TV>>> func, string cacheName = null)
        {
            return new MultiKeyFunctionCacheConfigurationManager<TK, TV>(keys => func((HashSet<TK>)keys), cacheName);
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<HashSet<TK>, Task<Dictionary<TK, TV>>> func, string cacheName = null)
        {
            return new MultiKeyFunctionCacheConfigurationManager<TK, TV>(async keys => await func((HashSet<TK>)keys), cacheName);
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<HashSet<TK>, Task<SortedDictionary<TK, TV>>> func, string cacheName = null)
        {
            return new MultiKeyFunctionCacheConfigurationManager<TK, TV>(async keys => await func((HashSet<TK>)keys), cacheName);
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<HashSet<TK>, Task<ConcurrentDictionary<TK, TV>>> func, string cacheName = null)
        {
            return new MultiKeyFunctionCacheConfigurationManager<TK, TV>(async keys => await func((HashSet<TK>)keys), cacheName);
        }
        
        // Sync
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<HashSet<TK>, IDictionary<TK, TV>> func, string cacheName = null)
        {
            return new MultiKeyFunctionCacheConfigurationManager<TK, TV>(keys => Task.FromResult(func((HashSet<TK>)keys)), cacheName);
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<HashSet<TK>, Dictionary<TK, TV>> func, string cacheName = null)
        {
            return new MultiKeyFunctionCacheConfigurationManager<TK, TV>(keys => Task.FromResult<IDictionary<TK, TV>>(func((HashSet<TK>)keys)), cacheName);
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<HashSet<TK>, SortedDictionary<TK, TV>> func, string cacheName = null)
        {
            return new MultiKeyFunctionCacheConfigurationManager<TK, TV>(keys => Task.FromResult<IDictionary<TK, TV>>(func((HashSet<TK>)keys)), cacheName);
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<HashSet<TK>, ConcurrentDictionary<TK, TV>> func, string cacheName = null)
        {
            return new MultiKeyFunctionCacheConfigurationManager<TK, TV>(keys => Task.FromResult<IDictionary<TK, TV>>(func((HashSet<TK>)keys)), cacheName);
        }
        
        // SortedSet input
        // Async
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<SortedSet<TK>, Task<IDictionary<TK, TV>>> func, string cacheName = null)
        {
            return new MultiKeyFunctionCacheConfigurationManager<TK, TV>(keys => func((SortedSet<TK>)keys), cacheName);
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<SortedSet<TK>, Task<Dictionary<TK, TV>>> func, string cacheName = null)
        {
            return new MultiKeyFunctionCacheConfigurationManager<TK, TV>(async keys => await func((SortedSet<TK>)keys), cacheName);
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<SortedSet<TK>, Task<SortedDictionary<TK, TV>>> func, string cacheName = null)
        {
            return new MultiKeyFunctionCacheConfigurationManager<TK, TV>(async keys => await func((SortedSet<TK>)keys), cacheName);
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<SortedSet<TK>, Task<ConcurrentDictionary<TK, TV>>> func, string cacheName = null)
        {
            return new MultiKeyFunctionCacheConfigurationManager<TK, TV>(async keys => await func((SortedSet<TK>)keys), cacheName);
        }
        
        // Sync
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<SortedSet<TK>, IDictionary<TK, TV>> func, string cacheName = null)
        {
            return new MultiKeyFunctionCacheConfigurationManager<TK, TV>(keys => Task.FromResult(func((SortedSet<TK>)keys)), cacheName);
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<SortedSet<TK>, Dictionary<TK, TV>> func, string cacheName = null)
        {
            return new MultiKeyFunctionCacheConfigurationManager<TK, TV>(keys => Task.FromResult<IDictionary<TK, TV>>(func((SortedSet<TK>)keys)), cacheName);
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<SortedSet<TK>, SortedDictionary<TK, TV>> func, string cacheName = null)
        {
            return new MultiKeyFunctionCacheConfigurationManager<TK, TV>(keys => Task.FromResult<IDictionary<TK, TV>>(func((SortedSet<TK>)keys)), cacheName);
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<SortedSet<TK>, ConcurrentDictionary<TK, TV>> func, string cacheName = null)
        {
            return new MultiKeyFunctionCacheConfigurationManager<TK, TV>(keys => Task.FromResult<IDictionary<TK, TV>>(func((SortedSet<TK>)keys)), cacheName);
        }
    }
}