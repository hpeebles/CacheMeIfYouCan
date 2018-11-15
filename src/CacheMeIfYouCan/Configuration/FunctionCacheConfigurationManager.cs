using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CacheMeIfYouCan.Internal;

namespace CacheMeIfYouCan.Configuration
{
    public class FunctionCacheConfigurationManager<TK, TV> : FunctionCacheConfigurationManagerBase<FunctionCacheConfigurationManager<TK, TV>, TK, TV>
    {
        private Func<TK, Task<TV>> _cachedFunc;

        internal FunctionCacheConfigurationManager(
            Func<TK, Task<TV>> inputFunc,
            string functionName)
            : base(ConvertFunc(inputFunc), functionName, false)
        { }

        internal FunctionCacheConfigurationManager(
            Func<TK, Task<TV>> inputFunc,
            CachedProxyConfig interfaceConfig,
            MethodInfo methodInfo)
            : base(
                ConvertFunc(inputFunc),
                $"{interfaceConfig.InterfaceType.Name}.{methodInfo.Name}",
                false,
                interfaceConfig,
                new CachedProxyFunctionInfo(interfaceConfig.InterfaceType, methodInfo, typeof(TK), typeof(TV)))
        { }
        
        public Func<TK, Task<TV>> Build()
        {
            var functionCache = BuildFunctionCache();
            
            _cachedFunc = functionCache.GetSingle;
            
            return _cachedFunc;
        }
        
        public static implicit operator Func<TK, Task<TV>>(FunctionCacheConfigurationManager<TK, TV> cacheConfig)
        {
            return cacheConfig.Build();
        }
        
        private static Func<IEnumerable<TK>, Task<IDictionary<TK, TV>>> ConvertFunc(Func<TK, Task<TV>> func)
        {
            return async keys =>
            {
                var key = keys.Single();

                var value = await func(key);
                
                return new Dictionary<TK, TV> { { key, value } };
            };
        }
    }
}