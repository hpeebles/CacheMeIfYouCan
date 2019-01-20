using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CacheMeIfYouCan.Configuration;
using CacheMeIfYouCan.Internal;

namespace CacheMeIfYouCan
{
    public static class FunctionExtensions
    {
        public static SingleKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(
            this Func<TK, Task<TV>> func)
        {
            return Cached(func, BuildCacheName<TK, TV>(false));
        }
        
        public static SingleKeyFunctionCacheConfigurationManager<TK, TV> Cached<TK, TV>(
            this Func<TK, Task<TV>> func, string cacheName)
        {
            if (cacheName == null)
                throw new ArgumentNullException(nameof(cacheName));

            return new SingleKeyFunctionCacheConfigurationManager<TK, TV>(func, cacheName);
        }
        
        public static SingleKeyFunctionCacheConfigurationManagerSync<TK, TV> Cached<TK, TV>(
            this Func<TK, TV> func)
        {
            return Cached(func, BuildCacheName<TK, TV>(false));
        }

        public static SingleKeyFunctionCacheConfigurationManagerSync<TK, TV> Cached<TK, TV>(
            this Func<TK, TV> func, string cacheName)
        {
            if (cacheName == null)
                throw new ArgumentNullException(nameof(cacheName));

            return new SingleKeyFunctionCacheConfigurationManagerSync<TK, TV>(func, cacheName);
        }
        
        public static SingleKeyFunctionCacheConfigurationManager<TK, TV> CachedAsync<TK, TV>(
            this Func<TK, TV> func)
        {
            return CachedAsync(func, BuildCacheName<TK, TV>(false));
        }

        public static SingleKeyFunctionCacheConfigurationManager<TK, TV> CachedAsync<TK, TV>(
            this Func<TK, TV> func, string cacheName)
        {
            if (cacheName == null)
                throw new ArgumentNullException(nameof(cacheName));

            return new SingleKeyFunctionCacheConfigurationManager<TK, TV>(func.ConvertToAsync(), cacheName);
        }
        
        public static EnumerableKeyFunctionCacheConfigurationManager<TReq, TRes, TK, TV> Cached<TReq, TRes, TK, TV>(
            this Func<TReq, Task<TRes>> func)
            where TReq : IEnumerable<TK>
            where TRes : IDictionary<TK, TV>
        {
            return Cached<TReq, TRes, TK, TV>(func, BuildCacheName<TK, TV>(true));
        }
        
        public static EnumerableKeyFunctionCacheConfigurationManager<TReq, TRes, TK, TV> Cached<TReq, TRes, TK, TV>(
            this Func<TReq, Task<TRes>> func, string cacheName)
            where TReq : IEnumerable<TK>
            where TRes : IDictionary<TK, TV>
        {
            if (cacheName == null)
                throw new ArgumentNullException(nameof(cacheName));

            return new EnumerableKeyFunctionCacheConfigurationManager<TReq, TRes, TK, TV>(func, cacheName);
        }
        
        public static EnumerableKeyFunctionCacheConfigurationManagerSync<TReq, TRes, TK, TV> Cached<TReq, TRes, TK, TV>(
            this Func<TReq, TRes> func)
            where TReq : IEnumerable<TK>
            where TRes : IDictionary<TK, TV>
        {
            return Cached<TReq, TRes, TK, TV>(func, BuildCacheName<TK, TV>(true));
        }
        
        public static EnumerableKeyFunctionCacheConfigurationManagerSync<TReq, TRes, TK, TV> Cached<TReq, TRes, TK, TV>(
            this Func<TReq, TRes> func, string cacheName)
            where TReq : IEnumerable<TK>
            where TRes : IDictionary<TK, TV>
        {
            if (cacheName == null)
                throw new ArgumentNullException(nameof(cacheName));

            return new EnumerableKeyFunctionCacheConfigurationManagerSync<TReq, TRes, TK, TV>(func, cacheName);
        }
        
        public static EnumerableKeyFunctionCacheConfigurationManager<TReq, TRes, TK, TV> CachedAsync<TReq, TRes, TK, TV>(
            this Func<TReq, TRes> func)
            where TReq : IEnumerable<TK>
            where TRes : IDictionary<TK, TV>
        {
            return CachedAsync<TReq, TRes, TK, TV>(func, BuildCacheName<TK, TV>(true));
        }
        
        public static EnumerableKeyFunctionCacheConfigurationManager<TReq, TRes, TK, TV> CachedAsync<TReq, TRes, TK, TV>(
            this Func<TReq, TRes> func, string cacheName)
            where TReq : IEnumerable<TK>
            where TRes : IDictionary<TK, TV>
        {
            if (cacheName == null)
                throw new ArgumentNullException(nameof(cacheName));
            
            return new EnumerableKeyFunctionCacheConfigurationManager<TReq, TRes, TK, TV>(func.ConvertToAsync(), cacheName);
        }
        
        public static MultiParamFunctionCacheConfigurationManager<TK1, TK2, TV> Cached<TK1, TK2, TV>(
            this Func<TK1, TK2, Task<TV>> func)
        {
            return Cached(func, BuildCacheName<TK1, TK2, TV>(false));
        }
        
        public static MultiParamFunctionCacheConfigurationManager<TK1, TK2, TV> Cached<TK1, TK2, TV>(
            this Func<TK1, TK2, Task<TV>> func, string cacheName)
        {
            if (cacheName == null)
                throw new ArgumentNullException(nameof(cacheName));

            return new MultiParamFunctionCacheConfigurationManager<TK1, TK2, TV>(func, cacheName);
        }
        
        public static MultiParamFunctionCacheConfigurationManagerSync<TK1, TK2, TV> Cached<TK1, TK2, TV>(
            this Func<TK1, TK2, TV> func)
        {
            return Cached(func, BuildCacheName<TK1, TK2, TV>(false));
        }
        
        public static MultiParamFunctionCacheConfigurationManagerSync<TK1, TK2, TV> Cached<TK1, TK2, TV>(
            this Func<TK1, TK2, TV> func, string cacheName)
        {
            if (cacheName == null)
                throw new ArgumentNullException(nameof(cacheName));

            return new MultiParamFunctionCacheConfigurationManagerSync<TK1, TK2, TV>(func, cacheName);
        }
        
        public static MultiParamFunctionCacheConfigurationManager<TK1, TK2, TV> CachedAsync<TK1, TK2, TV>(
            this Func<TK1, TK2, TV> func)
        {
            return CachedAsync(func, BuildCacheName<TK1, TK2, TV>(false));
        }
        
        public static MultiParamFunctionCacheConfigurationManager<TK1, TK2, TV> CachedAsync<TK1, TK2, TV>(
            this Func<TK1, TK2, TV> func, string cacheName)
        {
            if (cacheName == null)
                throw new ArgumentNullException(nameof(cacheName));

            return new MultiParamFunctionCacheConfigurationManager<TK1, TK2, TV>(func.ConvertToAsync(), cacheName);
        }
        
        public static MultiParamFunctionCacheConfigurationManager<TK1, TK2, TK3, TV> Cached<TK1, TK2, TK3, TV>(
            this Func<TK1, TK2, TK3, Task<TV>> func)
        {
            return Cached(func, BuildCacheName<TK1, TK2, TK3, TV>(false));
        }
        
        public static MultiParamFunctionCacheConfigurationManager<TK1, TK2, TK3, TV> Cached<TK1, TK2, TK3, TV>(
            this Func<TK1, TK2, TK3, Task<TV>> func, string cacheName)
        {
            if (cacheName == null)
                throw new ArgumentNullException(nameof(cacheName));

            return new MultiParamFunctionCacheConfigurationManager<TK1, TK2, TK3, TV>(func, cacheName);
        }
        
        public static MultiParamFunctionCacheConfigurationManagerSync<TK1, TK2, TK3, TV> Cached<TK1, TK2, TK3, TV>(
            this Func<TK1, TK2, TK3, TV> func)
        {
            return Cached(func, BuildCacheName<TK1, TK2, TK3, TV>(false));
        }
        
        public static MultiParamFunctionCacheConfigurationManagerSync<TK1, TK2, TK3, TV> Cached<TK1, TK2, TK3, TV>(
            this Func<TK1, TK2, TK3, TV> func, string cacheName)
        {
            if (cacheName == null)
                throw new ArgumentNullException(nameof(cacheName));

            return new MultiParamFunctionCacheConfigurationManagerSync<TK1, TK2, TK3, TV>(func, cacheName);
        }
        
        public static MultiParamFunctionCacheConfigurationManager<TK1, TK2, TK3, TV> CachedAsync<TK1, TK2, TK3, TV>(
            this Func<TK1, TK2, TK3, TV> func)
        {
            return CachedAsync(func, BuildCacheName<TK1, TK2, TK3, TV>(false));
        }
        
        public static MultiParamFunctionCacheConfigurationManager<TK1, TK2, TK3, TV> CachedAsync<TK1, TK2, TK3, TV>(
            this Func<TK1, TK2, TK3, TV> func, string cacheName)
        {
            if (cacheName == null)
                throw new ArgumentNullException(nameof(cacheName));

            return new MultiParamFunctionCacheConfigurationManager<TK1, TK2, TK3, TV>(func.ConvertToAsync(), cacheName);
        }
        
        public static MultiParamFunctionCacheConfigurationManager<TK1, TK2, TK3, TK4, TV> Cached<TK1, TK2, TK3, TK4, TV>(
            this Func<TK1, TK2, TK3, TK4, Task<TV>> func)
        {
            return Cached(func, BuildCacheName<TK1, TK2, TK3, TK4, TV>(false));
        }
        
        public static MultiParamFunctionCacheConfigurationManager<TK1, TK2, TK3, TK4, TV> Cached<TK1, TK2, TK3, TK4, TV>(
            this Func<TK1, TK2, TK3, TK4, Task<TV>> func, string cacheName)
        {
            if (cacheName == null)
                throw new ArgumentNullException(nameof(cacheName));

            return new MultiParamFunctionCacheConfigurationManager<TK1, TK2, TK3, TK4, TV>(func, cacheName);
        }
        
        public static MultiParamFunctionCacheConfigurationManagerSync<TK1, TK2, TK3, TK4, TV> Cached<TK1, TK2, TK3, TK4, TV>(
            this Func<TK1, TK2, TK3, TK4, TV> func)
        {
            return Cached(func, BuildCacheName<TK1, TK2, TK3, TK4, TV>(false));
        }
        
        public static MultiParamFunctionCacheConfigurationManagerSync<TK1, TK2, TK3, TK4, TV> Cached<TK1, TK2, TK3, TK4, TV>(
            this Func<TK1, TK2, TK3, TK4, TV> func, string cacheName)
        {
            if (cacheName == null)
                throw new ArgumentNullException(nameof(cacheName));

            return new MultiParamFunctionCacheConfigurationManagerSync<TK1, TK2, TK3, TK4, TV>(func, cacheName);
        }
        
        public static MultiParamFunctionCacheConfigurationManager<TK1, TK2, TK3, TK4, TV> CachedAsync<TK1, TK2, TK3, TK4, TV>(
            this Func<TK1, TK2, TK3, TK4, TV> func)
        {
            return CachedAsync(func, BuildCacheName<TK1, TK2, TK3, TK4, TV>(false));
        }
        
        public static MultiParamFunctionCacheConfigurationManager<TK1, TK2, TK3, TK4, TV> CachedAsync<TK1, TK2, TK3, TK4, TV>(
            this Func<TK1, TK2, TK3, TK4, TV> func, string cacheName)
        {
            if (cacheName == null)
                throw new ArgumentNullException(nameof(cacheName));

            return new MultiParamFunctionCacheConfigurationManager<TK1, TK2, TK3, TK4, TV>(func.ConvertToAsync(), cacheName);
        }
        
        private static string BuildCacheName<TK, TV>(bool isEnumerableKey)
        {
            var prefix = GetPrefix(isEnumerableKey);
            
            return $"{prefix}_{typeof(TK).Name}->{typeof(TV).Name}";
        }

        private static string BuildCacheName<TK1, TK2, TV>(bool isEnumerableKey)
        {
            var prefix = GetPrefix(isEnumerableKey);
            
            return $"{prefix}_{typeof(TK1).Name}+{typeof(TK2).Name}->{typeof(TV).Name}";
        }
        
        private static string BuildCacheName<TK1, TK2, TK3, TV>(bool isEnumerableKey)
        {
            var prefix = GetPrefix(isEnumerableKey);
            
            return $"{prefix}_{typeof(TK1).Name}+{typeof(TK2).Name}+{typeof(TK3).Name}->{typeof(TV).Name}";
        }
        
        private static string BuildCacheName<TK1, TK2, TK3, TK4, TV>(bool isEnumerableKey)
        {
            var prefix = GetPrefix(isEnumerableKey);
            
            return $"{prefix}_{typeof(TK1).Name}+{typeof(TK2).Name}+{typeof(TK3).Name}+{typeof(TK4).Name}->{typeof(TV).Name}";
        }

        private static string GetPrefix(bool isEnumerableKey)
        {
            return isEnumerableKey
                ? "EnumerableKeyFunctionCache"
                : "FunctionCache";
        }
    }
}
