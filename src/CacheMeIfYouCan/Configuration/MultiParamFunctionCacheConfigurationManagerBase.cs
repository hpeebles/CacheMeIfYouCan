using System;
using System.Reflection;
using System.Threading.Tasks;
using CacheMeIfYouCan.Internal;

namespace CacheMeIfYouCan.Configuration
{
    public abstract class MultiParamFunctionCacheConfigurationManagerBase<TConfig, TK, TV>
        : FunctionCacheConfigurationManagerBase<TConfig, TK, TV>
        where TConfig : MultiParamFunctionCacheConfigurationManagerBase<TConfig, TK, TV>
    {
        private protected string _keyParamSeparator;

        internal MultiParamFunctionCacheConfigurationManagerBase(
            Func<TK, Task<TV>> inputFunc,
            string functionName)
            : base(inputFunc, functionName)
        {
            _keyParamSeparator = DefaultSettings.Cache.KeyParamSeparator;
        }

        internal MultiParamFunctionCacheConfigurationManagerBase(
            Func<TK, Task<TV>> inputFunc,
            CachedProxyConfig interfaceConfig,
            MethodInfo methodInfo)
            : base(
                inputFunc,
                $"{interfaceConfig.InterfaceType.Name}.{methodInfo.Name}",
                interfaceConfig,
                new CachedProxyFunctionInfo(interfaceConfig.InterfaceType, methodInfo, typeof(TK), typeof(TV)))
        {
            _keyParamSeparator = interfaceConfig.KeyParamSeparator;
        }

        public TConfig WithKeyParamSeparator(string separator)
        {
            _keyParamSeparator = separator;
            return (TConfig)this;
        }
    }
}