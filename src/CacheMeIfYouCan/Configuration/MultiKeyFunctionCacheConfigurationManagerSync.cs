using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using CacheMeIfYouCan.Internal;

namespace CacheMeIfYouCan.Configuration
{
    public class MultiKeyFunctionCacheConfigurationManagerSync<TReq, TRes, TK, TV>
        : FunctionCacheConfigurationManagerBase<MultiKeyFunctionCacheConfigurationManagerSync<TReq, TRes, TK, TV>, TK, TV>
        where TReq : IEnumerable<TK>
        where TRes : IDictionary<TK, TV>
    {
        internal MultiKeyFunctionCacheConfigurationManagerSync(
            Func<TReq, TRes> inputFunc,
            string functionName)
            : base(inputFunc.ConvertInput<TReq, TRes, TK, TV>().ConvertToAsync().ConvertOutput<IEnumerable<TK>, TRes, TK, TV>(), functionName)
        { }

        internal MultiKeyFunctionCacheConfigurationManagerSync(
            Func<TReq, TRes> inputFunc,
            CachedProxyConfig interfaceConfig,
            MethodInfo methodInfo)
            : base(
                inputFunc.ConvertInput<TReq, TRes, TK, TV>().ConvertToAsync().ConvertOutput<IEnumerable<TK>, TRes, TK, TV>(),
                $"{interfaceConfig.InterfaceType.Name}.{methodInfo.Name}",
                interfaceConfig,
                new CachedProxyFunctionInfo(interfaceConfig.InterfaceType, methodInfo, typeof(TK), typeof(TV)))
        { }
        
        public Func<IEnumerable<TK>, TRes> Build()
        {
            var functionCache = BuildFunctionCacheMulti(DictionaryFactoryFuncResolver.Get<TRes, TK, TV>());
            
            PendingRequestsCounterContainer.Add(functionCache);
            
            return k => (TRes)functionCache.GetMulti(k).GetAwaiter().GetResult();
        }
    }
}