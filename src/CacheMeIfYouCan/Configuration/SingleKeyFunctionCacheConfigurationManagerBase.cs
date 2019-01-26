using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using CacheMeIfYouCan.Internal;

namespace CacheMeIfYouCan.Configuration
{
    public abstract class SingleKeyFunctionCacheConfigurationManagerBase<TConfig, TK, TV>
        : FunctionCacheConfigurationManagerBase<TConfig, TK, TV>
        where TConfig : SingleKeyFunctionCacheConfigurationManagerBase<TConfig, TK, TV>
    {
        internal SingleKeyFunctionCacheConfigurationManagerBase(
            Func<TK, Task<TV>> inputFunc,
            string functionName)
            : base(inputFunc, functionName)
        { }

        internal SingleKeyFunctionCacheConfigurationManagerBase(
            Func<TK, Task<TV>> inputFunc,
            CachedProxyConfig interfaceConfig,
            MethodInfo methodInfo)
            : base(
                inputFunc,
                $"{interfaceConfig.InterfaceType.Name}.{methodInfo.Name}",
                interfaceConfig,
                new CachedProxyFunctionInfo(interfaceConfig.InterfaceType, methodInfo, typeof(TK), typeof(TV)))
        { }

        public new TConfig WithTimeToLiveFactory(Func<TK, TV, TimeSpan> timeToLiveFactory)
        {
            return base.WithTimeToLiveFactory(timeToLiveFactory);
        }

        public TConfig WithKeyComparer(IEqualityComparer<TK> comparer)
        {
            return base.WithKeyComparer(comparer);
        }
    }
}