using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using CacheMeIfYouCan.Internal;
using CacheMeIfYouCan.Notifications;

namespace CacheMeIfYouCan.Configuration
{
    public sealed class EnumerableKeyFunctionCacheConfigurationManagerSync<TReq, TRes, TK, TV>
        : EnumerableKeyFunctionCacheConfigurationManagerBase<EnumerableKeyFunctionCacheConfigurationManagerSync<TReq, TRes, TK, TV>, TK, TV>
        where TReq : IEnumerable<TK>
        where TRes : IDictionary<TK, TV>
    {
        internal Func<IEqualityComparer<TK>, IReturnDictionaryBuilder<TK, TV, TRes>> ReturnDictionaryBuilderFunc { get; private set; }
        
        internal EnumerableKeyFunctionCacheConfigurationManagerSync(Func<TReq, TRes> inputFunc)
            : base(
                inputFunc
                    .ConvertToAsync()
                    .ConvertInputToEnumerable<TReq, TRes, TK, TV>()
                    .ConvertOutputToDictionary<IEnumerable<TK>, TRes, TK, TV>(),
                $"FunctionCache_{typeof(TReq).Name}->{typeof(TRes).Name}")
        { }

        internal EnumerableKeyFunctionCacheConfigurationManagerSync(
            Func<TReq, TRes> inputFunc,
            CachedProxyConfig interfaceConfig,
            MethodInfo methodInfo)
            : base(
                inputFunc
                    .ConvertToAsync()
                    .ConvertInputToEnumerable<TReq, TRes, TK, TV>()
                    .ConvertOutputToDictionary<IEnumerable<TK>, TRes, TK, TV>(),
                interfaceConfig,
                methodInfo)
        { }
        
        public EnumerableKeyFunctionCacheConfigurationManagerSync<TReq, TRes, TK, TV> WithReturnDictionaryBuilder(
            IReturnDictionaryBuilder<TK, TV, TRes> builder)
        {
            return WithReturnDictionaryBuilder(c => builder);
        }
        
        public EnumerableKeyFunctionCacheConfigurationManagerSync<TReq, TRes, TK, TV> WithReturnDictionaryBuilder(
            Func<IEqualityComparer<TK>, IReturnDictionaryBuilder<TK, TV, TRes>> builder)
        {
            ReturnDictionaryBuilderFunc = builder;
            return this;
        }
        
        public Func<TReq, TRes> Build()
        {
            var functionCache = BuildEnumerableKeyFunctionCache();

            var keyComparer = GetKeyComparer().Inner;

            var returnDictionaryBuilder = ReturnDictionaryBuilderFunc == null
                ? ReturnDictionaryBuilderResolver.Get<TRes, TK, TV>(keyComparer)
                : ReturnDictionaryBuilderFunc(keyComparer);
            
            Func<IEnumerable<TK>, Task<IDictionary<TK, TV>>> func = GetResults;
            
            return func
                .ConvertInputFromEnumerable<TReq, IDictionary<TK, TV>, TK, TV>()
                .ConvertOutputFromDictionary<TReq, TRes, TK, TV>()
                .ConvertToSync();
            
            async Task<IDictionary<TK, TV>> GetResults(IEnumerable<TK> keys)
            {
                var results = await functionCache.GetMulti(keys);

                return returnDictionaryBuilder.BuildResponse(results);
            }
        }
    }
}