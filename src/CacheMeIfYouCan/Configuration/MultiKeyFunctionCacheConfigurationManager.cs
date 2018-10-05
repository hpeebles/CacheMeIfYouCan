using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using CacheMeIfYouCan.Caches;
using CacheMeIfYouCan.Internal;
using CacheMeIfYouCan.Notifications;
using CacheMeIfYouCan.Serializers;

[assembly: InternalsVisibleTo("ProxyFactoryAsm")]
[assembly: InternalsVisibleTo("CacheMeIfYouCan.Tests")]
namespace CacheMeIfYouCan.Configuration
{
    public class MultiKeyFunctionCacheConfigurationManager<TK, TV> : FunctionCacheConfigurationManagerBase<MultiKeyFunctionCacheConfigurationManager<TK, TV>, TK, TV>
    {
        private Func<IEnumerable<TK>, Task<IDictionary<TK, TV>>> _cachedFunc;

        internal MultiKeyFunctionCacheConfigurationManager(
            Func<IEnumerable<TK>, Task<IDictionary<TK, TV>>> inputFunc,
            string functionName,
            CachedProxyConfig interfaceConfig = null)
            : base(inputFunc, functionName, true, interfaceConfig)
        { }
        
        public Func<IEnumerable<TK>, Task<IDictionary<TK, TV>>> Build()
        {
            var functionCache = BuildFunctionCache();
            
            _cachedFunc = functionCache.GetMulti;
            
            return _cachedFunc;
        }

        public static implicit operator Func<IEnumerable<TK>, Task<IDictionary<TK, TV>>>(MultiKeyFunctionCacheConfigurationManager<TK, TV> cacheConfig)
        {
            return cacheConfig.Build();
        }
    }
}