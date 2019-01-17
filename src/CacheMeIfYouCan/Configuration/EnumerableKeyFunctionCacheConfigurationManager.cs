using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using CacheMeIfYouCan.Internal;

namespace CacheMeIfYouCan.Configuration
{
    public class EnumerableKeyFunctionCacheConfigurationManager<TReq, TRes, TK, TV>
        : FunctionCacheConfigurationManagerBase<EnumerableKeyFunctionCacheConfigurationManager<TReq, TRes, TK, TV>, TK, TV>
        where TReq : IEnumerable<TK>
        where TRes : IDictionary<TK, TV>
    {
        internal EnumerableKeyFunctionCacheConfigurationManager(
            Func<TReq, Task<TRes>> inputFunc,
            string functionName)
            : base(inputFunc.ConvertInput<TReq, TRes, TK, TV>().ConvertOutput<IEnumerable<TK>, TRes, TK, TV>(), functionName)
        { }

        internal EnumerableKeyFunctionCacheConfigurationManager(
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
            var functionCache = BuildEnumerableKeyFunction(DictionaryFactoryFuncResolver.Get<TRes, TK, TV>());
            
            PendingRequestsCounterContainer.Add(functionCache);
            
            return async k => (TRes)await functionCache.GetMulti(k);
        }
    }
}