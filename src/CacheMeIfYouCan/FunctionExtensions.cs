using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CacheMeIfYouCan.Configuration;
using CacheMeIfYouCan.Internal;

namespace CacheMeIfYouCan
{
    public static class FunctionExtensions
    {
        // SingleKey
        public static FunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(
            this Func<TK, Task<TV>> func)
        {
            return Cached(func, BuildCacheName<TK, TV>());
        }

        public static FunctionCacheConfigurationManagerSync<TK, TV> Cached<TK, TV>(
            this Func<TK, TV> func)
        {
            return Cached(func, BuildCacheName<TK, TV>());
        }
        
        public static FunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(
            this Func<TK, Task<TV>> func, string cacheName)
        {
            EnsureNotEnumerable<TK, TV>();
            
            return new FunctionCacheConfigurationManager<TK, TV>(func, cacheName);
        }
        
        public static FunctionCacheConfigurationManagerSync<TK, TV> Cached<TK, TV>(
            this Func<TK, TV> func, string cacheName)
        {
            EnsureNotEnumerable<TK, TV>();
            
            return new FunctionCacheConfigurationManagerSync<TK, TV>(func, cacheName);
        }
        
        // MultiKey
        public static MultiKeyFunctionCacheConfigurationManager<TReq, TRes, TK, TV> Cached<TReq, TRes, TK, TV>(
            this Func<TReq, Task<TRes>> func)
            where TReq : IEnumerable<TK>
            where TRes : IDictionary<TK, TV>
        {
            return new MultiKeyFunctionCacheConfigurationManager<TReq, TRes, TK, TV>(
                func, BuildCacheName<TK, TV>());
        }
        
        public static MultiKeyFunctionCacheConfigurationManagerSync<TReq, TRes, TK, TV> Cached<TReq, TRes, TK, TV>(
            this Func<TReq, TRes> func)
            where TReq : IEnumerable<TK>
            where TRes : IDictionary<TK, TV>
        {
            return new MultiKeyFunctionCacheConfigurationManagerSync<TReq, TRes, TK, TV>(
                func, BuildCacheName<TK, TV>());
        }
        
        public static MultiKeyFunctionCacheConfigurationManager<TReq, TRes, TK, TV> CachedAsync<TReq, TRes, TK, TV>(
            this Func<TReq, TRes> func)
            where TReq : IEnumerable<TK>
            where TRes : IDictionary<TK, TV>
        {
            return new MultiKeyFunctionCacheConfigurationManager<TReq, TRes, TK, TV>(
                func.ConvertToAsync(), BuildCacheName<TK, TV>());
        }
        
        private static string BuildCacheName<TK, TV>()
        {
            return $"FunctionCache_{typeof(TK).Name}->{typeof(TV).Name}";
        }

        private static void EnsureNotEnumerable<TK, TV>()
        {
            if (typeof(TK) == typeof(string))
                return;
            
            if (typeof(IEnumerable).IsAssignableFrom(typeof(TK)) &&
                typeof(IEnumerable).IsAssignableFrom(typeof(TV)))
            {
                throw new Exception("");
            }
        }
    }
}
