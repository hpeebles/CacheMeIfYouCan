using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using CacheMeIfYouCan.Internal;
using CacheMeIfYouCan.Serializers;

namespace CacheMeIfYouCan.Configuration
{
    public abstract class MultiParamEnumerableKeyFunctionCacheConfigurationManager<TConfig, TKOuter, TKInnerEnumerable, TRes, TKInner, TV>
        : MultiParamEnumerableKeyFunctionCacheConfigurationManagerBase<TConfig, TKOuter, TKInner, TV>
        where TConfig : MultiParamEnumerableKeyFunctionCacheConfigurationManager<TConfig, TKOuter, TKInnerEnumerable, TRes, TKInner, TV>
        where TKInnerEnumerable : IEnumerable<TKInner>
        where TRes : IDictionary<TKInner, TV>
    {
        internal Func<IEqualityComparer<TKInner>, IReturnDictionaryBuilder<TKInner, TV, TRes>> ReturnDictionaryBuilderFunc { get; private set; }
        
        internal MultiParamEnumerableKeyFunctionCacheConfigurationManager(
            Func<TKOuter, TKInnerEnumerable, CancellationToken, Task<TRes>> inputFunc)
            : base(
                inputFunc
                    .ConvertInputToEnumerable<TKOuter, TKInnerEnumerable, TRes, TKInner, TV>()
                    .ConvertOutputToDictionary<TKOuter, IEnumerable<TKInner>, TRes, TKInner, TV>(),
                $"FunctionCache_{typeof(TKOuter).Name}+{typeof(TKInnerEnumerable).Name}->{typeof(TRes).Name}")
        { }

        internal MultiParamEnumerableKeyFunctionCacheConfigurationManager(
            Func<TKOuter, TKInnerEnumerable, CancellationToken, Task<TRes>> inputFunc,
            CachedProxyConfig interfaceConfig,
            MethodInfo methodInfo)
            : base(
                inputFunc
                    .ConvertInputToEnumerable<TKOuter, TKInnerEnumerable, TRes, TKInner, TV>()
                    .ConvertOutputToDictionary<TKOuter, IEnumerable<TKInner>, TRes, TKInner, TV>(),
                interfaceConfig,
                methodInfo)
        { }

        public new TConfig WithTimeToLiveFactory(Func<TKOuter, TimeSpan> timeToLiveFactory, double jitterPercentage = 0)
        {
            return base.WithTimeToLiveFactory(timeToLiveFactory, jitterPercentage);
        }
        
        public new TConfig WithKeySerializer(ISerializer serializer)
        {
            return base
                .WithKeySerializerInternal(serializer.Serialize, serializer.Deserialize<TKOuter>)
                .WithKeySerializerInternal(serializer.Serialize, serializer.Deserialize<TKInner>);
        }
        
        public TConfig WithKeySerializer(ISerializer<TKOuter> serializer)
        {
            return WithKeySerializer(serializer.Serialize, serializer.Deserialize);
        }
        
        public TConfig WithKeySerializer(ISerializer<TKInner> serializer)
        {
            return WithKeySerializer(serializer.Serialize, serializer.Deserialize);
        }
        
        public TConfig WithKeySerializer(Func<TKOuter, string> serializer, Func<string, TKOuter> deserializer = null)
        {
            return WithKeySerializerInternal(serializer, deserializer);
        }
        
        public TConfig WithKeySerializer(Func<TKInner, string> serializer, Func<string, TKInner> deserializer = null)
        {
            return WithKeySerializerInternal(serializer, deserializer);
        }

        public TConfig WithKeySerializer<T>(ISerializer serializer)
        {
            var type = typeof(T);
            var match = false;
            if (typeof(TKOuter) == type)
            {
                WithKeySerializerInternal(serializer.Serialize, serializer.Deserialize<TKOuter>);
                match = true;
            }

            if (typeof(TKInner) == type)
            {
                WithKeySerializerInternal(serializer.Serialize, serializer.Deserialize<TKInner>);
                match = true;
            }

            if (!match)
                throw new InvalidOperationException($"Cannot use '{typeof(T).Name}' as the type argument in {this.GetType().Name}.{nameof(WithKeySerializer)} as no keys are of that type");

            return (TConfig)this;
        }

        public TConfig WithKeyComparer(IEqualityComparer<TKOuter> comparer)
        {
            return WithKeyComparerInternal(comparer);
        }
        
        public TConfig WithKeyComparer(IEqualityComparer<TKInner> comparer)
        {
            return WithKeyComparerInternal(comparer);
        }
        
        public TConfig SkipCacheWhen(Func<TKOuter, TKInner, bool> predicate, SkipCacheSettings settings = SkipCacheSettings.SkipGetAndSet)
        {
            return base.SkipCacheWhen(x => predicate(x.Item1, x.Item2), settings);
        }

        public new TConfig SkipCacheWhen(Func<TKOuter, bool> predicate, SkipCacheSettings settings = SkipCacheSettings.SkipGetAndSet)
        {
            return base.SkipCacheWhen(predicate, settings);
        }

        public TConfig ExcludeParametersFromKey(params int[] parameterIndexes)
        {
            return ExcludeParametersFromKeyImpl(parameterIndexes, 2);
        }
        
        public TConfig WithReturnDictionaryBuilder(IReturnDictionaryBuilder<TKInner, TV, TRes> builder)
        {
            return WithReturnDictionaryBuilder(c => builder);
        }
        
        public TConfig WithReturnDictionaryBuilder(Func<IEqualityComparer<TKInner>, IReturnDictionaryBuilder<TKInner, TV, TRes>> builder)
        {
            ReturnDictionaryBuilderFunc = builder;
            return (TConfig)this;
        }
    }

    public sealed class MultiParamEnumerableKeyFunctionCacheConfigurationManagerNoCanx<TKOuter, TKInnerEnumerable, TRes, TKInner, TV>
        : MultiParamEnumerableKeyFunctionCacheConfigurationManager<MultiParamEnumerableKeyFunctionCacheConfigurationManagerNoCanx<TKOuter, TKInnerEnumerable, TRes, TKInner, TV>, TKOuter, TKInnerEnumerable, TRes, TKInner, TV>
        where TKInnerEnumerable : IEnumerable<TKInner>
        where TRes : IDictionary<TKInner, TV>
    {
        internal MultiParamEnumerableKeyFunctionCacheConfigurationManagerNoCanx(
            Func<TKOuter, TKInnerEnumerable, Task<TRes>> inputFunc)
            : base(inputFunc.AppearCancellable())
        { }
        
        internal MultiParamEnumerableKeyFunctionCacheConfigurationManagerNoCanx(
            Func<TKOuter, TKInnerEnumerable, Task<TRes>> inputFunc,
            CachedProxyConfig interfaceConfig,
            MethodInfo methodInfo)
            : base(inputFunc.AppearCancellable(), interfaceConfig, methodInfo)
        { }
        
        public Func<TKOuter, TKInnerEnumerable, Task<TRes>> Build()
        {
            var key1Comparer = ParametersToExcludeFromKey == null || !ParametersToExcludeFromKey.Contains(0)
                ? KeyComparerResolver.Get<TKOuter>(KeyComparers)
                : new KeyComparer<TKOuter>(new AlwaysEqualComparer<TKOuter>());

            var key1Serializer = ParametersToExcludeFromKey == null || !ParametersToExcludeFromKey.Contains(0)
                ? GetKeySerializerImpl<TKOuter>()
                : k => null;
            
            var functionCache = BuildMultiParamEnumerableKeyFunctionCache(key1Comparer, key1Serializer);

            var keyComparer = KeyComparerResolver.GetInner<TKInner>();

            var returnDictionaryBuilder = ReturnDictionaryBuilderFunc == null
                ? ReturnDictionaryBuilderResolver.Get<TRes, TKInner, TV>(keyComparer)
                : ReturnDictionaryBuilderFunc(keyComparer);
            
            Func<TKOuter, IEnumerable<TKInner>, CancellationToken, Task<IDictionary<TKInner, TV>>> func = GetResults;
            
            return func
                .ConvertInputFromEnumerable<TKOuter, TKInnerEnumerable, IDictionary<TKInner, TV>, TKInner, TV>()
                .ConvertOutputFromDictionary<TKOuter, TKInnerEnumerable, TRes, TKInner, TV>()
                .MakeNonCancellable();

            async Task<IDictionary<TKInner, TV>> GetResults(TKOuter outerKey, IEnumerable<TKInner> innerKeys, CancellationToken token)
            {
                var results = await functionCache.GetMulti(outerKey, innerKeys, token);

                return returnDictionaryBuilder.BuildResponse(
                    results.Select(r => (IKeyValuePair<TKInner, TV>)new KeyValuePairInternal<TKInner, TV>(r.Key.Item2, r.Value)),
                    results.Count);
            }
        }
    }
    
    public sealed class MultiParamEnumerableKeyFunctionCacheConfigurationManagerCanx<TKOuter, TKInnerEnumerable, TRes, TKInner, TV>
        : MultiParamEnumerableKeyFunctionCacheConfigurationManager<MultiParamEnumerableKeyFunctionCacheConfigurationManagerCanx<TKOuter, TKInnerEnumerable, TRes, TKInner, TV>, TKOuter, TKInnerEnumerable, TRes, TKInner, TV>
        where TKInnerEnumerable : IEnumerable<TKInner>
        where TRes : IDictionary<TKInner, TV>
    {
        internal MultiParamEnumerableKeyFunctionCacheConfigurationManagerCanx(
            Func<TKOuter, TKInnerEnumerable, CancellationToken, Task<TRes>> inputFunc)
            : base(inputFunc)
        { }
        
        internal MultiParamEnumerableKeyFunctionCacheConfigurationManagerCanx(
            Func<TKOuter, TKInnerEnumerable, CancellationToken, Task<TRes>> inputFunc,
            CachedProxyConfig interfaceConfig,
            MethodInfo methodInfo)
            : base(inputFunc, interfaceConfig, methodInfo)
        { }
        
        public Func<TKOuter, TKInnerEnumerable, CancellationToken, Task<TRes>> Build()
        {
            var key1Comparer = ParametersToExcludeFromKey == null || !ParametersToExcludeFromKey.Contains(0)
                ? KeyComparerResolver.Get<TKOuter>(KeyComparers)
                : new KeyComparer<TKOuter>(new AlwaysEqualComparer<TKOuter>());

            var key1Serializer = ParametersToExcludeFromKey == null || !ParametersToExcludeFromKey.Contains(0)
                ? GetKeySerializerImpl<TKOuter>()
                : k => null;

            var functionCache = BuildMultiParamEnumerableKeyFunctionCache(key1Comparer, key1Serializer);

            var keyComparer = KeyComparerResolver.GetInner<TKInner>();

            var returnDictionaryBuilder = ReturnDictionaryBuilderFunc == null
                ? ReturnDictionaryBuilderResolver.Get<TRes, TKInner, TV>(keyComparer)
                : ReturnDictionaryBuilderFunc(keyComparer);
            
            Func<TKOuter, IEnumerable<TKInner>, CancellationToken, Task<IDictionary<TKInner, TV>>> func = GetResults;
            
            return func
                .ConvertInputFromEnumerable<TKOuter, TKInnerEnumerable, IDictionary<TKInner, TV>, TKInner, TV>()
                .ConvertOutputFromDictionary<TKOuter, TKInnerEnumerable, TRes, TKInner, TV>();

            async Task<IDictionary<TKInner, TV>> GetResults(TKOuter outerKey, IEnumerable<TKInner> innerKeys, CancellationToken token)
            {
                var results = await functionCache.GetMulti(outerKey, innerKeys, token);

                return returnDictionaryBuilder.BuildResponse(
                    results.Select(r => (IKeyValuePair<TKInner, TV>)new KeyValuePairInternal<TKInner, TV>(r.Key.Item2, r.Value)),
                    results.Count);
            }
        }
    }

    public abstract class MultiParamEnumerableKeyFunctionCacheConfigurationManager<TConfig, TKOuter1, TKOuter2, TKInnerEnumerable, TRes, TKInner, TV>
        : MultiParamEnumerableKeyFunctionCacheConfigurationManagerBase<TConfig, (TKOuter1, TKOuter2), TKInner, TV>
        where TConfig : MultiParamEnumerableKeyFunctionCacheConfigurationManager<TConfig, TKOuter1, TKOuter2, TKInnerEnumerable, TRes, TKInner, TV>
        where TKInnerEnumerable : IEnumerable<TKInner>
        where TRes : IDictionary<TKInner, TV>
    {
        internal Func<IEqualityComparer<TKInner>, IReturnDictionaryBuilder<TKInner, TV, TRes>> ReturnDictionaryBuilderFunc { get; private set; }
        
        internal MultiParamEnumerableKeyFunctionCacheConfigurationManager(
            Func<TKOuter1, TKOuter2, TKInnerEnumerable, CancellationToken, Task<TRes>> inputFunc)
            : base(
                inputFunc
                    .ConvertFunc()
                    .ConvertInputToEnumerable<(TKOuter1, TKOuter2), TKInnerEnumerable, TRes, TKInner, TV>()
                    .ConvertOutputToDictionary<(TKOuter1, TKOuter2), IEnumerable<TKInner>, TRes, TKInner, TV>(),
                $"FunctionCache_{typeof(TKOuter1).Name}+{typeof(TKOuter2).Name}+{typeof(TKInnerEnumerable).Name}->{typeof(TRes).Name}")
        { }

        internal MultiParamEnumerableKeyFunctionCacheConfigurationManager(
            Func<TKOuter1, TKOuter2, TKInnerEnumerable, CancellationToken, Task<TRes>> inputFunc,
            CachedProxyConfig interfaceConfig,
            MethodInfo methodInfo)
            : base(
                inputFunc
                    .ConvertFunc()
                    .ConvertInputToEnumerable<(TKOuter1, TKOuter2), TKInnerEnumerable, TRes, TKInner, TV>()
                    .ConvertOutputToDictionary<(TKOuter1, TKOuter2), IEnumerable<TKInner>, TRes, TKInner, TV>(),
                interfaceConfig,
                methodInfo)
        { }

        public TConfig WithTimeToLiveFactory(Func<TKOuter1, TKOuter2, TimeSpan> timeToLiveFactory, double jitterPercentage = 0)
        {
            return base.WithTimeToLiveFactory(timeToLiveFactory.ConvertToSingleParamNoCanx(), jitterPercentage);
        }
        
        public new TConfig WithKeySerializer(ISerializer serializer)
        {
            return base
                .WithKeySerializerInternal(serializer.Serialize, serializer.Deserialize<TKOuter1>)
                .WithKeySerializerInternal(serializer.Serialize, serializer.Deserialize<TKOuter2>)
                .WithKeySerializerInternal(serializer.Serialize, serializer.Deserialize<TKInner>);
        }

        public TConfig WithKeySerializer(ISerializer<TKOuter1> serializer)
        {
            return WithKeySerializer(serializer.Serialize, serializer.Deserialize);
        }
        
        public TConfig WithKeySerializer(ISerializer<TKOuter2> serializer)
        {
            return WithKeySerializer(serializer.Serialize, serializer.Deserialize);
        }

        public TConfig WithKeySerializer(ISerializer<TKInner> serializer)
        {
            return WithKeySerializer(serializer.Serialize, serializer.Deserialize);
        }

        public TConfig WithKeySerializer(Func<TKOuter1, string> serializer, Func<string, TKOuter1> deserializer = null)
        {
            return WithKeySerializerInternal(serializer, deserializer);
        }
        
        public TConfig WithKeySerializer(Func<TKOuter2, string> serializer, Func<string, TKOuter2> deserializer = null)
        {
            return WithKeySerializerInternal(serializer, deserializer);
        }

        public TConfig WithKeySerializer(Func<TKInner, string> serializer, Func<string, TKInner> deserializer = null)
        {
            return WithKeySerializerInternal(serializer, deserializer);
        }

        public TConfig WithKeySerializer<T>(ISerializer serializer)
        {
            var type = typeof(T);
            var match = false;
            if (typeof(TKOuter1) == type)
            {
                WithKeySerializerInternal(serializer.Serialize, serializer.Deserialize<TKOuter1>);
                match = true;
            }

            if (typeof(TKOuter2) == type)
            {
                WithKeySerializerInternal(serializer.Serialize, serializer.Deserialize<TKOuter2>);
                match = true;
            }

            if (typeof(TKInner) == type)
            {
                WithKeySerializerInternal(serializer.Serialize, serializer.Deserialize<TKInner>);
                match = true;
            }

            if (!match)
                throw new InvalidOperationException($"Cannot use '{typeof(T).Name}' as the type argument in {this.GetType().Name}.{nameof(WithKeySerializer)} as no keys are of that type");

            return (TConfig)this;
        }

        public TConfig WithKeyComparer(IEqualityComparer<TKOuter1> comparer)
        {
            return WithKeyComparerInternal(comparer);
        }

        public TConfig WithKeyComparer(IEqualityComparer<TKOuter2> comparer)
        {
            return WithKeyComparerInternal(comparer);
        }
        
        public TConfig WithKeyComparer(IEqualityComparer<TKInner> comparer)
        {
            return WithKeyComparerInternal(comparer);
        }
        
        public TConfig SkipCacheWhen(Func<TKOuter1, TKOuter2, TKInner, bool> predicate, SkipCacheSettings settings = SkipCacheSettings.SkipGetAndSet)
        {
            return base.SkipCacheWhen(x => predicate(x.Item1.Item1, x.Item1.Item2, x.Item2), settings);
        }

        public TConfig SkipCacheWhen(Func<TKOuter1, TKOuter2, bool> predicate, SkipCacheSettings settings = SkipCacheSettings.SkipGetAndSet)
        {
            return base.SkipCacheWhen(predicate.ConvertToSingleParamNoCanx(), settings);
        }

        public TConfig ExcludeParametersFromKey(params int[] parameterIndexes)
        {
            return ExcludeParametersFromKeyImpl(parameterIndexes, 3);
        }
        
        public TConfig WithReturnDictionaryBuilder(IReturnDictionaryBuilder<TKInner, TV, TRes> builder)
        {
            return WithReturnDictionaryBuilder(c => builder);
        }
        
        public TConfig WithReturnDictionaryBuilder(Func<IEqualityComparer<TKInner>, IReturnDictionaryBuilder<TKInner, TV, TRes>> builder)
        {
            ReturnDictionaryBuilderFunc = builder;
            return (TConfig)this;
        }
    }
    
    public sealed class MultiParamEnumerableKeyFunctionCacheConfigurationManagerNoCanx<TKOuter1, TKOuter2, TKInnerEnumerable, TRes, TKInner, TV>
        : MultiParamEnumerableKeyFunctionCacheConfigurationManager<MultiParamEnumerableKeyFunctionCacheConfigurationManagerNoCanx<TKOuter1, TKOuter2, TKInnerEnumerable, TRes, TKInner, TV>, TKOuter1, TKOuter2, TKInnerEnumerable, TRes, TKInner, TV>
        where TKInnerEnumerable : IEnumerable<TKInner>
        where TRes : IDictionary<TKInner, TV>
    {
        internal MultiParamEnumerableKeyFunctionCacheConfigurationManagerNoCanx(
            Func<TKOuter1, TKOuter2, TKInnerEnumerable, Task<TRes>> inputFunc)
            : base(inputFunc.AppearCancellable())
        { }
        
        internal MultiParamEnumerableKeyFunctionCacheConfigurationManagerNoCanx(
            Func<TKOuter1, TKOuter2, TKInnerEnumerable, Task<TRes>> inputFunc,
            CachedProxyConfig interfaceConfig,
            MethodInfo methodInfo)
            : base(inputFunc.AppearCancellable(), interfaceConfig, methodInfo)
        { }
        
        public Func<TKOuter1, TKOuter2, TKInnerEnumerable, Task<TRes>> Build()
        {
            var functionCache = BuildMultiParamEnumerableKeyFunctionCache(
                TupleKeyHelper.BuildKeyComparer<TKOuter1, TKOuter2>(KeyComparers, ParametersToExcludeFromKey),
                TupleKeyHelper.BuildKeySerializer<TKOuter1, TKOuter2>(KeySerializers, KeyParamSeparator, ParametersToExcludeFromKey));

            var keyComparer = KeyComparerResolver.GetInner<TKInner>();

            var returnDictionaryBuilder = ReturnDictionaryBuilderFunc == null
                ? ReturnDictionaryBuilderResolver.Get<TRes, TKInner, TV>(keyComparer)
                : ReturnDictionaryBuilderFunc(keyComparer);

            Func<(TKOuter1, TKOuter2), IEnumerable<TKInner>, CancellationToken, Task<IDictionary<TKInner, TV>>> func = GetResults;

            return func
                .ConvertInputFromEnumerable<(TKOuter1, TKOuter2), TKInnerEnumerable, IDictionary<TKInner, TV>, TKInner, TV>()
                .ConvertOutputFromDictionary<(TKOuter1, TKOuter2), TKInnerEnumerable, TRes, TKInner, TV>()
                .ConvertFunc()
                .MakeNonCancellable();

            async Task<IDictionary<TKInner, TV>> GetResults(
                (TKOuter1, TKOuter2) outerKeys,
                IEnumerable<TKInner> innerKeys,
                CancellationToken token)
            {
                var results = await functionCache.GetMulti(outerKeys, innerKeys, token);

                return returnDictionaryBuilder.BuildResponse(
                    results.Select(r => (IKeyValuePair<TKInner, TV>)new KeyValuePairInternal<TKInner, TV>(r.Key.Item2, r.Value)),
                    results.Count);
            }
        }
    }
    
    public sealed class MultiParamEnumerableKeyFunctionCacheConfigurationManagerCanx<TKOuter1, TKOuter2, TKInnerEnumerable, TRes, TKInner, TV>
        : MultiParamEnumerableKeyFunctionCacheConfigurationManager<MultiParamEnumerableKeyFunctionCacheConfigurationManagerCanx<TKOuter1, TKOuter2, TKInnerEnumerable, TRes, TKInner, TV>, TKOuter1, TKOuter2, TKInnerEnumerable, TRes, TKInner, TV>
        where TKInnerEnumerable : IEnumerable<TKInner>
        where TRes : IDictionary<TKInner, TV>
    {
        internal MultiParamEnumerableKeyFunctionCacheConfigurationManagerCanx(
            Func<TKOuter1, TKOuter2, TKInnerEnumerable, CancellationToken, Task<TRes>> inputFunc)
            : base(inputFunc)
        { }
        
        internal MultiParamEnumerableKeyFunctionCacheConfigurationManagerCanx(
            Func<TKOuter1, TKOuter2, TKInnerEnumerable, CancellationToken, Task<TRes>> inputFunc,
            CachedProxyConfig interfaceConfig,
            MethodInfo methodInfo)
            : base(inputFunc, interfaceConfig, methodInfo)
        { }
        
        public Func<TKOuter1, TKOuter2, TKInnerEnumerable, CancellationToken, Task<TRes>> Build()
        {
            var functionCache = BuildMultiParamEnumerableKeyFunctionCache(
                TupleKeyHelper.BuildKeyComparer<TKOuter1, TKOuter2>(KeyComparers, ParametersToExcludeFromKey),
                TupleKeyHelper.BuildKeySerializer<TKOuter1, TKOuter2>(KeySerializers, KeyParamSeparator, ParametersToExcludeFromKey));

            var keyComparer = KeyComparerResolver.GetInner<TKInner>();

            var returnDictionaryBuilder = ReturnDictionaryBuilderFunc == null
                ? ReturnDictionaryBuilderResolver.Get<TRes, TKInner, TV>(keyComparer)
                : ReturnDictionaryBuilderFunc(keyComparer);

            Func<(TKOuter1, TKOuter2), IEnumerable<TKInner>, CancellationToken, Task<IDictionary<TKInner, TV>>> func = GetResults;

            return func
                .ConvertInputFromEnumerable<(TKOuter1, TKOuter2), TKInnerEnumerable, IDictionary<TKInner, TV>, TKInner, TV>()
                .ConvertOutputFromDictionary<(TKOuter1, TKOuter2), TKInnerEnumerable, TRes, TKInner, TV>()
                .ConvertFunc();

            async Task<IDictionary<TKInner, TV>> GetResults(
                (TKOuter1, TKOuter2) outerKeys,
                IEnumerable<TKInner> innerKeys,
                CancellationToken token)
            {
                var results = await functionCache.GetMulti(outerKeys, innerKeys, token);

                return returnDictionaryBuilder.BuildResponse(
                    results.Select(r => (IKeyValuePair<TKInner, TV>)new KeyValuePairInternal<TKInner, TV>(r.Key.Item2, r.Value)),
                    results.Count);
            }
        }
    }

    public abstract class MultiParamEnumerableKeyFunctionCacheConfigurationManager<TConfig, TKOuter1, TKOuter2, TKOuter3, TKInnerEnumerable, TRes, TKInner, TV>
        : MultiParamEnumerableKeyFunctionCacheConfigurationManagerBase<TConfig, (TKOuter1, TKOuter2, TKOuter3), TKInner, TV>
        where TConfig : MultiParamEnumerableKeyFunctionCacheConfigurationManager<TConfig, TKOuter1, TKOuter2, TKOuter3, TKInnerEnumerable, TRes, TKInner, TV>
        where TKInnerEnumerable : IEnumerable<TKInner>
        where TRes : IDictionary<TKInner, TV>
    {
        internal Func<IEqualityComparer<TKInner>, IReturnDictionaryBuilder<TKInner, TV, TRes>> ReturnDictionaryBuilderFunc { get; private set; }
        
        internal MultiParamEnumerableKeyFunctionCacheConfigurationManager(
            Func<TKOuter1, TKOuter2, TKOuter3, TKInnerEnumerable, CancellationToken, Task<TRes>> inputFunc)
            : base(
                inputFunc
                    .ConvertFunc()
                    .ConvertInputToEnumerable<(TKOuter1, TKOuter2, TKOuter3), TKInnerEnumerable, TRes, TKInner, TV>()
                    .ConvertOutputToDictionary<(TKOuter1, TKOuter2, TKOuter3), IEnumerable<TKInner>, TRes, TKInner, TV>(),
                $"FunctionCache_{typeof(TKOuter1).Name}+{typeof(TKOuter2).Name}+{typeof(TKOuter3).Name}+{typeof(TKInnerEnumerable).Name}->{typeof(TRes).Name}")
        { }

        internal MultiParamEnumerableKeyFunctionCacheConfigurationManager(
            Func<TKOuter1, TKOuter2, TKOuter3, TKInnerEnumerable, CancellationToken, Task<TRes>> inputFunc,
            CachedProxyConfig interfaceConfig,
            MethodInfo methodInfo)
            : base(
                inputFunc
                    .ConvertFunc()
                    .ConvertInputToEnumerable<(TKOuter1, TKOuter2, TKOuter3), TKInnerEnumerable, TRes, TKInner, TV>()
                    .ConvertOutputToDictionary<(TKOuter1, TKOuter2, TKOuter3), IEnumerable<TKInner>, TRes, TKInner, TV>(),
                interfaceConfig,
                methodInfo)
        { }

        public TConfig WithTimeToLiveFactory(Func<TKOuter1, TKOuter2, TKOuter3, TimeSpan> timeToLiveFactory, double jitterPercentage = 0)
        {
            return base.WithTimeToLiveFactory(timeToLiveFactory.ConvertToSingleParamNoCanx(), jitterPercentage);
        }

        public new TConfig WithKeySerializer(ISerializer serializer)
        {
            return base
                .WithKeySerializerInternal(serializer.Serialize, serializer.Deserialize<TKOuter1>)
                .WithKeySerializerInternal(serializer.Serialize, serializer.Deserialize<TKOuter2>)
                .WithKeySerializerInternal(serializer.Serialize, serializer.Deserialize<TKOuter3>)
                .WithKeySerializerInternal(serializer.Serialize, serializer.Deserialize<TKInner>);
        }

        public TConfig WithKeySerializer(ISerializer<TKOuter1> serializer)
        {
            return WithKeySerializer(serializer.Serialize, serializer.Deserialize);
        }

        public TConfig WithKeySerializer(ISerializer<TKOuter2> serializer)
        {
            return WithKeySerializer(serializer.Serialize, serializer.Deserialize);
        }

        public TConfig WithKeySerializer(ISerializer<TKOuter3> serializer)
        {
            return WithKeySerializer(serializer.Serialize, serializer.Deserialize);
        }

        public TConfig WithKeySerializer(ISerializer<TKInner> serializer)
        {
            return WithKeySerializer(serializer.Serialize, serializer.Deserialize);
        }

        public TConfig WithKeySerializer(Func<TKOuter1, string> serializer, Func<string, TKOuter1> deserializer = null)
        {
            return WithKeySerializerInternal(serializer, deserializer);
        }

        public TConfig WithKeySerializer(Func<TKOuter2, string> serializer, Func<string, TKOuter2> deserializer = null)
        {
            return WithKeySerializerInternal(serializer, deserializer);
        }
        
        public TConfig WithKeySerializer(Func<TKOuter3, string> serializer, Func<string, TKOuter3> deserializer = null)
        {
            return WithKeySerializerInternal(serializer, deserializer);
        }

        public TConfig WithKeySerializer(Func<TKInner, string> serializer, Func<string, TKInner> deserializer = null)
        {
            return WithKeySerializerInternal(serializer, deserializer);
        }

        public TConfig WithKeySerializer<T>(ISerializer serializer)
        {
            var type = typeof(T);
            var match = false;
            if (typeof(TKOuter1) == type)
            {
                WithKeySerializerInternal(serializer.Serialize, serializer.Deserialize<TKOuter1>);
                match = true;
            }

            if (typeof(TKOuter2) == type)
            {
                WithKeySerializerInternal(serializer.Serialize, serializer.Deserialize<TKOuter2>);
                match = true;
            }

            if (typeof(TKOuter3) == type)
            {
                WithKeySerializerInternal(serializer.Serialize, serializer.Deserialize<TKOuter3>);
                match = true;
            }

            if (typeof(TKInner) == type)
            {
                WithKeySerializerInternal(serializer.Serialize, serializer.Deserialize<TKInner>);
                match = true;
            }

            if (!match)
                throw new InvalidOperationException($"Cannot use '{typeof(T).Name}' as the type argument in {this.GetType().Name}.{nameof(WithKeySerializer)} as no keys are of that type");

            return (TConfig)this;
        }

        public TConfig WithKeyComparer(IEqualityComparer<TKOuter1> comparer)
        {
            return WithKeyComparerInternal(comparer);
        }

        public TConfig WithKeyComparer(IEqualityComparer<TKOuter2> comparer)
        {
            return WithKeyComparerInternal(comparer);
        }

        public TConfig WithKeyComparer(IEqualityComparer<TKInner> comparer)
        {
            return WithKeyComparerInternal(comparer);
        }
        
        public TConfig SkipCacheWhen(Func<TKOuter1, TKOuter2, TKOuter3, TKInner, bool> predicate, SkipCacheSettings settings = SkipCacheSettings.SkipGetAndSet)
        {
            return base.SkipCacheWhen(x => predicate(x.Item1.Item1, x.Item1.Item2, x.Item1.Item3, x.Item2), settings);
        }

        public TConfig SkipCacheWhen(Func<TKOuter1, TKOuter2, TKOuter3, bool> predicate, SkipCacheSettings settings = SkipCacheSettings.SkipGetAndSet)
        {
            return base.SkipCacheWhen(predicate.ConvertToSingleParamNoCanx(), settings);
        }

        public TConfig ExcludeParametersFromKey(params int[] parameterIndexes)
        {
            return ExcludeParametersFromKeyImpl(parameterIndexes, 4);
        }
        
        public TConfig WithReturnDictionaryBuilder(IReturnDictionaryBuilder<TKInner, TV, TRes> builder)
        {
            return WithReturnDictionaryBuilder(c => builder);
        }
        
        public TConfig WithReturnDictionaryBuilder(Func<IEqualityComparer<TKInner>, IReturnDictionaryBuilder<TKInner, TV, TRes>> builder)
        {
            ReturnDictionaryBuilderFunc = builder;
            return (TConfig)this;
        }
    }
    
    public sealed class MultiParamEnumerableKeyFunctionCacheConfigurationManagerNoCanx<TKOuter1, TKOuter2, TKOuter3, TKInnerEnumerable, TRes, TKInner, TV>
        : MultiParamEnumerableKeyFunctionCacheConfigurationManager<MultiParamEnumerableKeyFunctionCacheConfigurationManagerNoCanx<TKOuter1, TKOuter2, TKOuter3, TKInnerEnumerable, TRes, TKInner, TV>, TKOuter1, TKOuter2, TKOuter3, TKInnerEnumerable, TRes, TKInner, TV>
        where TKInnerEnumerable : IEnumerable<TKInner>
        where TRes : IDictionary<TKInner, TV>
    {
        internal MultiParamEnumerableKeyFunctionCacheConfigurationManagerNoCanx(
            Func<TKOuter1, TKOuter2, TKOuter3, TKInnerEnumerable, Task<TRes>> inputFunc)
            : base(inputFunc.AppearCancellable())
        { }
        
        internal MultiParamEnumerableKeyFunctionCacheConfigurationManagerNoCanx(
            Func<TKOuter1, TKOuter2, TKOuter3, TKInnerEnumerable, Task<TRes>> inputFunc,
            CachedProxyConfig interfaceConfig,
            MethodInfo methodInfo)
            : base(inputFunc.AppearCancellable(), interfaceConfig, methodInfo)
        { }
        
        public Func<TKOuter1, TKOuter2, TKOuter3, TKInnerEnumerable, Task<TRes>> Build()
        {
            var functionCache = BuildMultiParamEnumerableKeyFunctionCache(
                TupleKeyHelper.BuildKeyComparer<TKOuter1, TKOuter2, TKOuter3>(KeyComparers, ParametersToExcludeFromKey),
                TupleKeyHelper.BuildKeySerializer<TKOuter1, TKOuter2, TKOuter3>(KeySerializers, KeyParamSeparator, ParametersToExcludeFromKey));

            var keyComparer = KeyComparerResolver.GetInner<TKInner>();

            var returnDictionaryBuilder = ReturnDictionaryBuilderFunc == null
                ? ReturnDictionaryBuilderResolver.Get<TRes, TKInner, TV>(keyComparer)
                : ReturnDictionaryBuilderFunc(keyComparer);

            Func<(TKOuter1, TKOuter2, TKOuter3), IEnumerable<TKInner>, CancellationToken, Task<IDictionary<TKInner, TV>>> func = GetResults;

            return func
                .ConvertInputFromEnumerable<(TKOuter1, TKOuter2, TKOuter3), TKInnerEnumerable, IDictionary<TKInner, TV>, TKInner, TV>()
                .ConvertOutputFromDictionary<(TKOuter1, TKOuter2, TKOuter3), TKInnerEnumerable, TRes, TKInner, TV>()
                .ConvertFunc()
                .MakeNonCancellable();

            async Task<IDictionary<TKInner, TV>> GetResults(
                (TKOuter1, TKOuter2, TKOuter3) outerKeys,
                IEnumerable<TKInner> innerKeys,
                CancellationToken token)
            {
                var results = await functionCache.GetMulti(outerKeys, innerKeys, token);

                return returnDictionaryBuilder.BuildResponse(
                    results.Select(r => (IKeyValuePair<TKInner, TV>)new KeyValuePairInternal<TKInner, TV>(r.Key.Item2, r.Value)),
                    results.Count);
            }
        }
    }
    
    public sealed class MultiParamEnumerableKeyFunctionCacheConfigurationManagerCanx<TKOuter1, TKOuter2, TKOuter3, TKInnerEnumerable, TRes, TKInner, TV>
        : MultiParamEnumerableKeyFunctionCacheConfigurationManager<MultiParamEnumerableKeyFunctionCacheConfigurationManagerCanx<TKOuter1, TKOuter2, TKOuter3, TKInnerEnumerable, TRes, TKInner, TV>, TKOuter1, TKOuter2, TKOuter3, TKInnerEnumerable, TRes, TKInner, TV>
        where TKInnerEnumerable : IEnumerable<TKInner>
        where TRes : IDictionary<TKInner, TV>
    {
        internal MultiParamEnumerableKeyFunctionCacheConfigurationManagerCanx(
            Func<TKOuter1, TKOuter2, TKOuter3, TKInnerEnumerable, CancellationToken, Task<TRes>> inputFunc)
            : base(inputFunc)
        { }
        
        internal MultiParamEnumerableKeyFunctionCacheConfigurationManagerCanx(
            Func<TKOuter1, TKOuter2, TKOuter3, TKInnerEnumerable, CancellationToken, Task<TRes>> inputFunc,
            CachedProxyConfig interfaceConfig,
            MethodInfo methodInfo)
            : base(inputFunc, interfaceConfig, methodInfo)
        { }
        
        public Func<TKOuter1, TKOuter2, TKOuter3, TKInnerEnumerable, CancellationToken, Task<TRes>> Build()
        {
            var functionCache = BuildMultiParamEnumerableKeyFunctionCache(
                TupleKeyHelper.BuildKeyComparer<TKOuter1, TKOuter2, TKOuter3>(KeyComparers, ParametersToExcludeFromKey),
                TupleKeyHelper.BuildKeySerializer<TKOuter1, TKOuter2, TKOuter3>(KeySerializers, KeyParamSeparator, ParametersToExcludeFromKey));

            var keyComparer = KeyComparerResolver.GetInner<TKInner>();

            var returnDictionaryBuilder = ReturnDictionaryBuilderFunc == null
                ? ReturnDictionaryBuilderResolver.Get<TRes, TKInner, TV>(keyComparer)
                : ReturnDictionaryBuilderFunc(keyComparer);

            Func<(TKOuter1, TKOuter2, TKOuter3), IEnumerable<TKInner>, CancellationToken, Task<IDictionary<TKInner, TV>>> func = GetResults;

            return func
                .ConvertInputFromEnumerable<(TKOuter1, TKOuter2, TKOuter3), TKInnerEnumerable, IDictionary<TKInner, TV>, TKInner, TV>()
                .ConvertOutputFromDictionary<(TKOuter1, TKOuter2, TKOuter3), TKInnerEnumerable, TRes, TKInner, TV>()
                .ConvertFunc();

            async Task<IDictionary<TKInner, TV>> GetResults(
                (TKOuter1, TKOuter2, TKOuter3) outerKeys,
                IEnumerable<TKInner> innerKeys,
                CancellationToken token)
            {
                var results = await functionCache.GetMulti(outerKeys, innerKeys, token);

                return returnDictionaryBuilder.BuildResponse(
                    results.Select(r => (IKeyValuePair<TKInner, TV>)new KeyValuePairInternal<TKInner, TV>(r.Key.Item2, r.Value)),
                    results.Count);
            }
        }
    }
}