﻿using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using CacheMeIfYouCan.Internal;

namespace CacheMeIfYouCan.Configuration
{
    public abstract class SingleKeyFunctionCacheConfigurationManagerSync<TConfig, TK, TV>
        : SingleKeyFunctionCacheConfigurationManagerBase<TConfig, TK, TV>
        where TConfig : SingleKeyFunctionCacheConfigurationManagerSync<TConfig, TK, TV>
    {
        internal SingleKeyFunctionCacheConfigurationManagerSync(Func<TK, CancellationToken, TV> inputFunc)
            : base(inputFunc.ConvertToAsync(), $"FunctionCache_{typeof(TK).Name}->{typeof(TV).Name}")
        { }

        internal SingleKeyFunctionCacheConfigurationManagerSync(
            Func<TK, CancellationToken, TV> inputFunc,
            CachedProxyConfig interfaceConfig,
            MethodInfo methodInfo)
            : base(
                inputFunc.ConvertToAsync(),
                interfaceConfig,
                methodInfo)
        { }
    }
    
    public sealed class SingleKeyFunctionCacheConfigurationManagerSyncNoCanx<TK, TV>
        : SingleKeyFunctionCacheConfigurationManagerSync<SingleKeyFunctionCacheConfigurationManagerSyncNoCanx<TK, TV>, TK, TV>
    {
        internal SingleKeyFunctionCacheConfigurationManagerSyncNoCanx(Func<TK, TV> inputFunc)
            : base((k, t) => inputFunc(k))
        { }

        internal SingleKeyFunctionCacheConfigurationManagerSyncNoCanx(
            Func<TK, TV> inputFunc,
            CachedProxyConfig interfaceConfig,
            MethodInfo methodInfo)
            : base(
                (k, t) => inputFunc(k),
                interfaceConfig,
                methodInfo)
        { }
        
        public Func<TK, TV> Build()
        {
            var functionCache = BuildFunctionCacheSingle();

            Func<TK, CancellationToken, Task<TV>> func = functionCache.Get;
            
            return func.ConvertToSync().MakeNonCancellable();
        }
    }
    
    public sealed class SingleKeyFunctionCacheConfigurationManagerSyncCanx<TK, TV>
        : SingleKeyFunctionCacheConfigurationManagerSync<SingleKeyFunctionCacheConfigurationManagerSyncCanx<TK, TV>, TK, TV>
    {
        internal SingleKeyFunctionCacheConfigurationManagerSyncCanx(Func<TK, CancellationToken, TV> inputFunc)
            : base(inputFunc)
        { }

        internal SingleKeyFunctionCacheConfigurationManagerSyncCanx(
            Func<TK, CancellationToken, TV> inputFunc,
            CachedProxyConfig interfaceConfig,
            MethodInfo methodInfo)
            : base(
                inputFunc,
                interfaceConfig,
                methodInfo)
        { }
        
        public Func<TK, CancellationToken, TV> Build()
        {
            var functionCache = BuildFunctionCacheSingle();
            
            Func<TK, CancellationToken, Task<TV>> func = functionCache.Get;
            
            return func.ConvertToSync();
        }
    }
}