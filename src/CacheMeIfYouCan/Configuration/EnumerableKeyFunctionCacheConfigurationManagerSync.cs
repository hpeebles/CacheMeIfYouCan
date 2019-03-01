using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using CacheMeIfYouCan.Internal;
using CacheMeIfYouCan.Notifications;

namespace CacheMeIfYouCan.Configuration
{
    public abstract class EnumerableKeyFunctionCacheConfigurationManagerSync<TConfig, TReq, TRes, TK, TV>
        : EnumerableKeyFunctionCacheConfigurationManagerBase<TConfig, TK, TV>
        where TConfig : EnumerableKeyFunctionCacheConfigurationManagerSync<TConfig, TReq, TRes, TK, TV>
        where TReq : IEnumerable<TK>
        where TRes : IDictionary<TK, TV>
    {
        internal Func<IEqualityComparer<TK>, IReturnDictionaryBuilder<TK, TV, TRes>> ReturnDictionaryBuilderFunc { get; private set; }
        
        internal EnumerableKeyFunctionCacheConfigurationManagerSync(Func<TReq, CancellationToken, TRes> inputFunc)
            : base(
                inputFunc
                    .ConvertToAsync()
                    .ConvertInputToEnumerable<TReq, TRes, TK, TV>()
                    .ConvertOutputToDictionary<IEnumerable<TK>, TRes, TK, TV>(),
                $"FunctionCache_{typeof(TReq).Name}->{typeof(TRes).Name}")
        { }

        internal EnumerableKeyFunctionCacheConfigurationManagerSync(
            Func<TReq, CancellationToken, TRes> inputFunc,
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
        
        public TConfig WithReturnDictionaryBuilder(
            IReturnDictionaryBuilder<TK, TV, TRes> builder)
        {
            return WithReturnDictionaryBuilder(c => builder);
        }
        
        public TConfig WithReturnDictionaryBuilder(
            Func<IEqualityComparer<TK>, IReturnDictionaryBuilder<TK, TV, TRes>> builder)
        {
            ReturnDictionaryBuilderFunc = builder;
            return (TConfig)this;
        }
    }
    
    public sealed class EnumerableKeyFunctionCacheConfigurationManagerSyncNoCanx<TReq, TRes, TK, TV>
        : EnumerableKeyFunctionCacheConfigurationManagerSync<EnumerableKeyFunctionCacheConfigurationManagerSyncNoCanx<TReq, TRes, TK, TV>, TReq, TRes, TK, TV>
        where TReq : IEnumerable<TK>
        where TRes : IDictionary<TK, TV>
    {
        internal EnumerableKeyFunctionCacheConfigurationManagerSyncNoCanx(Func<TReq, TRes> inputFunc)
            : base(inputFunc.AppearCancellable())
        { }
        
        internal EnumerableKeyFunctionCacheConfigurationManagerSyncNoCanx(
            Func<TReq, TRes> inputFunc,
            CachedProxyConfig interfaceConfig,
            MethodInfo methodInfo)
            : base(inputFunc.AppearCancellable(), interfaceConfig, methodInfo)
        { }
        
        public Func<TReq, TRes> Build()
        {
            var functionCache = BuildEnumerableKeyFunctionCache();

            var keyComparer = GetKeyComparer().Inner;

            var returnDictionaryBuilder = ReturnDictionaryBuilderFunc == null
                ? ReturnDictionaryBuilderResolver.Get<TRes, TK, TV>(keyComparer)
                : ReturnDictionaryBuilderFunc(keyComparer);
            
            Func<IEnumerable<TK>, CancellationToken, Task<IDictionary<TK, TV>>> func = GetResults;
            
            return func
                .ConvertInputFromEnumerable<TReq, IDictionary<TK, TV>, TK, TV>()
                .ConvertOutputFromDictionary<TReq, TRes, TK, TV>()
                .ConvertToSync()
                .MakeNonCancellable();
            
            async Task<IDictionary<TK, TV>> GetResults(IEnumerable<TK> keys, CancellationToken token)
            {
                var results = await functionCache.GetMulti(keys, token);

                return returnDictionaryBuilder.BuildResponse(results);
            }
        }
    }
    
    public sealed class EnumerableKeyFunctionCacheConfigurationManagerSyncCanx<TReq, TRes, TK, TV>
        : EnumerableKeyFunctionCacheConfigurationManagerSync<EnumerableKeyFunctionCacheConfigurationManagerSyncCanx<TReq, TRes, TK, TV>, TReq, TRes, TK, TV>
        where TReq : IEnumerable<TK>
        where TRes : IDictionary<TK, TV>
    {
        internal EnumerableKeyFunctionCacheConfigurationManagerSyncCanx(Func<TReq, CancellationToken, TRes> inputFunc)
            : base(inputFunc)
        { }
        
        internal EnumerableKeyFunctionCacheConfigurationManagerSyncCanx(
            Func<TReq, CancellationToken, TRes> inputFunc,
            CachedProxyConfig interfaceConfig,
            MethodInfo methodInfo)
            : base(inputFunc, interfaceConfig, methodInfo)
        { }
        
        public Func<TReq, CancellationToken, Task<TRes>> Build()
        {
            var functionCache = BuildEnumerableKeyFunctionCache();

            var keyComparer = GetKeyComparer().Inner;

            var returnDictionaryBuilder = ReturnDictionaryBuilderFunc == null
                ? ReturnDictionaryBuilderResolver.Get<TRes, TK, TV>(keyComparer)
                : ReturnDictionaryBuilderFunc(keyComparer);

            Func<IEnumerable<TK>, CancellationToken, Task<IDictionary<TK, TV>>> func = GetResults;
            
            return func
                .ConvertInputFromEnumerable<TReq, IDictionary<TK, TV>, TK, TV>()
                .ConvertOutputFromDictionary<TReq, TRes, TK, TV>();

            async Task<IDictionary<TK, TV>> GetResults(IEnumerable<TK> keys, CancellationToken token)
            {
                var results = await functionCache.GetMulti(keys, token);

                return returnDictionaryBuilder.BuildResponse(results);
            }
        } 
    }
}