using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using CacheMeIfYouCan.Internal;

namespace CacheMeIfYouCan.Configuration
{
    public abstract class EnumerableKeyFunctionCacheConfigurationManagerBase<TConfig, TK, TV>
        : FunctionCacheConfigurationManagerBase<TConfig, TK, TV>
        where TConfig : EnumerableKeyFunctionCacheConfigurationManagerBase<TConfig, TK, TV>
    {
        private protected Func<IEqualityComparer<TK>, int, IDictionary<TK, TV>> _dictionaryFactoryFunc;
        
        internal EnumerableKeyFunctionCacheConfigurationManagerBase(
            Func<IEnumerable<TK>, Task<IDictionary<TK, TV>>> inputFunc,
            string functionName)
            : base(
                inputFunc,
                functionName)
        { }

        internal EnumerableKeyFunctionCacheConfigurationManagerBase(
            Func<IEnumerable<TK>, Task<IDictionary<TK, TV>>> inputFunc,
            CachedProxyConfig interfaceConfig,
            MethodInfo methodInfo)
            : base(
                inputFunc,
                $"{interfaceConfig.InterfaceType.Name}.{methodInfo.Name}",
                interfaceConfig,
                new CachedProxyFunctionInfo(interfaceConfig.InterfaceType, methodInfo, typeof(TK), typeof(TV)))
        { }

        public TConfig WithReturnDictionaryFactory(
            Func<IEqualityComparer<TK>, int, IDictionary<TK, TV>> dictionaryFactoryFunc)
        {
            _dictionaryFactoryFunc = dictionaryFactoryFunc;
            return (TConfig)this;
        }
    }
}