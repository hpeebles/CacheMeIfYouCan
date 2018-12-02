using System;
using System.Reflection;
using System.Threading.Tasks;
using CacheMeIfYouCan.Internal;

namespace CacheMeIfYouCan.Configuration
{
    public class FunctionCacheConfigurationManager<TK, TV> : FunctionCacheConfigurationManagerBase<FunctionCacheConfigurationManager<TK, TV>, TK, TV>
    {
        private Func<TK, Task<TV>> _cachedFunc;

        internal FunctionCacheConfigurationManager(
            Func<TK, Task<TV>> inputFunc,
            string functionName)
            : base(inputFunc, functionName)
        { }

        internal FunctionCacheConfigurationManager(
            Func<TK, Task<TV>> inputFunc,
            CachedProxyConfig interfaceConfig,
            MethodInfo methodInfo)
            : base(
                inputFunc,
                $"{interfaceConfig.InterfaceType.Name}.{methodInfo.Name}",
                interfaceConfig,
                new CachedProxyFunctionInfo(interfaceConfig.InterfaceType, methodInfo, typeof(TK), typeof(TV)))
        { }

        public new FunctionCacheConfigurationManager<TK, TV> WithTimeToLiveFactory(Func<TK, TV, TimeSpan> timeToLiveFactory)
        {
            return base.WithTimeToLiveFactory(timeToLiveFactory);
        }
        
        public Func<TK, Task<TV>> Build()
        {
            var functionCache = BuildFunctionCacheSingle();

            _cachedFunc = functionCache.Get;
            
            return _cachedFunc;
        }
        
        public static implicit operator Func<TK, Task<TV>>(FunctionCacheConfigurationManager<TK, TV> cacheConfig)
        {
            return cacheConfig.Build();
        }
    }
}