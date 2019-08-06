using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using CacheMeIfYouCan.Internal;

namespace CacheMeIfYouCan.Configuration
{
    public abstract class SingleKeyFunctionCacheConfigurationManager<TConfig, TK, TV>
        : SingleKeyFunctionCacheConfigurationManagerBase<TConfig, TK, TV>
        where TConfig : SingleKeyFunctionCacheConfigurationManager<TConfig, TK, TV>
    {
        internal SingleKeyFunctionCacheConfigurationManager(Func<TK, CancellationToken, Task<TV>> inputFunc)
            : base(inputFunc, $"FunctionCache_{typeof(TK).Name}->{typeof(TV).Name}")
        { }

        internal SingleKeyFunctionCacheConfigurationManager(
            Func<TK, CancellationToken, Task<TV>> inputFunc,
            CachedProxyConfig interfaceConfig,
            MethodInfo methodInfo)
            : base(
                inputFunc,
                interfaceConfig,
                methodInfo)
        { }
        
        public new TConfig SkipCacheWhen(Func<TK, bool> predicate, SkipCacheSettings settings = SkipCacheSettings.SkipGetAndSet)
        {
            return base.SkipCacheWhen(predicate, settings);
        }
    }
    
    public sealed class SingleKeyFunctionCacheConfigurationManagerNoCanx<TK, TV>
        : SingleKeyFunctionCacheConfigurationManager<SingleKeyFunctionCacheConfigurationManagerNoCanx<TK, TV>, TK, TV>
    {
        internal SingleKeyFunctionCacheConfigurationManagerNoCanx(Func<TK, Task<TV>> inputFunc)
            : base(inputFunc.AppearCancellable())
        { }

        internal SingleKeyFunctionCacheConfigurationManagerNoCanx(
            Func<TK, Task<TV>> inputFunc,
            CachedProxyConfig interfaceConfig,
            MethodInfo methodInfo)
            : base(
                inputFunc.AppearCancellable(),
                interfaceConfig,
                methodInfo)
        { }
        
        public Func<TK, Task<TV>> Build()
        {
            var functionCache = BuildFunctionCacheSingle();

            Func<TK, CancellationToken, Task<TV>> func = functionCache.Get;

            return func.MakeNonCancellable();
        }
    }
    
    public sealed class SingleKeyFunctionCacheConfigurationManagerCanx<TK, TV>
        : SingleKeyFunctionCacheConfigurationManager<SingleKeyFunctionCacheConfigurationManagerCanx<TK, TV>, TK, TV>
    {
        internal SingleKeyFunctionCacheConfigurationManagerCanx(Func<TK, CancellationToken, Task<TV>> inputFunc)
            : base(inputFunc)
        { }

        internal SingleKeyFunctionCacheConfigurationManagerCanx(
            Func<TK, CancellationToken, Task<TV>> inputFunc,
            CachedProxyConfig interfaceConfig,
            MethodInfo methodInfo)
            : base(
                inputFunc,
                interfaceConfig,
                methodInfo)
        { }
        
        public Func<TK, CancellationToken, Task<TV>> Build()
        {
            var functionCache = BuildFunctionCacheSingle();

            return functionCache.Get;
        }
    }
}