using System;
using System.Reflection;
using System.Threading.Tasks;
using CacheMeIfYouCan.Internal;

namespace CacheMeIfYouCan.Configuration
{
    public class FunctionCacheConfigurationManagerSync<TK, TV>
        : FunctionCacheConfigurationManagerBase<FunctionCacheConfigurationManagerSync<TK, TV>, TK, TV>
    {
        private Func<TK, TV> _cachedFunc;

        internal FunctionCacheConfigurationManagerSync(
            Func<TK, TV> inputFunc,
            string functionName)
            : base(k => Task.FromResult(inputFunc(k)), functionName)
        { }

        internal FunctionCacheConfigurationManagerSync(
            Func<TK, TV> inputFunc,
            CachedProxyConfig interfaceConfig,
            MethodInfo methodInfo)
            : base(
                k => Task.FromResult(inputFunc(k)),
                $"{interfaceConfig.InterfaceType.Name}.{methodInfo.Name}",
                interfaceConfig,
                new CachedProxyFunctionInfo(interfaceConfig.InterfaceType, methodInfo, typeof(TK), typeof(TV)))
        { }

        public new FunctionCacheConfigurationManagerSync<TK, TV> WithTimeToLiveFactory(
            Func<TK, TV, TimeSpan> timeToLiveFactory)
        {
            return base.WithTimeToLiveFactory(timeToLiveFactory);
        }
        
        public Func<TK, TV> Build()
        {
            var functionCache = BuildFunctionCacheSingle();

            _cachedFunc = k => Task.Run(() => functionCache.Get(k)).GetAwaiter().GetResult();
            
            PendingRequestsCounterContainer.Add(functionCache);
            
            return _cachedFunc;
        }
        
        public static implicit operator Func<TK, TV>(
            FunctionCacheConfigurationManagerSync<TK, TV> cacheConfig)
        {
            return cacheConfig.Build();
        }
    }
}