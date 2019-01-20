using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using CacheMeIfYouCan.Internal;

namespace CacheMeIfYouCan.Configuration
{
    public sealed class EnumerableKeyFunctionCacheConfigurationManager<TReq, TRes, TK, TV>
        : FunctionCacheConfigurationManagerBase<EnumerableKeyFunctionCacheConfigurationManager<TReq, TRes, TK, TV>, TK, TV>
        where TReq : IEnumerable<TK>
        where TRes : IDictionary<TK, TV>
    {
        internal EnumerableKeyFunctionCacheConfigurationManager(
            Func<TReq, Task<TRes>> inputFunc,
            string functionName)
            : base(
                inputFunc.ConvertInputToEnumerable<TReq, TRes, TK, TV>().ConvertOutputToDictionary<IEnumerable<TK>, TRes, TK, TV>(),
                functionName)
        { }

        internal EnumerableKeyFunctionCacheConfigurationManager(
            Func<TReq, Task<TRes>> inputFunc,
            CachedProxyConfig interfaceConfig,
            MethodInfo methodInfo)
            : base(
                inputFunc.ConvertInputToEnumerable<TReq, TRes, TK, TV>().ConvertOutputToDictionary<IEnumerable<TK>, TRes, TK, TV>(),
                $"{interfaceConfig.InterfaceType.Name}.{methodInfo.Name}",
                interfaceConfig,
                new CachedProxyFunctionInfo(interfaceConfig.InterfaceType, methodInfo, typeof(TK), typeof(TV)))
        { }
        
        public Func<TReq, Task<TRes>> Build()
        {
            var functionCache = BuildEnumerableKeyFunction(DictionaryFactoryFuncResolver.Get<TRes, TK, TV>());

            Func<IEnumerable<TK>, Task<IDictionary<TK, TV>>> func = functionCache.GetMulti;

            return func
                .ConvertInputFromEnumerable<TReq, IDictionary<TK, TV>, TK, TV>()
                .ConvertOutputFromDictionary<TReq, TRes, TK, TV>();
        }
    }
}