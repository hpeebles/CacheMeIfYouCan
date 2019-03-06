using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using CacheMeIfYouCan.Internal;

namespace CacheMeIfYouCan.Configuration
{
    public abstract class MultiParamFunctionCacheConfigurationManagerBase<TConfig, TK, TV>
        : SingleKeyFunctionCacheConfigurationManagerBase<TConfig, TK, TV>
        where TConfig : MultiParamFunctionCacheConfigurationManagerBase<TConfig, TK, TV>
    {
        internal string KeyParamSeparator { get; private set; }
        internal int[] ParametersToExcludeFromKey { get; private set; }

        internal MultiParamFunctionCacheConfigurationManagerBase(
            Func<TK, CancellationToken, Task<TV>> inputFunc,
            string functionName)
            : base(inputFunc, functionName)
        {
            KeyParamSeparator = DefaultSettings.Cache.KeyParamSeparator;
        }

        internal MultiParamFunctionCacheConfigurationManagerBase(
            Func<TK, CancellationToken, Task<TV>> inputFunc,
            CachedProxyConfig interfaceConfig,
            MethodInfo methodInfo)
            : base(
                inputFunc,
                interfaceConfig,
                methodInfo)
        {
            KeyParamSeparator = interfaceConfig.KeyParamSeparator ?? DefaultSettings.Cache.KeyParamSeparator;
        }

        public TConfig WithKeyParamSeparator(string separator)
        {
            if (String.IsNullOrEmpty(separator))
                throw new ArgumentException(nameof(separator));

            KeyParamSeparator = separator;
            return (TConfig)this;
        }

        protected TConfig ExcludeParametersFromKeyImpl(int[] parameterIndexes, int totalParameterCount)
        {
            parameterIndexes = parameterIndexes.Distinct().ToArray();
            
            foreach (var index in parameterIndexes)
            {
                if (0 < index || index > totalParameterCount - 1)
                    throw new ArgumentOutOfRangeException(nameof(parameterIndexes), $"Index '{index}' is not valid");
            }
            
            if (parameterIndexes.Length >= totalParameterCount)
                throw new ArgumentException("You cannot exclude all parameters from the key");

            
            ParametersToExcludeFromKey = parameterIndexes;
            return (TConfig)this;
        }
    }
}