using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using CacheMeIfYouCan.Internal;

namespace CacheMeIfYouCan.Configuration
{
    public class MultiKeyFunctionCacheConfigurationManager<TReq, TRes, TK, TV>
        : FunctionCacheConfigurationManagerBase<MultiKeyFunctionCacheConfigurationManager<TReq, TRes, TK, TV>, TK, TV>
        where TReq : IEnumerable<TK>
        where TRes : IDictionary<TK, TV>
    {
        internal MultiKeyFunctionCacheConfigurationManager(
            Func<TReq, Task<TRes>> inputFunc,
            string functionName)
            : base(inputFunc.ConvertInput<TReq, TRes, TK, TV>().ConvertOutput<IEnumerable<TK>, TRes, TK, TV>(), functionName)
        { }

        internal MultiKeyFunctionCacheConfigurationManager(
            Func<TReq, Task<TRes>> inputFunc,
            CachedProxyConfig interfaceConfig,
            MethodInfo methodInfo)
            : base(
                inputFunc.ConvertInput<TReq, TRes, TK, TV>().ConvertOutput<IEnumerable<TK>, TRes, TK, TV>(),
                $"{interfaceConfig.InterfaceType.Name}.{methodInfo.Name}",
                interfaceConfig,
                new CachedProxyFunctionInfo(interfaceConfig.InterfaceType, methodInfo, typeof(TK), typeof(TV)))
        { }
        
        public Func<TReq, Task<TRes>> Build()
        {
            var functionCache = BuildFunctionCacheMulti(DictionaryFactoryFuncResolver.Get<TRes, TK, TV>());
            
            PendingRequestsCounterContainer.Add(functionCache);
            
            return async k => (TRes)await functionCache.GetMulti(k);
        }
    }
}