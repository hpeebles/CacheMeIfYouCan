using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using CacheMeIfYouCan.Internal;

namespace CacheMeIfYouCan.Configuration
{
    public class MultiKeyFunctionCacheConfigurationManagerSync<TK, TV>
        : FunctionCacheConfigurationManagerBase<MultiKeyFunctionCacheConfigurationManager<TK, TV>, TK, TV>
    {
        private Func<IEnumerable<TK>, IDictionary<TK, TV>> _cachedFunc;

        internal MultiKeyFunctionCacheConfigurationManagerSync(
            Func<IEnumerable<TK>, IDictionary<TK, TV>> inputFunc,
            string functionName)
            : base(k => Task.FromResult(inputFunc(k)), functionName)
        { }

        internal MultiKeyFunctionCacheConfigurationManagerSync(
            Func<IEnumerable<TK>, IDictionary<TK, TV>> inputFunc,
            CachedProxyConfig interfaceConfig,
            MethodInfo methodInfo)
            : base(
                k => Task.FromResult(inputFunc(k)),
                $"{interfaceConfig.InterfaceType.Name}.{methodInfo.Name}",
                interfaceConfig,
                new CachedProxyFunctionInfo(interfaceConfig.InterfaceType, methodInfo, typeof(TK), typeof(TV)))
        { }
        
        public Func<IEnumerable<TK>, IDictionary<TK, TV>> Build()
        {
            var functionCache = BuildFunctionCacheMulti();
            
            _cachedFunc = k => functionCache.GetMulti(k).GetAwaiter().GetResult();
            
            PendingRequestsCounterContainer.Add(functionCache);
            
            return _cachedFunc;
        }

        public static implicit operator Func<IEnumerable<TK>, IDictionary<TK, TV>>(
            MultiKeyFunctionCacheConfigurationManagerSync<TK, TV> cacheConfig)
        {
            return cacheConfig.Build();
        }
    }
}