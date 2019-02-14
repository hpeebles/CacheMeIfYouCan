using System;
using System.Reflection;
using System.Threading.Tasks;
using CacheMeIfYouCan.Internal;

namespace CacheMeIfYouCan.Configuration
{
    public sealed class SingleKeyFunctionCacheConfigurationManagerSync<TK, TV>
        : SingleKeyFunctionCacheConfigurationManagerBase<SingleKeyFunctionCacheConfigurationManagerSync<TK, TV>, TK, TV>
    {
        internal SingleKeyFunctionCacheConfigurationManagerSync(Func<TK, TV> inputFunc)
            : base(
                inputFunc.ConvertToAsync(),
                $"FunctionCache_{typeof(TK).Name}->{typeof(TV).Name}")
        { }

        internal SingleKeyFunctionCacheConfigurationManagerSync(
            Func<TK, TV> inputFunc,
            CachedProxyConfig interfaceConfig,
            MethodInfo methodInfo)
            : base(
                inputFunc.ConvertToAsync(),
                interfaceConfig,
                methodInfo)
        { }
        
        public Func<TK, TV> Build()
        {
            var functionCache = BuildFunctionCacheSingle();

            Func<TK, Task<TV>> func = functionCache.Get;
            
            return func.ConvertToSync();
        }
    }
}