using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace CacheMeIfYouCan.Configuration
{
    public class MultiKeyFunctionCacheConfigurationManager<TK, TV> : FunctionCacheConfigurationManagerBase<MultiKeyFunctionCacheConfigurationManager<TK, TV>, TK, TV>
    {
        private Func<IEnumerable<TK>, Task<IDictionary<TK, TV>>> _cachedFunc;

        internal MultiKeyFunctionCacheConfigurationManager(
            Func<IEnumerable<TK>, Task<IDictionary<TK, TV>>> inputFunc,
            string functionName)
            : base(inputFunc, functionName, true)
        { }

        internal MultiKeyFunctionCacheConfigurationManager(
            Func<IEnumerable<TK>, Task<IDictionary<TK, TV>>> inputFunc,
            CachedProxyConfig interfaceConfig,
            MethodInfo methodInfo)
            : base(
                inputFunc,
                $"{interfaceConfig.InterfaceType.Name}.{methodInfo.Name}",
                true,
                interfaceConfig,
                new CachedProxyFunctionInfo(interfaceConfig.InterfaceType, methodInfo, typeof(TK), typeof(TV)))
        { }
        
        public Func<IEnumerable<TK>, Task<IDictionary<TK, TV>>> Build()
        {
            var functionCache = BuildFunctionCache();
            
            _cachedFunc = functionCache.GetMulti;
            
            return _cachedFunc;
        }

        public static implicit operator Func<IEnumerable<TK>, Task<IDictionary<TK, TV>>>(MultiKeyFunctionCacheConfigurationManager<TK, TV> cacheConfig)
        {
            return cacheConfig.Build();
        }
    }
}