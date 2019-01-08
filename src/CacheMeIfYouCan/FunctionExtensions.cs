using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CacheMeIfYouCan.Configuration;

namespace CacheMeIfYouCan
{
    public static class FunctionExtensions
    {
        // SingleKey
        public static FunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<TK, Task<TV>> func)
        {
            return Cached(func, BuildCacheName<TK, TV>());
        }
        
        public static FunctionCacheConfigurationManagerSync<TK, TV> Cached<TK, TV>(this Func<TK, TV> func)
        {
            return Cached(func, BuildCacheName<TK, TV>());
        }
        
        public static FunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<TK, Task<TV>> func, string cacheName)
        {
            return new FunctionCacheConfigurationManager<TK, TV>(func, cacheName);
        }
        
        public static FunctionCacheConfigurationManagerSync<TK, TV> Cached<TK, TV>(this Func<TK, TV> func, string cacheName)
        {
            return new FunctionCacheConfigurationManagerSync<TK, TV>(func, cacheName);
        }
        
        // MultiKey
        // IEnumerable input
        // Async
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<IEnumerable<TK>, Task<IDictionary<TK, TV>>> func)
        {
            return Cached(func, BuildCacheName<TK, TV>());
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<IEnumerable<TK>, Task<Dictionary<TK, TV>>> func)
        {
            return Cached(func, BuildCacheName<TK, TV>());
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<IEnumerable<TK>, Task<SortedDictionary<TK, TV>>> func)
        {
            return Cached(func, BuildCacheName<TK, TV>());
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<IEnumerable<TK>, Task<ConcurrentDictionary<TK, TV>>> func)
        {
            return Cached(func, BuildCacheName<TK, TV>());
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<IEnumerable<TK>, Task<IDictionary<TK, TV>>> func, string cacheName)
        {
            return new MultiKeyFunctionCacheConfigurationManager<TK, TV>(func, cacheName);
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<IEnumerable<TK>, Task<Dictionary<TK, TV>>> func, string cacheName)
        {
            return new MultiKeyFunctionCacheConfigurationManager<TK, TV>(async keys => await func(keys), cacheName);
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<IEnumerable<TK>, Task<SortedDictionary<TK, TV>>> func, string cacheName)
        {
            return new MultiKeyFunctionCacheConfigurationManager<TK, TV>(async keys => await func(keys), cacheName);
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<IEnumerable<TK>, Task<ConcurrentDictionary<TK, TV>>> func, string cacheName)
        {
            return new MultiKeyFunctionCacheConfigurationManager<TK, TV>(async keys => await func(keys), cacheName);
        }
        
        // Sync
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<IEnumerable<TK>, IDictionary<TK, TV>> func)
        {
            return Cached(func, BuildCacheName<TK, TV>());
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<IEnumerable<TK>, Dictionary<TK, TV>> func)
        {
            return Cached(func, BuildCacheName<TK, TV>());
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<IEnumerable<TK>, SortedDictionary<TK, TV>> func)
        {
            return Cached(func, BuildCacheName<TK, TV>());
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<IEnumerable<TK>, ConcurrentDictionary<TK, TV>> func)
        {
            return Cached(func, BuildCacheName<TK, TV>());
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<IEnumerable<TK>, IDictionary<TK, TV>> func, string cacheName)
        {
            return new MultiKeyFunctionCacheConfigurationManager<TK, TV>(key => Task.FromResult(func(key)), cacheName);
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<IEnumerable<TK>, Dictionary<TK, TV>> func, string cacheName)
        {
            return new MultiKeyFunctionCacheConfigurationManager<TK, TV>(key => Task.FromResult<IDictionary<TK, TV>>(func(key)), cacheName);
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<IEnumerable<TK>, SortedDictionary<TK, TV>> func, string cacheName)
        {
            return new MultiKeyFunctionCacheConfigurationManager<TK, TV>(key => Task.FromResult<IDictionary<TK, TV>>(func(key)), cacheName);
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<IEnumerable<TK>, ConcurrentDictionary<TK, TV>> func, string cacheName)
        {
            return new MultiKeyFunctionCacheConfigurationManager<TK, TV>(key => Task.FromResult<IDictionary<TK, TV>>(func(key)), cacheName);
        }
        
        // IList input
        // Async
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<IList<TK>, Task<IDictionary<TK, TV>>> func)
        {
            return Cached(func, BuildCacheName<TK, TV>());
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<IList<TK>, Task<Dictionary<TK, TV>>> func)
        {
            return Cached(func, BuildCacheName<TK, TV>());
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<IList<TK>, Task<SortedDictionary<TK, TV>>> func)
        {
            return Cached(func, BuildCacheName<TK, TV>());
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<IList<TK>, Task<ConcurrentDictionary<TK, TV>>> func)
        {
            return Cached(func, BuildCacheName<TK, TV>());
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<IList<TK>, Task<IDictionary<TK, TV>>> func, string cacheName)
        {
            return new MultiKeyFunctionCacheConfigurationManager<TK, TV>(keys => func(AsIList(keys)), cacheName);
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<IList<TK>, Task<Dictionary<TK, TV>>> func, string cacheName)
        {
            return new MultiKeyFunctionCacheConfigurationManager<TK, TV>(async keys => await func(AsIList(keys)), cacheName);
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<IList<TK>, Task<SortedDictionary<TK, TV>>> func, string cacheName)
        {
            return new MultiKeyFunctionCacheConfigurationManager<TK, TV>(async keys => await func(AsIList(keys)), cacheName);
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<IList<TK>, Task<ConcurrentDictionary<TK, TV>>> func, string cacheName)
        {
            return new MultiKeyFunctionCacheConfigurationManager<TK, TV>(async keys => await func(AsIList(keys)), cacheName);
        }
        
        // Sync
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<IList<TK>, IDictionary<TK, TV>> func)
        {
            return Cached(func, BuildCacheName<TK, TV>());
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<IList<TK>, Dictionary<TK, TV>> func)
        {
            return Cached(func, BuildCacheName<TK, TV>());
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<IList<TK>, SortedDictionary<TK, TV>> func)
        {
            return Cached(func, BuildCacheName<TK, TV>());
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<IList<TK>, ConcurrentDictionary<TK, TV>> func)
        {
            return Cached(func, BuildCacheName<TK, TV>());
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<IList<TK>, IDictionary<TK, TV>> func, string cacheName)
        {
            return new MultiKeyFunctionCacheConfigurationManager<TK, TV>(keys => Task.FromResult(func(AsIList(keys))), cacheName);
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<IList<TK>, Dictionary<TK, TV>> func, string cacheName)
        {
            return new MultiKeyFunctionCacheConfigurationManager<TK, TV>(keys => Task.FromResult<IDictionary<TK, TV>>(func(AsIList(keys))), cacheName);
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<IList<TK>, SortedDictionary<TK, TV>> func, string cacheName)
        {
            return new MultiKeyFunctionCacheConfigurationManager<TK, TV>(keys => Task.FromResult<IDictionary<TK, TV>>(func(AsIList(keys))), cacheName);
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<IList<TK>, ConcurrentDictionary<TK, TV>> func, string cacheName)
        {
            return new MultiKeyFunctionCacheConfigurationManager<TK, TV>(keys => Task.FromResult<IDictionary<TK, TV>>(func(AsIList(keys))), cacheName);
        }
        
        // List input
        // Async
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<List<TK>, Task<IDictionary<TK, TV>>> func)
        {
            return Cached(func, BuildCacheName<TK, TV>());
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<List<TK>, Task<Dictionary<TK, TV>>> func)
        {
            return Cached(func, BuildCacheName<TK, TV>());
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<List<TK>, Task<SortedDictionary<TK, TV>>> func)
        {
            return Cached(func, BuildCacheName<TK, TV>());
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<List<TK>, Task<ConcurrentDictionary<TK, TV>>> func)
        {
            return Cached(func, BuildCacheName<TK, TV>());
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<List<TK>, Task<IDictionary<TK, TV>>> func, string cacheName)
        {
            return new MultiKeyFunctionCacheConfigurationManager<TK, TV>(keys => func(AsList(keys)), cacheName);
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<List<TK>, Task<Dictionary<TK, TV>>> func, string cacheName)
        {
            return new MultiKeyFunctionCacheConfigurationManager<TK, TV>(async keys => await func(AsList(keys)), cacheName);
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<List<TK>, Task<SortedDictionary<TK, TV>>> func, string cacheName)
        {
            return new MultiKeyFunctionCacheConfigurationManager<TK, TV>(async keys => await func(AsList(keys)), cacheName);
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<List<TK>, Task<ConcurrentDictionary<TK, TV>>> func, string cacheName)
        {
            return new MultiKeyFunctionCacheConfigurationManager<TK, TV>(async keys => await func(AsList(keys)), cacheName);
        }
        
        // Sync
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<List<TK>, IDictionary<TK, TV>> func)
        {
            return Cached(func, BuildCacheName<TK, TV>());
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<List<TK>, Dictionary<TK, TV>> func)
        {
            return Cached(func, BuildCacheName<TK, TV>());
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<List<TK>, SortedDictionary<TK, TV>> func)
        {
            return Cached(func, BuildCacheName<TK, TV>());
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<List<TK>, ConcurrentDictionary<TK, TV>> func)
        {
            return Cached(func, BuildCacheName<TK, TV>());
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<List<TK>, IDictionary<TK, TV>> func, string cacheName)
        {
            return new MultiKeyFunctionCacheConfigurationManager<TK, TV>(keys => Task.FromResult(func(AsList(keys))), cacheName);
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<List<TK>, Dictionary<TK, TV>> func, string cacheName)
        {
            return new MultiKeyFunctionCacheConfigurationManager<TK, TV>(keys => Task.FromResult<IDictionary<TK, TV>>(func(AsList(keys))), cacheName);
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<List<TK>, SortedDictionary<TK, TV>> func, string cacheName)
        {
            return new MultiKeyFunctionCacheConfigurationManager<TK, TV>(keys => Task.FromResult<IDictionary<TK, TV>>(func(AsList(keys))), cacheName);
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<List<TK>, ConcurrentDictionary<TK, TV>> func, string cacheName)
        {
            return new MultiKeyFunctionCacheConfigurationManager<TK, TV>(keys => Task.FromResult<IDictionary<TK, TV>>(func(AsList(keys))), cacheName);
        }
        
        // Array input
        // Async
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<TK[], Task<IDictionary<TK, TV>>> func)
        {
            return Cached(func, BuildCacheName<TK, TV>());
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<TK[], Task<Dictionary<TK, TV>>> func)
        {
            return Cached(func, BuildCacheName<TK, TV>());
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<TK[], Task<SortedDictionary<TK, TV>>> func)
        {
            return Cached(func, BuildCacheName<TK, TV>());
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<TK[], Task<ConcurrentDictionary<TK, TV>>> func)
        {
            return Cached(func, BuildCacheName<TK, TV>());
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<TK[], Task<IDictionary<TK, TV>>> func, string cacheName)
        {
            return new MultiKeyFunctionCacheConfigurationManager<TK, TV>(keys => func(AsArray(keys)), cacheName);
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<TK[], Task<Dictionary<TK, TV>>> func, string cacheName)
        {
            return new MultiKeyFunctionCacheConfigurationManager<TK, TV>(async keys => await func(AsArray(keys)), cacheName);
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<TK[], Task<SortedDictionary<TK, TV>>> func, string cacheName)
        {
            return new MultiKeyFunctionCacheConfigurationManager<TK, TV>(async keys => await func(AsArray(keys)), cacheName);
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<TK[], Task<ConcurrentDictionary<TK, TV>>> func, string cacheName)
        {
            return new MultiKeyFunctionCacheConfigurationManager<TK, TV>(async keys => await func(AsArray(keys)), cacheName);
        }

        // Sync
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<TK[], IDictionary<TK, TV>> func)
        {
            return Cached(func, BuildCacheName<TK, TV>());
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<TK[], Dictionary<TK, TV>> func)
        {
            return Cached(func, BuildCacheName<TK, TV>());
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<TK[], SortedDictionary<TK, TV>> func)
        {
            return Cached(func, BuildCacheName<TK, TV>());
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<TK[], ConcurrentDictionary<TK, TV>> func)
        {
            return Cached(func, BuildCacheName<TK, TV>());
        }

        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<TK[], IDictionary<TK, TV>> func, string cacheName)
        {
            return new MultiKeyFunctionCacheConfigurationManager<TK, TV>(keys => Task.FromResult(func(AsArray(keys))), cacheName);
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<TK[], Dictionary<TK, TV>> func, string cacheName)
        {
            return new MultiKeyFunctionCacheConfigurationManager<TK, TV>(keys => Task.FromResult<IDictionary<TK, TV>>(func(AsArray(keys))), cacheName);
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<TK[], SortedDictionary<TK, TV>> func, string cacheName)
        {
            return new MultiKeyFunctionCacheConfigurationManager<TK, TV>(keys => Task.FromResult<IDictionary<TK, TV>>(func(AsArray(keys))), cacheName);
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<TK[], ConcurrentDictionary<TK, TV>> func, string cacheName)
        {
            return new MultiKeyFunctionCacheConfigurationManager<TK, TV>(keys => Task.FromResult<IDictionary<TK, TV>>(func(AsArray(keys))), cacheName);
        }

        // ICollection input
        // Async
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<ICollection<TK>, Task<IDictionary<TK, TV>>> func)
        {
            return Cached(func, BuildCacheName<TK, TV>());
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<ICollection<TK>, Task<Dictionary<TK, TV>>> func)
        {
            return Cached(func, BuildCacheName<TK, TV>());
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<ICollection<TK>, Task<SortedDictionary<TK, TV>>> func)
        {
            return Cached(func, BuildCacheName<TK, TV>());
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<ICollection<TK>, Task<ConcurrentDictionary<TK, TV>>> func)
        {
            return Cached(func, BuildCacheName<TK, TV>());
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<ICollection<TK>, Task<IDictionary<TK, TV>>> func, string cacheName)
        {
            return new MultiKeyFunctionCacheConfigurationManager<TK, TV>(keys => func(AsArray(keys)), cacheName);
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<ICollection<TK>, Task<Dictionary<TK, TV>>> func, string cacheName)
        {
            return new MultiKeyFunctionCacheConfigurationManager<TK, TV>(async keys => await func(AsArray(keys)), cacheName);
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<ICollection<TK>, Task<SortedDictionary<TK, TV>>> func, string cacheName)
        {
            return new MultiKeyFunctionCacheConfigurationManager<TK, TV>(async keys => await func(AsArray(keys)), cacheName);
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<ICollection<TK>, Task<ConcurrentDictionary<TK, TV>>> func, string cacheName)
        {
            return new MultiKeyFunctionCacheConfigurationManager<TK, TV>(async keys => await func(AsArray(keys)), cacheName);
        }

        // Sync
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<ICollection<TK>, IDictionary<TK, TV>> func)
        {
            return Cached(func, BuildCacheName<TK, TV>());
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<ICollection<TK>, Dictionary<TK, TV>> func)
        {
            return Cached(func, BuildCacheName<TK, TV>());
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<ICollection<TK>, SortedDictionary<TK, TV>> func)
        {
            return Cached(func, BuildCacheName<TK, TV>());
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<ICollection<TK>, ConcurrentDictionary<TK, TV>> func)
        {
            return Cached(func, BuildCacheName<TK, TV>());
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<ICollection<TK>, IDictionary<TK, TV>> func, string cacheName)
        {
            return new MultiKeyFunctionCacheConfigurationManager<TK, TV>(keys => Task.FromResult(func(AsArray(keys))), cacheName);
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<ICollection<TK>, Dictionary<TK, TV>> func, string cacheName)
        {
            return new MultiKeyFunctionCacheConfigurationManager<TK, TV>(keys => Task.FromResult<IDictionary<TK, TV>>(func(AsArray(keys))), cacheName);
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<ICollection<TK>, SortedDictionary<TK, TV>> func, string cacheName)
        {
            return new MultiKeyFunctionCacheConfigurationManager<TK, TV>(keys => Task.FromResult<IDictionary<TK, TV>>(func(AsArray(keys))), cacheName);
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<ICollection<TK>, ConcurrentDictionary<TK, TV>> func, string cacheName)
        {
            return new MultiKeyFunctionCacheConfigurationManager<TK, TV>(keys => Task.FromResult<IDictionary<TK, TV>>(func(AsArray(keys))), cacheName);
        }

        // ISet input
        // Async
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<ISet<TK>, Task<IDictionary<TK, TV>>> func)
        {
            return Cached(func, BuildCacheName<TK, TV>());
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<ISet<TK>, Task<Dictionary<TK, TV>>> func)
        {
            return Cached(func, BuildCacheName<TK, TV>());
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<ISet<TK>, Task<SortedDictionary<TK, TV>>> func)
        {
            return Cached(func, BuildCacheName<TK, TV>());
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<ISet<TK>, Task<ConcurrentDictionary<TK, TV>>> func)
        {
            return Cached(func, BuildCacheName<TK, TV>());
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<ISet<TK>, Task<IDictionary<TK, TV>>> func, string cacheName)
        {
            return new MultiKeyFunctionCacheConfigurationManager<TK, TV>(keys => func(AsISet(keys)), cacheName);
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<ISet<TK>, Task<Dictionary<TK, TV>>> func, string cacheName)
        {
            return new MultiKeyFunctionCacheConfigurationManager<TK, TV>(async keys => await func(AsISet(keys)), cacheName);
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<ISet<TK>, Task<SortedDictionary<TK, TV>>> func, string cacheName)
        {
            return new MultiKeyFunctionCacheConfigurationManager<TK, TV>(async keys => await func(AsISet(keys)), cacheName);
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<ISet<TK>, Task<ConcurrentDictionary<TK, TV>>> func, string cacheName)
        {
            return new MultiKeyFunctionCacheConfigurationManager<TK, TV>(async keys => await func(AsISet(keys)), cacheName);
        }

        // Sync
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<ISet<TK>, IDictionary<TK, TV>> func)
        {
            return Cached(func, BuildCacheName<TK, TV>());
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<ISet<TK>, Dictionary<TK, TV>> func)
        {
            return Cached(func, BuildCacheName<TK, TV>());
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<ISet<TK>, SortedDictionary<TK, TV>> func)
        {
            return Cached(func, BuildCacheName<TK, TV>());
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<ISet<TK>, ConcurrentDictionary<TK, TV>> func)
        {
            return Cached(func, BuildCacheName<TK, TV>());
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<ISet<TK>, IDictionary<TK, TV>> func, string cacheName)
        {
            return new MultiKeyFunctionCacheConfigurationManager<TK, TV>(keys => Task.FromResult(func(AsISet(keys))), cacheName);
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<ISet<TK>, Dictionary<TK, TV>> func, string cacheName)
        {
            return new MultiKeyFunctionCacheConfigurationManager<TK, TV>(keys => Task.FromResult<IDictionary<TK, TV>>(func(AsISet(keys))), cacheName);
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<ISet<TK>, SortedDictionary<TK, TV>> func, string cacheName)
        {
            return new MultiKeyFunctionCacheConfigurationManager<TK, TV>(keys => Task.FromResult<IDictionary<TK, TV>>(func(AsISet(keys))), cacheName);
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<ISet<TK>, ConcurrentDictionary<TK, TV>> func, string cacheName)
        {
            return new MultiKeyFunctionCacheConfigurationManager<TK, TV>(keys => Task.FromResult<IDictionary<TK, TV>>(func(AsISet(keys))), cacheName);
        }
        
        // HashSet input
        // Async
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<HashSet<TK>, Task<IDictionary<TK, TV>>> func)
        {
            return Cached(func, BuildCacheName<TK, TV>());
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<HashSet<TK>, Task<Dictionary<TK, TV>>> func)
        {
            return Cached(func, BuildCacheName<TK, TV>());
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<HashSet<TK>, Task<SortedDictionary<TK, TV>>> func)
        {
            return Cached(func, BuildCacheName<TK, TV>());
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<HashSet<TK>, Task<ConcurrentDictionary<TK, TV>>> func)
        {
            return Cached(func, BuildCacheName<TK, TV>());
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<HashSet<TK>, Task<IDictionary<TK, TV>>> func, string cacheName)
        {
            return new MultiKeyFunctionCacheConfigurationManager<TK, TV>(keys => func(AsHashSet(keys)), cacheName);
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<HashSet<TK>, Task<Dictionary<TK, TV>>> func, string cacheName)
        {
            return new MultiKeyFunctionCacheConfigurationManager<TK, TV>(async keys => await func(AsHashSet(keys)), cacheName);
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<HashSet<TK>, Task<SortedDictionary<TK, TV>>> func, string cacheName)
        {
            return new MultiKeyFunctionCacheConfigurationManager<TK, TV>(async keys => await func(AsHashSet(keys)), cacheName);
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<HashSet<TK>, Task<ConcurrentDictionary<TK, TV>>> func, string cacheName)
        {
            return new MultiKeyFunctionCacheConfigurationManager<TK, TV>(async keys => await func(AsHashSet(keys)), cacheName);
        }
        
        // Sync
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<HashSet<TK>, IDictionary<TK, TV>> func)
        {
            return Cached(func, BuildCacheName<TK, TV>());
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<HashSet<TK>, Dictionary<TK, TV>> func)
        {
            return Cached(func, BuildCacheName<TK, TV>());
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<HashSet<TK>, SortedDictionary<TK, TV>> func)
        {
            return Cached(func, BuildCacheName<TK, TV>());
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<HashSet<TK>, ConcurrentDictionary<TK, TV>> func)
        {
            return Cached(func, BuildCacheName<TK, TV>());
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<HashSet<TK>, IDictionary<TK, TV>> func, string cacheName)
        {
            return new MultiKeyFunctionCacheConfigurationManager<TK, TV>(keys => Task.FromResult(func(AsHashSet(keys))), cacheName);
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<HashSet<TK>, Dictionary<TK, TV>> func, string cacheName)
        {
            return new MultiKeyFunctionCacheConfigurationManager<TK, TV>(keys => Task.FromResult<IDictionary<TK, TV>>(func(AsHashSet(keys))), cacheName);
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<HashSet<TK>, SortedDictionary<TK, TV>> func, string cacheName)
        {
            return new MultiKeyFunctionCacheConfigurationManager<TK, TV>(keys => Task.FromResult<IDictionary<TK, TV>>(func(AsHashSet(keys))), cacheName);
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<HashSet<TK>, ConcurrentDictionary<TK, TV>> func, string cacheName)
        {
            return new MultiKeyFunctionCacheConfigurationManager<TK, TV>(keys => Task.FromResult<IDictionary<TK, TV>>(func(AsHashSet(keys))), cacheName);
        }

        // SortedSet input
        // Async
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<SortedSet<TK>, Task<IDictionary<TK, TV>>> func)
        {
            return Cached(func, BuildCacheName<TK, TV>());
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<SortedSet<TK>, Task<Dictionary<TK, TV>>> func)
        {
            return Cached(func, BuildCacheName<TK, TV>());
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<SortedSet<TK>, Task<SortedDictionary<TK, TV>>> func)
        {
            return Cached(func, BuildCacheName<TK, TV>());
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<SortedSet<TK>, Task<ConcurrentDictionary<TK, TV>>> func)
        {
            return Cached(func, BuildCacheName<TK, TV>());
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<SortedSet<TK>, Task<IDictionary<TK, TV>>> func, string cacheName)
        {
            return new MultiKeyFunctionCacheConfigurationManager<TK, TV>(keys => func(AsSortedSet(keys)), cacheName);
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<SortedSet<TK>, Task<Dictionary<TK, TV>>> func, string cacheName)
        {
            return new MultiKeyFunctionCacheConfigurationManager<TK, TV>(async keys => await func(AsSortedSet(keys)), cacheName);
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<SortedSet<TK>, Task<SortedDictionary<TK, TV>>> func, string cacheName)
        {
            return new MultiKeyFunctionCacheConfigurationManager<TK, TV>(async keys => await func(AsSortedSet(keys)), cacheName);
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<SortedSet<TK>, Task<ConcurrentDictionary<TK, TV>>> func, string cacheName)
        {
            return new MultiKeyFunctionCacheConfigurationManager<TK, TV>(async keys => await func(AsSortedSet(keys)), cacheName);
        }
        
        // Sync
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<SortedSet<TK>, IDictionary<TK, TV>> func)
        {
            return Cached(func, BuildCacheName<TK, TV>());
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<SortedSet<TK>, Dictionary<TK, TV>> func)
        {
            return Cached(func, BuildCacheName<TK, TV>());
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<SortedSet<TK>, SortedDictionary<TK, TV>> func)
        {
            return Cached(func, BuildCacheName<TK, TV>());
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<SortedSet<TK>, ConcurrentDictionary<TK, TV>> func)
        {
            return Cached(func, BuildCacheName<TK, TV>());
        }

        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<SortedSet<TK>, IDictionary<TK, TV>> func, string cacheName)
        {
            return new MultiKeyFunctionCacheConfigurationManager<TK, TV>(keys => Task.FromResult(func(AsSortedSet(keys))), cacheName);
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<SortedSet<TK>, Dictionary<TK, TV>> func, string cacheName)
        {
            return new MultiKeyFunctionCacheConfigurationManager<TK, TV>(keys => Task.FromResult<IDictionary<TK, TV>>(func(AsSortedSet(keys))), cacheName);
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<SortedSet<TK>, SortedDictionary<TK, TV>> func, string cacheName)
        {
            return new MultiKeyFunctionCacheConfigurationManager<TK, TV>(keys => Task.FromResult<IDictionary<TK, TV>>(func(AsSortedSet(keys))), cacheName);
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(this Func<SortedSet<TK>, ConcurrentDictionary<TK, TV>> func, string cacheName)
        {
            return new MultiKeyFunctionCacheConfigurationManager<TK, TV>(keys => Task.FromResult<IDictionary<TK, TV>>(func(AsSortedSet(keys))), cacheName);
        }

        private static IList<T> AsIList<T>(IEnumerable<T> items)
        {
            return items as IList<T> ?? items.ToArray();
        }

        private static T[] AsArray<T>(IEnumerable<T> items)
        {
            return items as T[] ?? items.ToArray();
        }
        
        private static List<T> AsList<T>(IEnumerable<T> items)
        {
            return items as List<T> ?? items.ToList();
        }
        
        private static ISet<T> AsISet<T>(IEnumerable<T> items)
        {
            return items as ISet<T> ?? new HashSet<T>(items);
        }
        
        private static HashSet<T> AsHashSet<T>(IEnumerable<T> items)
        {
            return items as HashSet<T> ?? new HashSet<T>(items);
        }
        
        private static SortedSet<T> AsSortedSet<T>(IEnumerable<T> items)
        {
            return items as SortedSet<T> ?? new SortedSet<T>(items);
        }

        private static string BuildCacheName<TK, TV>()
        {
            return $"FunctionCache_{typeof(TK).Name}->{typeof(TV).Name}";
        }
    }
}