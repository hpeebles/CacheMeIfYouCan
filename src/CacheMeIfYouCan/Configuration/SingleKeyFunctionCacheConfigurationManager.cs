using System;
using System.Reflection;
using System.Threading.Tasks;
using CacheMeIfYouCan.Internal;

namespace CacheMeIfYouCan.Configuration
{
    public sealed class SingleKeyFunctionCacheConfigurationManager<TK, TV>
        : SingleKeyFunctionCacheConfigurationManagerBase<SingleKeyFunctionCacheConfigurationManager<TK, TV>, TK, TV>
    {
        internal SingleKeyFunctionCacheConfigurationManager(
            Func<TK, Task<TV>> inputFunc,
            string functionName)
            : base(inputFunc, functionName)
        { }

        internal SingleKeyFunctionCacheConfigurationManager(
            Func<TK, Task<TV>> inputFunc,
            CachedProxyConfig interfaceConfig,
            MethodInfo methodInfo)
            : base(
                inputFunc,
                interfaceConfig,
                methodInfo)
        { }
        
        public Func<TK, Task<TV>> Build()
        {
            var functionCache = BuildFunctionCacheSingle();

            return functionCache.Get;
        }
    }
}