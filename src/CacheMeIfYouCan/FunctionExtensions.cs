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
            return new SingleKeyFunctionCacheConfigurationManager<TK, TV>(func);
        }
        
        public static SingleKeyFunctionCacheConfigurationManagerSync<TK, TV> Cached<TK, TV>(
            this Func<TK, TV> func)
        {
            return new SingleKeyFunctionCacheConfigurationManagerSync<TK, TV>(func);
        }
        
        public static SingleKeyFunctionCacheConfigurationManager<TK, TV> CachedAsync<TK, TV>(
            this Func<TK, TV> func)
        {
            return new SingleKeyFunctionCacheConfigurationManager<TK, TV>(func.ConvertToAsync());
        }
        
        public static EnumerableKeyFunctionCacheConfigurationManager<TReq, TRes, TK, TV> Cached<TReq, TRes, TK, TV>(
            this Func<TReq, Task<TRes>> func)
            where TReq : IEnumerable<TK>
            where TRes : IDictionary<TK, TV>
        {
            return new EnumerableKeyFunctionCacheConfigurationManager<TReq, TRes, TK, TV>(func);
        }
        
        public static EnumerableKeyFunctionCacheConfigurationManagerSync<TReq, TRes, TK, TV> Cached<TReq, TRes, TK, TV>(
            this Func<TReq, TRes> func)
            where TReq : IEnumerable<TK>
            where TRes : IDictionary<TK, TV>
        {
            return new EnumerableKeyFunctionCacheConfigurationManagerSync<TReq, TRes, TK, TV>(func);
        }
        
        public static EnumerableKeyFunctionCacheConfigurationManager<TReq, TRes, TK, TV> CachedAsync<TReq, TRes, TK, TV>(
            this Func<TReq, TRes> func)
            where TReq : IEnumerable<TK>
            where TRes : IDictionary<TK, TV>
        {
            return new EnumerableKeyFunctionCacheConfigurationManager<TReq, TRes, TK, TV>(func.ConvertToAsync());
        }
        
        public static MultiParamFunctionCacheConfigurationManager<TK1, TK2, TV> Cached<TK1, TK2, TV>(
            this Func<TK1, TK2, Task<TV>> func)
        {
            return new MultiParamFunctionCacheConfigurationManager<TK1, TK2, TV>(func);
        }
        
        public static MultiParamFunctionCacheConfigurationManagerSync<TK1, TK2, TV> Cached<TK1, TK2, TV>(
            this Func<TK1, TK2, TV> func)
        {
            return new MultiParamFunctionCacheConfigurationManagerSync<TK1, TK2, TV>(func);
        }
        
        public static MultiParamFunctionCacheConfigurationManager<TK1, TK2, TV> CachedAsync<TK1, TK2, TV>(
            this Func<TK1, TK2, TV> func)
        {
            return new MultiParamFunctionCacheConfigurationManager<TK1, TK2, TV>(func.ConvertToAsync());
        }
        
        public static MultiParamFunctionCacheConfigurationManager<TK1, TK2, TK3, TV> Cached<TK1, TK2, TK3, TV>(
            this Func<TK1, TK2, TK3, Task<TV>> func)
        {
            return new MultiParamFunctionCacheConfigurationManager<TK1, TK2, TK3, TV>(func);
        }
        
        public static MultiParamFunctionCacheConfigurationManagerSync<TK1, TK2, TK3, TV> Cached<TK1, TK2, TK3, TV>(
            this Func<TK1, TK2, TK3, TV> func)
        {
            return new MultiParamFunctionCacheConfigurationManagerSync<TK1, TK2, TK3, TV>(func);
        }
        
        public static MultiParamFunctionCacheConfigurationManager<TK1, TK2, TK3, TV> CachedAsync<TK1, TK2, TK3, TV>(
            this Func<TK1, TK2, TK3, TV> func)
        {
            return new MultiParamFunctionCacheConfigurationManager<TK1, TK2, TK3, TV>(func.ConvertToAsync());
        }
        
        public static MultiParamFunctionCacheConfigurationManager<TK1, TK2, TK3, TK4, TV> Cached<TK1, TK2, TK3, TK4, TV>(
            this Func<TK1, TK2, TK3, TK4, Task<TV>> func)
        {
            return new MultiParamFunctionCacheConfigurationManager<TK1, TK2, TK3, TK4, TV>(func);
        }
        
        public static MultiParamFunctionCacheConfigurationManagerSync<TK1, TK2, TK3, TK4, TV> Cached<TK1, TK2, TK3, TK4, TV>(
            this Func<TK1, TK2, TK3, TK4, TV> func)
        {
            return new MultiParamFunctionCacheConfigurationManagerSync<TK1, TK2, TK3, TK4, TV>(func);
        }
        
        public static MultiParamFunctionCacheConfigurationManager<TK1, TK2, TK3, TK4, TV> CachedAsync<TK1, TK2, TK3, TK4, TV>(
            this Func<TK1, TK2, TK3, TK4, TV> func)
        {
            return new MultiParamFunctionCacheConfigurationManager<TK1, TK2, TK3, TK4, TV>(func.ConvertToAsync());
        }

        public static MultiParamEnumerableKeyFunctionCacheConfigurationManager<TK1, TK2Enum, TRes, TK2, TV> Cached<TK1, TK2Enum, TRes, TK2, TV>(
            this Func<TK1, TK2Enum, Task<TRes>> func)
            where TK2Enum : IEnumerable<TK2>
            where TRes : IDictionary<TK2, TV>
        {
            return new MultiParamEnumerableKeyFunctionCacheConfigurationManager<TK1, TK2Enum, TRes, TK2, TV>(func);
        }
        
        public static MultiParamEnumerableKeyFunctionCacheConfigurationManagerSync<TK1, TK2Enum, TRes, TK2, TV> Cached<TK1, TK2Enum, TRes, TK2, TV>(
            this Func<TK1, TK2Enum, TRes> func)
            where TK2Enum : IEnumerable<TK2>
            where TRes : IDictionary<TK2, TV>
        {
            return new MultiParamEnumerableKeyFunctionCacheConfigurationManagerSync<TK1, TK2Enum, TRes, TK2, TV>(func);
        }
        
        public static MultiParamEnumerableKeyFunctionCacheConfigurationManager<TK1, TK2Enum, TRes, TK2, TV> CachedAsync<TK1, TK2Enum, TRes, TK2, TV>(
            this Func<TK1, TK2Enum, TRes> func)
            where TK2Enum : IEnumerable<TK2>
            where TRes : IDictionary<TK2, TV>
        {
            return new MultiParamEnumerableKeyFunctionCacheConfigurationManager<TK1, TK2Enum, TRes, TK2, TV>(
                func.ConvertToAsync());
        }
        
        public static MultiParamEnumerableKeyFunctionCacheConfigurationManager<TK1, TK2, TK3Enum, TRes, TK3, TV> Cached<TK1, TK2, TK3Enum, TRes, TK3, TV>(
            this Func<TK1, TK2, TK3Enum, Task<TRes>> func)
            where TK3Enum : IEnumerable<TK3>
            where TRes : IDictionary<TK3, TV>
        {
            return new MultiParamEnumerableKeyFunctionCacheConfigurationManager<TK1, TK2, TK3Enum, TRes, TK3, TV>(func);
        }
        
        public static MultiParamEnumerableKeyFunctionCacheConfigurationManagerSync<TK1, TK2, TK3Enum, TRes, TK3, TV> Cached<TK1, TK2, TK3Enum, TRes, TK3, TV>(
            this Func<TK1, TK2, TK3Enum, TRes> func)
            where TK3Enum : IEnumerable<TK3>
            where TRes : IDictionary<TK3, TV>
        {
            return new MultiParamEnumerableKeyFunctionCacheConfigurationManagerSync<TK1, TK2, TK3Enum, TRes, TK3, TV>(func);
        }
        
        public static MultiParamEnumerableKeyFunctionCacheConfigurationManager<TK1, TK2, TK3Enum, TRes, TK3, TV> CachedAsync<TK1, TK2, TK3Enum, TRes, TK3, TV>(
            this Func<TK1, TK2, TK3Enum, TRes> func)
            where TK3Enum : IEnumerable<TK3>
            where TRes : IDictionary<TK3, TV>
        {
            return new MultiParamEnumerableKeyFunctionCacheConfigurationManager<TK1, TK2, TK3Enum, TRes, TK3, TV>(
                func.ConvertToAsync());
        }
        
        public static MultiParamEnumerableKeyFunctionCacheConfigurationManager<TK1, TK2, TK3, TK4Enum, TRes, TK4, TV> Cached<TK1, TK2, TK3, TK4Enum, TRes, TK4, TV>(
            this Func<TK1, TK2, TK3, TK4Enum, Task<TRes>> func)
            where TK4Enum : IEnumerable<TK4>
            where TRes : IDictionary<TK4, TV>
        {
            return new MultiParamEnumerableKeyFunctionCacheConfigurationManager<TK1, TK2, TK3, TK4Enum, TRes, TK4, TV>(func);
        }
        
        public static MultiParamEnumerableKeyFunctionCacheConfigurationManagerSync<TK1, TK2, TK3, TK4Enum, TRes, TK4, TV> Cached<TK1, TK2, TK3, TK4Enum, TRes, TK4, TV>(
            this Func<TK1, TK2, TK3, TK4Enum, TRes> func)
            where TK4Enum : IEnumerable<TK4>
            where TRes : IDictionary<TK4, TV>
        {
            return new MultiParamEnumerableKeyFunctionCacheConfigurationManagerSync<TK1, TK2, TK3, TK4Enum, TRes, TK4, TV>(func);
        }
        
        public static MultiParamEnumerableKeyFunctionCacheConfigurationManager<TK1, TK2, TK3, TK4Enum, TRes, TK4, TV> CachedAsync<TK1, TK2, TK3, TK4Enum, TRes, TK4, TV>(
            this Func<TK1, TK2, TK3, TK4Enum, TRes> func)
            where TK4Enum : IEnumerable<TK4>
            where TRes : IDictionary<TK4, TV>
        {
            return new MultiParamEnumerableKeyFunctionCacheConfigurationManager<TK1, TK2, TK3, TK4Enum, TRes, TK4, TV>(
                func.ConvertToAsync());
        }
        
        private static string BuildCacheName<TK, TV>(FunctionCacheType type)
        {
            var prefix = GetPrefix(type);
            
            return $"{prefix}_{typeof(TK).Name}->{typeof(TV).Name}";
        }

        private static string BuildCacheName<TK1, TK2, TV>(FunctionCacheType type)
        {
            var prefix = GetPrefix(type);
            
            return $"{prefix}_{typeof(TK1).Name}+{typeof(TK2).Name}->{typeof(TV).Name}";
        }
        
        private static string BuildCacheName<TK1, TK2, TK3, TV>(FunctionCacheType type)
        {
            var prefix = GetPrefix(type);
            
            return $"{prefix}_{typeof(TK1).Name}+{typeof(TK2).Name}+{typeof(TK3).Name}->{typeof(TV).Name}";
        }
        
        private static string BuildCacheName<TK1, TK2, TK3, TK4, TV>(FunctionCacheType type)
        {
            var prefix = GetPrefix(type);
            
            return $"{prefix}_{typeof(TK1).Name}+{typeof(TK2).Name}+{typeof(TK3).Name}+{typeof(TK4).Name}->{typeof(TV).Name}";
        }

        private static string GetPrefix(FunctionCacheType type)
        {
            return type + "FunctionCache";
        }

        private enum FunctionCacheType
        {
            SingleKey,
            EnumerableKey,
            MultiParam,
            MultiParamEnumerableKey
        }
    }
}
