using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CacheMeIfYouCan.Configuration;
using CacheMeIfYouCan.Internal;

namespace CacheMeIfYouCan
{
    public static class FunctionExtensions
    {
        public static SingleKeyFunctionCacheConfigurationManagerNoCanx<TK, TV> Cached<TK, TV>(
            this Func<TK, Task<TV>> func)
        {
            return new SingleKeyFunctionCacheConfigurationManagerNoCanx<TK, TV>(func);
        }
        
        public static SingleKeyFunctionCacheConfigurationManagerSyncNoCanx<TK, TV> Cached<TK, TV>(
            this Func<TK, TV> func)
        {
            return new SingleKeyFunctionCacheConfigurationManagerSyncNoCanx<TK, TV>(func);
        }
        
        public static SingleKeyFunctionCacheConfigurationManagerNoCanx<TK, TV> CachedAsync<TK, TV>(
            this Func<TK, TV> func)
        {
            return new SingleKeyFunctionCacheConfigurationManagerNoCanx<TK, TV>(func.ConvertToAsync());
        }
        
        public static SingleKeyFunctionCacheConfigurationManagerCanx<TK, TV> Cached<TK, TV>(
            this Func<TK, CancellationToken, Task<TV>> func)
        {
            return new SingleKeyFunctionCacheConfigurationManagerCanx<TK, TV>(func);
        }
        
        public static SingleKeyFunctionCacheConfigurationManagerSyncCanx<TK, TV> Cached<TK, TV>(
            this Func<TK, CancellationToken, TV> func)
        {
            return new SingleKeyFunctionCacheConfigurationManagerSyncCanx<TK, TV>(func);
        }
        
        public static SingleKeyFunctionCacheConfigurationManagerCanx<TK, TV> CachedAsync<TK, TV>(
            this Func<TK, CancellationToken, TV> func)
        {
            return new SingleKeyFunctionCacheConfigurationManagerCanx<TK, TV>(func.ConvertToAsync());
        }
        
        public static EnumerableKeyFunctionCacheConfigurationManagerNoCanx<TReq, TRes, TK, TV> Cached<TReq, TRes, TK, TV>(
            this Func<TReq, Task<TRes>> func)
            where TReq : IEnumerable<TK>
            where TRes : IDictionary<TK, TV>
        {
            return new EnumerableKeyFunctionCacheConfigurationManagerNoCanx<TReq, TRes, TK, TV>(func);
        }
        
        public static EnumerableKeyFunctionCacheConfigurationManagerSyncNoCanx<TReq, TRes, TK, TV> Cached<TReq, TRes, TK, TV>(
            this Func<TReq, TRes> func)
            where TReq : IEnumerable<TK>
            where TRes : IDictionary<TK, TV>
        {
            return new EnumerableKeyFunctionCacheConfigurationManagerSyncNoCanx<TReq, TRes, TK, TV>(func);
        }
        
        public static EnumerableKeyFunctionCacheConfigurationManagerNoCanx<TReq, TRes, TK, TV> CachedAsync<TReq, TRes, TK, TV>(
            this Func<TReq, TRes> func)
            where TReq : IEnumerable<TK>
            where TRes : IDictionary<TK, TV>
        {
            return new EnumerableKeyFunctionCacheConfigurationManagerNoCanx<TReq, TRes, TK, TV>(func.ConvertToAsync());
        }
        
        public static EnumerableKeyFunctionCacheConfigurationManagerCanx<TReq, TRes, TK, TV> Cached<TReq, TRes, TK, TV>(
            this Func<TReq, CancellationToken, Task<TRes>> func)
            where TReq : IEnumerable<TK>
            where TRes : IDictionary<TK, TV>
        {
            return new EnumerableKeyFunctionCacheConfigurationManagerCanx<TReq, TRes, TK, TV>(func);
        }
        
        public static EnumerableKeyFunctionCacheConfigurationManagerSyncCanx<TReq, TRes, TK, TV> Cached<TReq, TRes, TK, TV>(
            this Func<TReq, CancellationToken, TRes> func)
            where TReq : IEnumerable<TK>
            where TRes : IDictionary<TK, TV>
        {
            return new EnumerableKeyFunctionCacheConfigurationManagerSyncCanx<TReq, TRes, TK, TV>(func);
        }
        
        public static EnumerableKeyFunctionCacheConfigurationManagerCanx<TReq, TRes, TK, TV> CachedAsync<TReq, TRes, TK, TV>(
            this Func<TReq, CancellationToken, TRes> func)
            where TReq : IEnumerable<TK>
            where TRes : IDictionary<TK, TV>
        {
            return new EnumerableKeyFunctionCacheConfigurationManagerCanx<TReq, TRes, TK, TV>(func.ConvertToAsync());
        }
        
        public static MultiParamFunctionCacheConfigurationManagerNoCanx<TK1, TK2, TV> Cached<TK1, TK2, TV>(
            this Func<TK1, TK2, Task<TV>> func)
        {
            return new MultiParamFunctionCacheConfigurationManagerNoCanx<TK1, TK2, TV>(func);
        }
        
        public static MultiParamFunctionCacheConfigurationManagerSyncNoCanx<TK1, TK2, TV> Cached<TK1, TK2, TV>(
            this Func<TK1, TK2, TV> func)
        {
            return new MultiParamFunctionCacheConfigurationManagerSyncNoCanx<TK1, TK2, TV>(func);
        }
        
        public static MultiParamFunctionCacheConfigurationManagerNoCanx<TK1, TK2, TV> CachedAsync<TK1, TK2, TV>(
            this Func<TK1, TK2, TV> func)
        {
            return new MultiParamFunctionCacheConfigurationManagerNoCanx<TK1, TK2, TV>(func.ConvertToAsync());
        }
        
        public static MultiParamFunctionCacheConfigurationManagerNoCanx<TK1, TK2, TK3, TV> Cached<TK1, TK2, TK3, TV>(
            this Func<TK1, TK2, TK3, Task<TV>> func)
        {
            return new MultiParamFunctionCacheConfigurationManagerNoCanx<TK1, TK2, TK3, TV>(func);
        }
        
        public static MultiParamFunctionCacheConfigurationManagerSyncNoCanx<TK1, TK2, TK3, TV> Cached<TK1, TK2, TK3, TV>(
            this Func<TK1, TK2, TK3, TV> func)
        {
            return new MultiParamFunctionCacheConfigurationManagerSyncNoCanx<TK1, TK2, TK3, TV>(func);
        }
        
        public static MultiParamFunctionCacheConfigurationManagerNoCanx<TK1, TK2, TK3, TV> CachedAsync<TK1, TK2, TK3, TV>(
            this Func<TK1, TK2, TK3, TV> func)
        {
            return new MultiParamFunctionCacheConfigurationManagerNoCanx<TK1, TK2, TK3, TV>(func.ConvertToAsync());
        }
        
        public static MultiParamFunctionCacheConfigurationManagerNoCanx<TK1, TK2, TK3, TK4, TV> Cached<TK1, TK2, TK3, TK4, TV>(
            this Func<TK1, TK2, TK3, TK4, Task<TV>> func)
        {
            return new MultiParamFunctionCacheConfigurationManagerNoCanx<TK1, TK2, TK3, TK4, TV>(func);
        }
        
        public static MultiParamFunctionCacheConfigurationManagerSyncNoCanx<TK1, TK2, TK3, TK4, TV> Cached<TK1, TK2, TK3, TK4, TV>(
            this Func<TK1, TK2, TK3, TK4, TV> func)
        {
            return new MultiParamFunctionCacheConfigurationManagerSyncNoCanx<TK1, TK2, TK3, TK4, TV>(func);
        }
        
        public static MultiParamFunctionCacheConfigurationManagerNoCanx<TK1, TK2, TK3, TK4, TV> CachedAsync<TK1, TK2, TK3, TK4, TV>(
            this Func<TK1, TK2, TK3, TK4, TV> func)
        {
            return new MultiParamFunctionCacheConfigurationManagerNoCanx<TK1, TK2, TK3, TK4, TV>(func.ConvertToAsync());
        }
        
        public static MultiParamFunctionCacheConfigurationManagerCanx<TK1, TK2, TV> Cached<TK1, TK2, TV>(
            this Func<TK1, TK2, CancellationToken, Task<TV>> func)
        {
            return new MultiParamFunctionCacheConfigurationManagerCanx<TK1, TK2, TV>(func);
        }
        
        public static MultiParamFunctionCacheConfigurationManagerSyncCanx<TK1, TK2, TV> Cached<TK1, TK2, TV>(
            this Func<TK1, TK2, CancellationToken, TV> func)
        {
            return new MultiParamFunctionCacheConfigurationManagerSyncCanx<TK1, TK2, TV>(func);
        }
        
        public static MultiParamFunctionCacheConfigurationManagerCanx<TK1, TK2, TV> CachedAsync<TK1, TK2, TV>(
            this Func<TK1, TK2, CancellationToken, TV> func)
        {
            return new MultiParamFunctionCacheConfigurationManagerCanx<TK1, TK2, TV>(func.ConvertToAsync());
        }
        
        public static MultiParamFunctionCacheConfigurationManagerCanx<TK1, TK2, TK3, TV> Cached<TK1, TK2, TK3, TV>(
            this Func<TK1, TK2, TK3, CancellationToken, Task<TV>> func)
        {
            return new MultiParamFunctionCacheConfigurationManagerCanx<TK1, TK2, TK3, TV>(func);
        }
        
        public static MultiParamFunctionCacheConfigurationManagerSyncCanx<TK1, TK2, TK3, TV> Cached<TK1, TK2, TK3, TV>(
            this Func<TK1, TK2, TK3, CancellationToken, TV> func)
        {
            return new MultiParamFunctionCacheConfigurationManagerSyncCanx<TK1, TK2, TK3, TV>(func);
        }
        
        public static MultiParamFunctionCacheConfigurationManagerCanx<TK1, TK2, TK3, TV> CachedAsync<TK1, TK2, TK3, TV>(
            this Func<TK1, TK2, TK3, CancellationToken, TV> func)
        {
            return new MultiParamFunctionCacheConfigurationManagerCanx<TK1, TK2, TK3, TV>(func.ConvertToAsync());
        }
        
        public static MultiParamFunctionCacheConfigurationManagerCanx<TK1, TK2, TK3, TK4, TV> Cached<TK1, TK2, TK3, TK4, TV>(
            this Func<TK1, TK2, TK3, TK4, CancellationToken, Task<TV>> func)
        {
            return new MultiParamFunctionCacheConfigurationManagerCanx<TK1, TK2, TK3, TK4, TV>(func);
        }
        
        public static MultiParamFunctionCacheConfigurationManagerSyncCanx<TK1, TK2, TK3, TK4, TV> Cached<TK1, TK2, TK3, TK4, TV>(
            this Func<TK1, TK2, TK3, TK4, CancellationToken, TV> func)
        {
            return new MultiParamFunctionCacheConfigurationManagerSyncCanx<TK1, TK2, TK3, TK4, TV>(func);
        }
        
        public static MultiParamFunctionCacheConfigurationManagerCanx<TK1, TK2, TK3, TK4, TV> CachedAsync<TK1, TK2, TK3, TK4, TV>(
            this Func<TK1, TK2, TK3, TK4, CancellationToken, TV> func)
        {
            return new MultiParamFunctionCacheConfigurationManagerCanx<TK1, TK2, TK3, TK4, TV>(func.ConvertToAsync());
        }

        public static MultiParamEnumerableKeyFunctionCacheConfigurationManagerNoCanx<TK1, TK2Enum, TRes, TK2, TV> Cached<TK1, TK2Enum, TRes, TK2, TV>(
            this Func<TK1, TK2Enum, Task<TRes>> func)
            where TK2Enum : IEnumerable<TK2>
            where TRes : IDictionary<TK2, TV>
        {
            return new MultiParamEnumerableKeyFunctionCacheConfigurationManagerNoCanx<TK1, TK2Enum, TRes, TK2, TV>(func);
        }
        
        public static MultiParamEnumerableKeyFunctionCacheConfigurationManagerSyncNoCanx<TK1, TK2Enum, TRes, TK2, TV> Cached<TK1, TK2Enum, TRes, TK2, TV>(
            this Func<TK1, TK2Enum, TRes> func)
            where TK2Enum : IEnumerable<TK2>
            where TRes : IDictionary<TK2, TV>
        {
            return new MultiParamEnumerableKeyFunctionCacheConfigurationManagerSyncNoCanx<TK1, TK2Enum, TRes, TK2, TV>(func);
        }
        
        public static MultiParamEnumerableKeyFunctionCacheConfigurationManagerNoCanx<TK1, TK2Enum, TRes, TK2, TV> CachedAsync<TK1, TK2Enum, TRes, TK2, TV>(
            this Func<TK1, TK2Enum, TRes> func)
            where TK2Enum : IEnumerable<TK2>
            where TRes : IDictionary<TK2, TV>
        {
            return new MultiParamEnumerableKeyFunctionCacheConfigurationManagerNoCanx<TK1, TK2Enum, TRes, TK2, TV>(
                func.ConvertToAsync());
        }
        
        public static MultiParamEnumerableKeyFunctionCacheConfigurationManagerNoCanx<TK1, TK2, TK3Enum, TRes, TK3, TV> Cached<TK1, TK2, TK3Enum, TRes, TK3, TV>(
            this Func<TK1, TK2, TK3Enum, Task<TRes>> func)
            where TK3Enum : IEnumerable<TK3>
            where TRes : IDictionary<TK3, TV>
        {
            return new MultiParamEnumerableKeyFunctionCacheConfigurationManagerNoCanx<TK1, TK2, TK3Enum, TRes, TK3, TV>(func);
        }
        
        public static MultiParamEnumerableKeyFunctionCacheConfigurationManagerSyncNoCanx<TK1, TK2, TK3Enum, TRes, TK3, TV> Cached<TK1, TK2, TK3Enum, TRes, TK3, TV>(
            this Func<TK1, TK2, TK3Enum, TRes> func)
            where TK3Enum : IEnumerable<TK3>
            where TRes : IDictionary<TK3, TV>
        {
            return new MultiParamEnumerableKeyFunctionCacheConfigurationManagerSyncNoCanx<TK1, TK2, TK3Enum, TRes, TK3, TV>(func);
        }
        
        public static MultiParamEnumerableKeyFunctionCacheConfigurationManagerNoCanx<TK1, TK2, TK3Enum, TRes, TK3, TV> CachedAsync<TK1, TK2, TK3Enum, TRes, TK3, TV>(
            this Func<TK1, TK2, TK3Enum, TRes> func)
            where TK3Enum : IEnumerable<TK3>
            where TRes : IDictionary<TK3, TV>
        {
            return new MultiParamEnumerableKeyFunctionCacheConfigurationManagerNoCanx<TK1, TK2, TK3Enum, TRes, TK3, TV>(
                func.ConvertToAsync());
        }
        
        public static MultiParamEnumerableKeyFunctionCacheConfigurationManagerNoCanx<TK1, TK2, TK3, TK4Enum, TRes, TK4, TV> Cached<TK1, TK2, TK3, TK4Enum, TRes, TK4, TV>(
            this Func<TK1, TK2, TK3, TK4Enum, Task<TRes>> func)
            where TK4Enum : IEnumerable<TK4>
            where TRes : IDictionary<TK4, TV>
        {
            return new MultiParamEnumerableKeyFunctionCacheConfigurationManagerNoCanx<TK1, TK2, TK3, TK4Enum, TRes, TK4, TV>(func);
        }
        
        public static MultiParamEnumerableKeyFunctionCacheConfigurationManagerSyncNoCanx<TK1, TK2, TK3, TK4Enum, TRes, TK4, TV> Cached<TK1, TK2, TK3, TK4Enum, TRes, TK4, TV>(
            this Func<TK1, TK2, TK3, TK4Enum, TRes> func)
            where TK4Enum : IEnumerable<TK4>
            where TRes : IDictionary<TK4, TV>
        {
            return new MultiParamEnumerableKeyFunctionCacheConfigurationManagerSyncNoCanx<TK1, TK2, TK3, TK4Enum, TRes, TK4, TV>(func);
        }
        
        public static MultiParamEnumerableKeyFunctionCacheConfigurationManagerNoCanx<TK1, TK2, TK3, TK4Enum, TRes, TK4, TV> CachedAsync<TK1, TK2, TK3, TK4Enum, TRes, TK4, TV>(
            this Func<TK1, TK2, TK3, TK4Enum, TRes> func)
            where TK4Enum : IEnumerable<TK4>
            where TRes : IDictionary<TK4, TV>
        {
            return new MultiParamEnumerableKeyFunctionCacheConfigurationManagerNoCanx<TK1, TK2, TK3, TK4Enum, TRes, TK4, TV>(
                func.ConvertToAsync());
        }

        public static MultiParamEnumerableKeyFunctionCacheConfigurationManagerCanx<TK1, TK2Enum, TRes, TK2, TV> Cached<TK1, TK2Enum, TRes, TK2, TV>(
            this Func<TK1, TK2Enum, CancellationToken, Task<TRes>> func)
            where TK2Enum : IEnumerable<TK2>
            where TRes : IDictionary<TK2, TV>
        {
            return new MultiParamEnumerableKeyFunctionCacheConfigurationManagerCanx<TK1, TK2Enum, TRes, TK2, TV>(func);
        }
        
        public static MultiParamEnumerableKeyFunctionCacheConfigurationManagerSyncCanx<TK1, TK2Enum, TRes, TK2, TV> Cached<TK1, TK2Enum, TRes, TK2, TV>(
            this Func<TK1, TK2Enum, CancellationToken, TRes> func)
            where TK2Enum : IEnumerable<TK2>
            where TRes : IDictionary<TK2, TV>
        {
            return new MultiParamEnumerableKeyFunctionCacheConfigurationManagerSyncCanx<TK1, TK2Enum, TRes, TK2, TV>(func);
        }
        
        public static MultiParamEnumerableKeyFunctionCacheConfigurationManagerCanx<TK1, TK2Enum, TRes, TK2, TV> CachedAsync<TK1, TK2Enum, TRes, TK2, TV>(
            this Func<TK1, TK2Enum, CancellationToken, TRes> func)
            where TK2Enum : IEnumerable<TK2>
            where TRes : IDictionary<TK2, TV>
        {
            return new MultiParamEnumerableKeyFunctionCacheConfigurationManagerCanx<TK1, TK2Enum, TRes, TK2, TV>(
                func.ConvertToAsync());
        }
        
        public static MultiParamEnumerableKeyFunctionCacheConfigurationManagerCanx<TK1, TK2, TK3Enum, TRes, TK3, TV> Cached<TK1, TK2, TK3Enum, TRes, TK3, TV>(
            this Func<TK1, TK2, TK3Enum, CancellationToken, Task<TRes>> func)
            where TK3Enum : IEnumerable<TK3>
            where TRes : IDictionary<TK3, TV>
        {
            return new MultiParamEnumerableKeyFunctionCacheConfigurationManagerCanx<TK1, TK2, TK3Enum, TRes, TK3, TV>(func);
        }
        
        public static MultiParamEnumerableKeyFunctionCacheConfigurationManagerSyncCanx<TK1, TK2, TK3Enum, TRes, TK3, TV> Cached<TK1, TK2, TK3Enum, TRes, TK3, TV>(
            this Func<TK1, TK2, TK3Enum, CancellationToken, TRes> func)
            where TK3Enum : IEnumerable<TK3>
            where TRes : IDictionary<TK3, TV>
        {
            return new MultiParamEnumerableKeyFunctionCacheConfigurationManagerSyncCanx<TK1, TK2, TK3Enum, TRes, TK3, TV>(func);
        }
        
        public static MultiParamEnumerableKeyFunctionCacheConfigurationManagerCanx<TK1, TK2, TK3Enum, TRes, TK3, TV> CachedAsync<TK1, TK2, TK3Enum, TRes, TK3, TV>(
            this Func<TK1, TK2, TK3Enum, CancellationToken, TRes> func)
            where TK3Enum : IEnumerable<TK3>
            where TRes : IDictionary<TK3, TV>
        {
            return new MultiParamEnumerableKeyFunctionCacheConfigurationManagerCanx<TK1, TK2, TK3Enum, TRes, TK3, TV>(
                func.ConvertToAsync());
        }
        
        public static MultiParamEnumerableKeyFunctionCacheConfigurationManagerCanx<TK1, TK2, TK3, TK4Enum, TRes, TK4, TV> Cached<TK1, TK2, TK3, TK4Enum, TRes, TK4, TV>(
            this Func<TK1, TK2, TK3, TK4Enum, CancellationToken, Task<TRes>> func)
            where TK4Enum : IEnumerable<TK4>
            where TRes : IDictionary<TK4, TV>
        {
            return new MultiParamEnumerableKeyFunctionCacheConfigurationManagerCanx<TK1, TK2, TK3, TK4Enum, TRes, TK4, TV>(func);
        }
        
        public static MultiParamEnumerableKeyFunctionCacheConfigurationManagerSyncCanx<TK1, TK2, TK3, TK4Enum, TRes, TK4, TV> Cached<TK1, TK2, TK3, TK4Enum, TRes, TK4, TV>(
            this Func<TK1, TK2, TK3, TK4Enum, CancellationToken, TRes> func)
            where TK4Enum : IEnumerable<TK4>
            where TRes : IDictionary<TK4, TV>
        {
            return new MultiParamEnumerableKeyFunctionCacheConfigurationManagerSyncCanx<TK1, TK2, TK3, TK4Enum, TRes, TK4, TV>(func);
        }
        
        public static MultiParamEnumerableKeyFunctionCacheConfigurationManagerCanx<TK1, TK2, TK3, TK4Enum, TRes, TK4, TV> CachedAsync<TK1, TK2, TK3, TK4Enum, TRes, TK4, TV>(
            this Func<TK1, TK2, TK3, TK4Enum, CancellationToken, TRes> func)
            where TK4Enum : IEnumerable<TK4>
            where TRes : IDictionary<TK4, TV>
        {
            return new MultiParamEnumerableKeyFunctionCacheConfigurationManagerCanx<TK1, TK2, TK3, TK4Enum, TRes, TK4, TV>(
                func.ConvertToAsync());
        }
    }
}
