using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using CacheMeIfYouCan.Internal;
using CacheMeIfYouCan.Serializers;

namespace CacheMeIfYouCan.Configuration
{
    public abstract class SingleKeyFunctionCacheConfigurationManager<TConfig, TK, TV>
        : SingleKeyFunctionCacheConfigurationManagerBase<TConfig, TK, TV>
        where TConfig : SingleKeyFunctionCacheConfigurationManager<TConfig, TK, TV>
    {
        internal SingleKeyFunctionCacheConfigurationManager(Func<TK, CancellationToken, Task<TV>> inputFunc)
            : base(inputFunc, $"FunctionCache_{typeof(TK).Name}->{typeof(TV).Name}")
        { }

        internal SingleKeyFunctionCacheConfigurationManager(
            Func<TK, CancellationToken, Task<TV>> inputFunc,
            CachedProxyConfig interfaceConfig,
            MethodInfo methodInfo)
            : base(
                inputFunc,
                interfaceConfig,
                methodInfo)
        { }

        public new TConfig WithTimeToLiveFactory(Func<TK, TV, TimeSpan> timeToLiveFactory, double jitterPercentage = 0)
        {
            return base.WithTimeToLiveFactory(timeToLiveFactory, jitterPercentage);
        }
        
        public TConfig WithKeySerializer(ISerializer<TK> serializer)
        {
            return base.WithKeySerializer(serializer.Serialize, serializer.Deserialize);
        }

        public TConfig WithKeySerializer(Func<TK, string> serializer, Func<string, TK> deserializer = null)
        {
            return base.WithKeySerializer(serializer, deserializer);
        }

        public new TConfig SkipCacheWhen(Func<TK, bool> predicate, SkipCacheSettings settings = SkipCacheSettings.SkipGetAndSet)
        {
            return base.SkipCacheWhen(predicate, settings);
        }
        
        public new TConfig OnlyStoreInLocalCacheWhen(Func<TK, TV, bool> predicate)
        {
            return base.OnlyStoreInLocalCacheWhen(predicate);
        }
    }
    
    public sealed class SingleKeyFunctionCacheConfigurationManagerNoCanx<TK, TV>
        : SingleKeyFunctionCacheConfigurationManager<SingleKeyFunctionCacheConfigurationManagerNoCanx<TK, TV>, TK, TV>
    {
        internal SingleKeyFunctionCacheConfigurationManagerNoCanx(Func<TK, Task<TV>> inputFunc)
            : base(inputFunc.AppearCancellable())
        { }

        internal SingleKeyFunctionCacheConfigurationManagerNoCanx(
            Func<TK, Task<TV>> inputFunc,
            CachedProxyConfig interfaceConfig,
            MethodInfo methodInfo)
            : base(
                inputFunc.AppearCancellable(),
                interfaceConfig,
                methodInfo)
        { }
        
        public Func<TK, Task<TV>> Build()
        {
            var functionCache = BuildFunctionCacheSingle();

            Func<TK, CancellationToken, Task<TV>> func = functionCache.Get;

            return func.MakeNonCancellable();
        }
    }
    
    public sealed class SingleKeyFunctionCacheConfigurationManagerCanx<TK, TV>
        : SingleKeyFunctionCacheConfigurationManager<SingleKeyFunctionCacheConfigurationManagerCanx<TK, TV>, TK, TV>
    {
        internal SingleKeyFunctionCacheConfigurationManagerCanx(Func<TK, CancellationToken, Task<TV>> inputFunc)
            : base(inputFunc)
        { }

        internal SingleKeyFunctionCacheConfigurationManagerCanx(
            Func<TK, CancellationToken, Task<TV>> inputFunc,
            CachedProxyConfig interfaceConfig,
            MethodInfo methodInfo)
            : base(
                inputFunc,
                interfaceConfig,
                methodInfo)
        { }
        
        public Func<TK, CancellationToken, Task<TV>> Build()
        {
            var functionCache = BuildFunctionCacheSingle();

            return functionCache.Get;
        }
    }
}