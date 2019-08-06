using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using CacheMeIfYouCan.Internal;
using CacheMeIfYouCan.Serializers;

namespace CacheMeIfYouCan.Configuration
{
    public abstract class MultiParamFunctionCacheConfigurationManager<TConfig, TK1, TK2, TV>
        : MultiParamFunctionCacheConfigurationManagerBase<TConfig, (TK1, TK2), TV>
        where TConfig : MultiParamFunctionCacheConfigurationManager<TConfig, TK1, TK2, TV>
    {
        internal MultiParamFunctionCacheConfigurationManager(Func<TK1, TK2, CancellationToken, Task<TV>> inputFunc)
            : base(
                inputFunc.ConvertToSingleParam(),
                $"FunctionCache_{typeof(TK1).Name}+{typeof(TK2).Name}->{typeof(TV).Name}")
        { }
        
        internal MultiParamFunctionCacheConfigurationManager(
            Func<TK1, TK2, CancellationToken, Task<TV>> inputFunc,
            CachedProxyConfig interfaceConfig,
            MethodInfo methodInfo)
            : base(
                inputFunc.ConvertToSingleParam(),
                interfaceConfig,
                methodInfo)
        { }

        public new TConfig WithKeySerializer(ISerializer serializer)
        {
            return base
                .WithKeySerializerInternal(serializer.Serialize, serializer.Deserialize<TK1>)
                .WithKeySerializerInternal(serializer.Serialize, serializer.Deserialize<TK2>);
        }
        
        public TConfig WithKeySerializer(ISerializer<TK1> serializer)
        {
            return WithKeySerializer(serializer.Serialize, serializer.Deserialize);
        }
        
        public TConfig WithKeySerializer(ISerializer<TK2> serializer)
        {
            return WithKeySerializer(serializer.Serialize, serializer.Deserialize);
        }
        
        public TConfig WithKeySerializer(Func<TK1, string> serializer, Func<string, TK1> deserializer = null)
        {
            return WithKeySerializerInternal(serializer, deserializer);
        }
        
        public TConfig WithKeySerializer(Func<TK2, string> serializer, Func<string, TK2> deserializer = null)
        {
            return WithKeySerializerInternal(serializer, deserializer);
        }

        internal override Func<(TK1, TK2), string> GetKeySerializer()
        {
            return TupleKeyHelper.BuildKeySerializer<TK1, TK2>(KeySerializers, KeyParamSeparator, ParametersToExcludeFromKey);
        }
        
        internal override Func<string, (TK1, TK2)> GetKeyDeserializer()
        {
            return TupleKeyHelper.BuildKeyDeserializer<TK1, TK2>(KeySerializers, KeyParamSeparator, ParametersToExcludeFromKey);
        }
        
        public TConfig WithKeyComparer(IEqualityComparer<TK1> comparer)
        {
            return WithKeyComparerInternal(comparer);
        }
        
        public TConfig WithKeyComparer(IEqualityComparer<TK2> comparer)
        {
            return WithKeyComparerInternal(comparer);
        }
        
        public TConfig WithTimeToLiveFactory(Func<TK1, TK2, TV, TimeSpan> timeToLiveFactory)
        {
            return WithTimeToLiveFactory((k, v) => timeToLiveFactory(k.Item1, k.Item2, v));
        }
        
        public TConfig SkipCacheWhen(Func<TK1, TK2, bool> predicate, SkipCacheSettings settings = SkipCacheSettings.SkipGetAndSet)
        {
            return base.SkipCacheWhen(predicate.ConvertToSingleParamNoCanx(), settings);
        }

        public TConfig ExcludeParametersFromKey(params int[] parameterIndexes)
        {
            return ExcludeParametersFromKeyImpl(parameterIndexes, 2);
        }

        internal override KeyComparer<(TK1, TK2)> GetKeyComparer()
        {
            return TupleKeyHelper.BuildKeyComparer<TK1, TK2>(KeyComparers, ParametersToExcludeFromKey);
        }
    }

    public sealed class MultiParamFunctionCacheConfigurationManagerNoCanx<TK1, TK2, TV>
        : MultiParamFunctionCacheConfigurationManager<MultiParamFunctionCacheConfigurationManagerNoCanx<TK1, TK2, TV>, TK1, TK2, TV>
    {
        internal MultiParamFunctionCacheConfigurationManagerNoCanx(Func<TK1, TK2, Task<TV>> inputFunc)
            : base(inputFunc.AppearCancellable())
        { }
        
        internal MultiParamFunctionCacheConfigurationManagerNoCanx(
            Func<TK1, TK2, Task<TV>> inputFunc,
            CachedProxyConfig interfaceConfig,
            MethodInfo methodInfo)
            : base(inputFunc.AppearCancellable(), interfaceConfig, methodInfo)
        { }

        public Func<TK1, TK2, Task<TV>> Build()
        {
            var functionCache = BuildFunctionCacheSingle();
            
            Func<(TK1, TK2), CancellationToken, Task<TV>> func = functionCache.Get;

            return func.ConvertToMultiParam().MakeNonCancellable();
        }
    }

    public sealed class MultiParamFunctionCacheConfigurationManagerCanx<TK1, TK2, TV>
        : MultiParamFunctionCacheConfigurationManager<MultiParamFunctionCacheConfigurationManagerCanx<TK1, TK2, TV>, TK1, TK2, TV>
    {
        internal MultiParamFunctionCacheConfigurationManagerCanx(Func<TK1, TK2, CancellationToken, Task<TV>> inputFunc)
            : base(inputFunc)
        { }
        
        internal MultiParamFunctionCacheConfigurationManagerCanx(
            Func<TK1, TK2, CancellationToken, Task<TV>> inputFunc,
            CachedProxyConfig interfaceConfig,
            MethodInfo methodInfo)
            : base(inputFunc, interfaceConfig, methodInfo)
        { }

        public Func<TK1, TK2, CancellationToken, Task<TV>> Build()
        {
            var functionCache = BuildFunctionCacheSingle();
            
            Func<(TK1, TK2), CancellationToken, Task<TV>> func = functionCache.Get;

            return func.ConvertToMultiParam();
        }
    }
    
    public abstract class MultiParamFunctionCacheConfigurationManager<TConfig, TK1, TK2, TK3, TV>
        : MultiParamFunctionCacheConfigurationManagerBase<TConfig, (TK1, TK2, TK3), TV>
        where TConfig : MultiParamFunctionCacheConfigurationManager<TConfig, TK1, TK2, TK3, TV>
    {
        internal MultiParamFunctionCacheConfigurationManager(Func<TK1, TK2, TK3, CancellationToken, Task<TV>> inputFunc)
            : base(
                inputFunc.ConvertToSingleParam(),
                $"FunctionCache_{typeof(TK1).Name}+{typeof(TK2).Name}+{typeof(TK3).Name}->{typeof(TV).Name}")
        { }
        
        internal MultiParamFunctionCacheConfigurationManager(
            Func<TK1, TK2, TK3, CancellationToken, Task<TV>> inputFunc,
            CachedProxyConfig interfaceConfig,
            MethodInfo methodInfo)
            : base(
                inputFunc.ConvertToSingleParam(),
                interfaceConfig,
                methodInfo)
        { }
        
        public new TConfig WithKeySerializer(ISerializer serializer)
        {
            return base
                .WithKeySerializerInternal(serializer.Serialize, serializer.Deserialize<TK1>)
                .WithKeySerializerInternal(serializer.Serialize, serializer.Deserialize<TK2>)
                .WithKeySerializerInternal(serializer.Serialize, serializer.Deserialize<TK3>);
        }
        
        public TConfig WithKeySerializer(ISerializer<TK1> serializer)
        {
            return WithKeySerializer(serializer.Serialize, serializer.Deserialize);
        }
        
        public TConfig WithKeySerializer(ISerializer<TK2> serializer)
        {
            return WithKeySerializer(serializer.Serialize, serializer.Deserialize);
        }
        
        public TConfig WithKeySerializer(ISerializer<TK3> serializer)
        {
            return WithKeySerializer(serializer.Serialize, serializer.Deserialize);
        }
        
        public TConfig WithKeySerializer(Func<TK1, string> serializer, Func<string, TK1> deserializer = null)
        {
            return WithKeySerializerInternal(serializer, deserializer);
        }
        
        public TConfig WithKeySerializer(Func<TK2, string> serializer, Func<string, TK2> deserializer = null)
        {
            return WithKeySerializerInternal(serializer, deserializer);
        }

        public TConfig WithKeySerializer(Func<TK3, string> serializer, Func<string, TK3> deserializer = null)
        {
            return WithKeySerializerInternal(serializer, deserializer);
        }
        
        internal override Func<(TK1, TK2, TK3), string> GetKeySerializer()
        {
            return TupleKeyHelper.BuildKeySerializer<TK1, TK2, TK3>(KeySerializers, KeyParamSeparator, ParametersToExcludeFromKey);
        }
        
        internal override Func<string, (TK1, TK2, TK3)> GetKeyDeserializer()
        {
            return TupleKeyHelper.BuildKeyDeserializer<TK1, TK2, TK3>(KeySerializers, KeyParamSeparator, ParametersToExcludeFromKey);
        }
        
        public TConfig WithKeyComparer(IEqualityComparer<TK1> comparer)
        {
            return WithKeyComparerInternal(comparer);
        }
        
        public TConfig WithKeyComparer(IEqualityComparer<TK2> comparer)
        {
            return WithKeyComparerInternal(comparer);
        }
        
        public TConfig WithKeyComparer(IEqualityComparer<TK3> comparer)
        {
            return WithKeyComparerInternal(comparer);
        }
        
        public TConfig WithTimeToLiveFactory(Func<TK1, TK2, TK3, TV, TimeSpan> timeToLiveFactory)
        {
            return WithTimeToLiveFactory((k, v) => timeToLiveFactory(k.Item1, k.Item2, k.Item3, v));
        }
        
        public TConfig SkipCacheWhen(Func<TK1, TK2, TK3, bool> predicate, SkipCacheSettings settings = SkipCacheSettings.SkipGetAndSet)
        {
            return base.SkipCacheWhen(predicate.ConvertToSingleParamNoCanx(), settings);
        }

        public TConfig ExcludeParametersFromKey(params int[] parameterIndexes)
        {
            return ExcludeParametersFromKeyImpl(parameterIndexes, 3);
        }
        
        internal override KeyComparer<(TK1, TK2, TK3)> GetKeyComparer()
        {
            return TupleKeyHelper.BuildKeyComparer<TK1, TK2, TK3>(KeyComparers, ParametersToExcludeFromKey);
        }
    }
    
    public sealed class MultiParamFunctionCacheConfigurationManagerNoCanx<TK1, TK2, TK3, TV>
        : MultiParamFunctionCacheConfigurationManager<MultiParamFunctionCacheConfigurationManagerNoCanx<TK1, TK2, TK3, TV>, TK1, TK2, TK3, TV>
    {
        internal MultiParamFunctionCacheConfigurationManagerNoCanx(Func<TK1, TK2, TK3, Task<TV>> inputFunc)
            : base(inputFunc.AppearCancellable())
        { }
        
        internal MultiParamFunctionCacheConfigurationManagerNoCanx(
            Func<TK1, TK2, TK3, Task<TV>> inputFunc,
            CachedProxyConfig interfaceConfig,
            MethodInfo methodInfo)
            : base(inputFunc.AppearCancellable(), interfaceConfig, methodInfo)
        { }

        public Func<TK1, TK2, TK3, Task<TV>> Build()
        {
            var functionCache = BuildFunctionCacheSingle();
            
            Func<(TK1, TK2, TK3), CancellationToken, Task<TV>> func = functionCache.Get;

            return func.ConvertToMultiParam().MakeNonCancellable();
        }
    }

    public sealed class MultiParamFunctionCacheConfigurationManagerCanx<TK1, TK2, TK3, TV>
        : MultiParamFunctionCacheConfigurationManager<MultiParamFunctionCacheConfigurationManagerCanx<TK1, TK2, TK3, TV>, TK1, TK2, TK3, TV>
    {
        internal MultiParamFunctionCacheConfigurationManagerCanx(Func<TK1, TK2, TK3, CancellationToken, Task<TV>> inputFunc)
            : base(inputFunc)
        { }
        
        internal MultiParamFunctionCacheConfigurationManagerCanx(
            Func<TK1, TK2, TK3, CancellationToken, Task<TV>> inputFunc,
            CachedProxyConfig interfaceConfig,
            MethodInfo methodInfo)
            : base(inputFunc, interfaceConfig, methodInfo)
        { }

        public Func<TK1, TK2, TK3, CancellationToken, Task<TV>> Build()
        {
            var functionCache = BuildFunctionCacheSingle();
            
            Func<(TK1, TK2, TK3), CancellationToken, Task<TV>> func = functionCache.Get;

            return func.ConvertToMultiParam();
        }
    }
    
    public abstract class MultiParamFunctionCacheConfigurationManager<TConfig, TK1, TK2, TK3, TK4, TV>
        : MultiParamFunctionCacheConfigurationManagerBase<TConfig, (TK1, TK2, TK3, TK4), TV>
        where TConfig : MultiParamFunctionCacheConfigurationManager<TConfig, TK1, TK2, TK3, TK4, TV>
    {
        internal MultiParamFunctionCacheConfigurationManager(Func<TK1, TK2, TK3, TK4, CancellationToken, Task<TV>> inputFunc)
            : base(
                inputFunc.ConvertToSingleParam(),
                $"FunctionCache_{typeof(TK1).Name}+{typeof(TK2).Name}+{typeof(TK3).Name}+{typeof(TK4).Name}->{typeof(TV).Name}")
        { }
        
        internal MultiParamFunctionCacheConfigurationManager(
            Func<TK1, TK2, TK3, TK4, CancellationToken, Task<TV>> inputFunc,
            CachedProxyConfig interfaceConfig,
            MethodInfo methodInfo)
            : base(
                inputFunc.ConvertToSingleParam(),
                interfaceConfig,
                methodInfo)
        { }
        
        public new TConfig WithKeySerializer(ISerializer serializer)
        {
            return base
                .WithKeySerializerInternal(serializer.Serialize, serializer.Deserialize<TK1>)
                .WithKeySerializerInternal(serializer.Serialize, serializer.Deserialize<TK2>)
                .WithKeySerializerInternal(serializer.Serialize, serializer.Deserialize<TK3>)
                .WithKeySerializerInternal(serializer.Serialize, serializer.Deserialize<TK4>);
        }
        
        public TConfig WithKeySerializer(ISerializer<TK1> serializer)
        {
            return WithKeySerializer(serializer.Serialize, serializer.Deserialize);
        }
        
        public TConfig WithKeySerializer(ISerializer<TK2> serializer)
        {
            return WithKeySerializer(serializer.Serialize, serializer.Deserialize);
        }
        
        public TConfig WithKeySerializer(ISerializer<TK3> serializer)
        {
            return WithKeySerializer(serializer.Serialize, serializer.Deserialize);
        }
        
        public TConfig WithKeySerializer(ISerializer<TK4> serializer)
        {
            return WithKeySerializer(serializer.Serialize, serializer.Deserialize);
        }
        
        public TConfig WithKeySerializer(Func<TK1, string> serializer, Func<string, TK1> deserializer = null)
        {
            return WithKeySerializerInternal(serializer, deserializer);
        }
        
        public TConfig WithKeySerializer(Func<TK2, string> serializer, Func<string, TK2> deserializer = null)
        {
            return WithKeySerializerInternal(serializer, deserializer);
        }

        public TConfig WithKeySerializer(Func<TK3, string> serializer, Func<string, TK3> deserializer = null)
        {
            return WithKeySerializerInternal(serializer, deserializer);
        }
        
        public TConfig WithKeySerializer(Func<TK4, string> serializer, Func<string, TK4> deserializer = null)
        {
            return WithKeySerializerInternal(serializer, deserializer);
        }
        
        internal override Func<(TK1, TK2, TK3, TK4), string> GetKeySerializer()
        {
            return TupleKeyHelper.BuildKeySerializer<TK1, TK2, TK3, TK4>(KeySerializers, KeyParamSeparator, ParametersToExcludeFromKey);
        }
        
        internal override Func<string, (TK1, TK2, TK3, TK4)> GetKeyDeserializer()
        {
            return TupleKeyHelper.BuildKeyDeserializer<TK1, TK2, TK3, TK4>(KeySerializers, KeyParamSeparator, ParametersToExcludeFromKey);
        }
        
        public TConfig WithKeyComparer(IEqualityComparer<TK1> comparer)
        {
            return WithKeyComparerInternal(comparer);
        }
        
        public TConfig WithKeyComparer(IEqualityComparer<TK2> comparer)
        {
            return WithKeyComparerInternal(comparer);
        }
        
        public TConfig WithKeyComparer(IEqualityComparer<TK3> comparer)
        {
            return WithKeyComparerInternal(comparer);
        }
        
        public TConfig WithKeyComparer(IEqualityComparer<TK4> comparer)
        {
            return WithKeyComparerInternal(comparer);
        }

        public TConfig WithTimeToLiveFactory(Func<TK1, TK2, TK3, TK4, TV, TimeSpan> timeToLiveFactory)
        {
            return WithTimeToLiveFactory((k, v) => timeToLiveFactory(k.Item1, k.Item2, k.Item3, k.Item4, v));
        }
        
        public TConfig SkipCacheWhen(Func<TK1, TK2, TK3, TK4, bool> predicate, SkipCacheSettings settings = SkipCacheSettings.SkipGetAndSet)
        {
            return base.SkipCacheWhen(predicate.ConvertToSingleParamNoCanx(), settings);
        }

        public TConfig ExcludeParametersFromKey(params int[] parameterIndexes)
        {
            return ExcludeParametersFromKeyImpl(parameterIndexes, 4);
        }
        
        internal override KeyComparer<(TK1, TK2, TK3, TK4)> GetKeyComparer()
        {
            return TupleKeyHelper.BuildKeyComparer<TK1, TK2, TK3, TK4>(KeyComparers, ParametersToExcludeFromKey);
        }
    }
    
    public sealed class MultiParamFunctionCacheConfigurationManagerNoCanx<TK1, TK2, TK3, TK4, TV>
        : MultiParamFunctionCacheConfigurationManager<MultiParamFunctionCacheConfigurationManagerNoCanx<TK1, TK2, TK3, TK4, TV>, TK1, TK2, TK3, TK4, TV>
    {
        internal MultiParamFunctionCacheConfigurationManagerNoCanx(Func<TK1, TK2, TK3, TK4, Task<TV>> inputFunc)
            : base(inputFunc.AppearCancellable())
        { }
        
        internal MultiParamFunctionCacheConfigurationManagerNoCanx(
            Func<TK1, TK2, TK3, TK4, Task<TV>> inputFunc,
            CachedProxyConfig interfaceConfig,
            MethodInfo methodInfo)
            : base(inputFunc.AppearCancellable(), interfaceConfig, methodInfo)
        { }

        public Func<TK1, TK2, TK3, TK4, Task<TV>> Build()
        {
            var functionCache = BuildFunctionCacheSingle();
            
            Func<(TK1, TK2, TK3, TK4), CancellationToken, Task<TV>> func = functionCache.Get;

            return func.ConvertToMultiParam().MakeNonCancellable();
        }
    }

    public sealed class MultiParamFunctionCacheConfigurationManagerCanx<TK1, TK2, TK3, TK4, TV>
        : MultiParamFunctionCacheConfigurationManager<MultiParamFunctionCacheConfigurationManagerCanx<TK1, TK2, TK3, TK4, TV>, TK1, TK2, TK3, TK4, TV>
    {
        internal MultiParamFunctionCacheConfigurationManagerCanx(Func<TK1, TK2, TK3, TK4, CancellationToken, Task<TV>> inputFunc)
            : base(inputFunc)
        { }
        
        internal MultiParamFunctionCacheConfigurationManagerCanx(
            Func<TK1, TK2, TK3, TK4, CancellationToken, Task<TV>> inputFunc,
            CachedProxyConfig interfaceConfig,
            MethodInfo methodInfo)
            : base(inputFunc, interfaceConfig, methodInfo)
        { }

        public Func<TK1, TK2, TK3, TK4, CancellationToken, Task<TV>> Build()
        {
            var functionCache = BuildFunctionCacheSingle();
            
            Func<(TK1, TK2, TK3, TK4), CancellationToken, Task<TV>> func = functionCache.Get;

            return func.ConvertToMultiParam();
        }
    }
}