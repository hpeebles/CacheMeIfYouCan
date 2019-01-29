using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using CacheMeIfYouCan.Internal;
using CacheMeIfYouCan.Serializers;

namespace CacheMeIfYouCan.Configuration
{
    public sealed class MultiParamFunctionCacheConfigurationManager<TK1, TK2, TV>
        : MultiParamFunctionCacheConfigurationManagerBase<MultiParamFunctionCacheConfigurationManager<TK1, TK2, TV>, (TK1, TK2), TV>
    {
        internal MultiParamFunctionCacheConfigurationManager(
            Func<TK1, TK2, Task<TV>> inputFunc,
            string functionName)
            : base(inputFunc.ConvertToSingleParam(), functionName)
        { }
        
        internal MultiParamFunctionCacheConfigurationManager(
            Func<TK1, TK2, Task<TV>> inputFunc,
            CachedProxyConfig interfaceConfig,
            MethodInfo methodInfo)
            : base(
                inputFunc.ConvertToSingleParam(),
                interfaceConfig,
                methodInfo)
        { }

        public new MultiParamFunctionCacheConfigurationManager<TK1, TK2, TV> WithKeySerializer(ISerializer serializer)
        {
            return base
                .WithKeySerializerInternal(serializer.Serialize, serializer.Deserialize<TK1>)
                .WithKeySerializerInternal(serializer.Serialize, serializer.Deserialize<TK2>);
        }
        
        public MultiParamFunctionCacheConfigurationManager<TK1, TK2, TV> WithKeySerializer(ISerializer<TK1> serializer)
        {
            return WithKeySerializer(serializer.Serialize, serializer.Deserialize);
        }
        
        public MultiParamFunctionCacheConfigurationManager<TK1, TK2, TV> WithKeySerializer(ISerializer<TK2> serializer)
        {
            return WithKeySerializer(serializer.Serialize, serializer.Deserialize);
        }
        
        public MultiParamFunctionCacheConfigurationManager<TK1, TK2, TV> WithKeySerializer(Func<TK1, string> serializer, Func<string, TK1> deserializer = null)
        {
            return WithKeySerializerInternal(serializer, deserializer);
        }
        
        public MultiParamFunctionCacheConfigurationManager<TK1, TK2, TV> WithKeySerializer(Func<TK2, string> serializer, Func<string, TK2> deserializer = null)
        {
            return WithKeySerializerInternal(serializer, deserializer);
        }

        internal override Func<(TK1, TK2), string> GetKeySerializer()
        {
            return TupleKeyHelper.BuildKeySerializer<TK1, TK2>(KeySerializers, KeyParamSeparator);
        }
        
        internal override Func<string, (TK1, TK2)> GetKeyDeserializer()
        {
            return TupleKeyHelper.BuildKeyDeserializer<TK1, TK2>(KeySerializers, KeyParamSeparator);
        }
        
        public MultiParamFunctionCacheConfigurationManager<TK1, TK2, TV> WithKeyComparer(
            IEqualityComparer<TK1> comparer)
        {
            return WithKeyComparerInternal(comparer);
        }
        
        public MultiParamFunctionCacheConfigurationManager<TK1, TK2, TV> WithKeyComparer(
            IEqualityComparer<TK2> comparer)
        {
            return WithKeyComparerInternal(comparer);
        }
        
        public MultiParamFunctionCacheConfigurationManager<TK1, TK2, TV> WithTimeToLiveFactory(
            Func<TK1, TK2, TV, TimeSpan> timeToLiveFactory)
        {
            return WithTimeToLiveFactory((k, v) => timeToLiveFactory(k.Item1, k.Item2, v));
        }
        
        internal override KeyComparer<(TK1, TK2)> GetKeyComparer()
        {
            return TupleKeyHelper.BuildKeyComparer<TK1, TK2>(KeyComparers);
        }

        public Func<TK1, TK2, Task<TV>> Build()
        {
            var functionCache = BuildFunctionCacheSingle();
            
            Func<(TK1, TK2), Task<TV>> func = functionCache.Get;

            return func.ConvertToMultiParam();
        }
    }
    
    public sealed class MultiParamFunctionCacheConfigurationManager<TK1, TK2, TK3, TV>
        : MultiParamFunctionCacheConfigurationManagerBase<MultiParamFunctionCacheConfigurationManager<TK1, TK2, TK3, TV>, (TK1, TK2, TK3), TV>
    {
        internal MultiParamFunctionCacheConfigurationManager(
            Func<TK1, TK2, TK3, Task<TV>> inputFunc,
            string functionName)
            : base(inputFunc.ConvertToSingleParam(), functionName)
        { }
        
        internal MultiParamFunctionCacheConfigurationManager(
            Func<TK1, TK2, TK3, Task<TV>> inputFunc,
            CachedProxyConfig interfaceConfig,
            MethodInfo methodInfo)
            : base(
                inputFunc.ConvertToSingleParam(),
                interfaceConfig,
                methodInfo)
        { }
        
        public new MultiParamFunctionCacheConfigurationManager<TK1, TK2, TK3, TV> WithKeySerializer(ISerializer serializer)
        {
            return base
                .WithKeySerializerInternal(serializer.Serialize, serializer.Deserialize<TK1>)
                .WithKeySerializerInternal(serializer.Serialize, serializer.Deserialize<TK2>)
                .WithKeySerializerInternal(serializer.Serialize, serializer.Deserialize<TK3>);
        }
        
        public MultiParamFunctionCacheConfigurationManager<TK1, TK2, TK3, TV> WithKeySerializer(
            ISerializer<TK1> serializer)
        {
            return WithKeySerializer(serializer.Serialize, serializer.Deserialize);
        }
        
        public MultiParamFunctionCacheConfigurationManager<TK1, TK2, TK3, TV> WithKeySerializer(
            ISerializer<TK2> serializer)
        {
            return WithKeySerializer(serializer.Serialize, serializer.Deserialize);
        }
        
        public MultiParamFunctionCacheConfigurationManager<TK1, TK2, TK3, TV> WithKeySerializer(
            ISerializer<TK3> serializer)
        {
            return WithKeySerializer(serializer.Serialize, serializer.Deserialize);
        }
        
        public MultiParamFunctionCacheConfigurationManager<TK1, TK2, TK3, TV> WithKeySerializer(
            Func<TK1, string> serializer, Func<string, TK1> deserializer = null)
        {
            return WithKeySerializerInternal(serializer, deserializer);
        }
        
        public MultiParamFunctionCacheConfigurationManager<TK1, TK2, TK3, TV> WithKeySerializer(
            Func<TK2, string> serializer, Func<string, TK2> deserializer = null)
        {
            return WithKeySerializerInternal(serializer, deserializer);
        }

        public MultiParamFunctionCacheConfigurationManager<TK1, TK2, TK3, TV> WithKeySerializer(
            Func<TK3, string> serializer, Func<string, TK3> deserializer = null)
        {
            return WithKeySerializerInternal(serializer, deserializer);
        }
        
        internal override Func<(TK1, TK2, TK3), string> GetKeySerializer()
        {
            return TupleKeyHelper.BuildKeySerializer<TK1, TK2, TK3>(KeySerializers, KeyParamSeparator);
        }
        
        internal override Func<string, (TK1, TK2, TK3)> GetKeyDeserializer()
        {
            return TupleKeyHelper.BuildKeyDeserializer<TK1, TK2, TK3>(KeySerializers, KeyParamSeparator);
        }
        
        public MultiParamFunctionCacheConfigurationManager<TK1, TK2, TK3, TV> WithKeyComparer(
            IEqualityComparer<TK1> comparer)
        {
            return WithKeyComparerInternal(comparer);
        }
        
        public MultiParamFunctionCacheConfigurationManager<TK1, TK2, TK3, TV> WithKeyComparer(
            IEqualityComparer<TK2> comparer)
        {
            return WithKeyComparerInternal(comparer);
        }
        
        public MultiParamFunctionCacheConfigurationManager<TK1, TK2, TK3, TV> WithKeyComparer(
            IEqualityComparer<TK3> comparer)
        {
            return WithKeyComparerInternal(comparer);
        }
        
        public MultiParamFunctionCacheConfigurationManager<TK1, TK2, TK3, TV> WithTimeToLiveFactory(
            Func<TK1, TK2, TK3, TV, TimeSpan> timeToLiveFactory)
        {
            return WithTimeToLiveFactory((k, v) => timeToLiveFactory(k.Item1, k.Item2, k.Item3, v));
        }
        
        internal override KeyComparer<(TK1, TK2, TK3)> GetKeyComparer()
        {
            return TupleKeyHelper.BuildKeyComparer<TK1, TK2, TK3>(KeyComparers);
        }

        public Func<TK1, TK2, TK3, Task<TV>> Build()
        {
            var functionCache = BuildFunctionCacheSingle();

            Func<(TK1, TK2, TK3), Task<TV>> func = functionCache.Get;

            return func.ConvertToMultiParam();
        }
    }
    
    public sealed class MultiParamFunctionCacheConfigurationManager<TK1, TK2, TK3, TK4, TV>
        : MultiParamFunctionCacheConfigurationManagerBase<MultiParamFunctionCacheConfigurationManager<TK1, TK2, TK3, TK4, TV>, (TK1, TK2, TK3, TK4), TV>
    {
        internal MultiParamFunctionCacheConfigurationManager(
            Func<TK1, TK2, TK3, TK4, Task<TV>> inputFunc,
            string functionName)
            : base(inputFunc.ConvertToSingleParam(), functionName)
        { }
        
        internal MultiParamFunctionCacheConfigurationManager(
            Func<TK1, TK2, TK3, TK4, Task<TV>> inputFunc,
            CachedProxyConfig interfaceConfig,
            MethodInfo methodInfo)
            : base(
                inputFunc.ConvertToSingleParam(),
                interfaceConfig,
                methodInfo)
        { }
        
        public new MultiParamFunctionCacheConfigurationManager<TK1, TK2, TK3, TK4, TV> WithKeySerializer(
            ISerializer serializer)
        {
            return base
                .WithKeySerializerInternal(serializer.Serialize, serializer.Deserialize<TK1>)
                .WithKeySerializerInternal(serializer.Serialize, serializer.Deserialize<TK2>)
                .WithKeySerializerInternal(serializer.Serialize, serializer.Deserialize<TK3>)
                .WithKeySerializerInternal(serializer.Serialize, serializer.Deserialize<TK4>);
        }
        
        public MultiParamFunctionCacheConfigurationManager<TK1, TK2, TK3, TK4, TV> WithKeySerializer(
            ISerializer<TK1> serializer)
        {
            return WithKeySerializer(serializer.Serialize, serializer.Deserialize);
        }
        
        public MultiParamFunctionCacheConfigurationManager<TK1, TK2, TK3, TK4, TV> WithKeySerializer(
            ISerializer<TK2> serializer)
        {
            return WithKeySerializer(serializer.Serialize, serializer.Deserialize);
        }
        
        public MultiParamFunctionCacheConfigurationManager<TK1, TK2, TK3, TK4, TV> WithKeySerializer(
            ISerializer<TK3> serializer)
        {
            return WithKeySerializer(serializer.Serialize, serializer.Deserialize);
        }
        
        public MultiParamFunctionCacheConfigurationManager<TK1, TK2, TK3, TK4, TV> WithKeySerializer(
            ISerializer<TK4> serializer)
        {
            return WithKeySerializer(serializer.Serialize, serializer.Deserialize);
        }
        
        public MultiParamFunctionCacheConfigurationManager<TK1, TK2, TK3, TK4, TV> WithKeySerializer(
            Func<TK1, string> serializer, Func<string, TK1> deserializer = null)
        {
            return WithKeySerializerInternal(serializer, deserializer);
        }
        
        public MultiParamFunctionCacheConfigurationManager<TK1, TK2, TK3, TK4, TV> WithKeySerializer(
            Func<TK2, string> serializer, Func<string, TK2> deserializer = null)
        {
            return WithKeySerializerInternal(serializer, deserializer);
        }

        public MultiParamFunctionCacheConfigurationManager<TK1, TK2, TK3, TK4, TV> WithKeySerializer(
            Func<TK3, string> serializer, Func<string, TK3> deserializer = null)
        {
            return WithKeySerializerInternal(serializer, deserializer);
        }
        
        public MultiParamFunctionCacheConfigurationManager<TK1, TK2, TK3, TK4, TV> WithKeySerializer(
            Func<TK4, string> serializer, Func<string, TK4> deserializer = null)
        {
            return WithKeySerializerInternal(serializer, deserializer);
        }
        
        internal override Func<(TK1, TK2, TK3, TK4), string> GetKeySerializer()
        {
            return TupleKeyHelper.BuildKeySerializer<TK1, TK2, TK3, TK4>(KeySerializers, KeyParamSeparator);
        }
        
        internal override Func<string, (TK1, TK2, TK3, TK4)> GetKeyDeserializer()
        {
            return TupleKeyHelper.BuildKeyDeserializer<TK1, TK2, TK3, TK4>(KeySerializers, KeyParamSeparator);
        }
        
        public MultiParamFunctionCacheConfigurationManager<TK1, TK2, TK3, TK4, TV> WithKeyComparer(
            IEqualityComparer<TK1> comparer)
        {
            return WithKeyComparerInternal(comparer);
        }
        
        public MultiParamFunctionCacheConfigurationManager<TK1, TK2, TK3, TK4, TV> WithKeyComparer(
            IEqualityComparer<TK2> comparer)
        {
            return WithKeyComparerInternal(comparer);
        }
        
        public MultiParamFunctionCacheConfigurationManager<TK1, TK2, TK3, TK4, TV> WithKeyComparer(
            IEqualityComparer<TK3> comparer)
        {
            return WithKeyComparerInternal(comparer);
        }
        
        public MultiParamFunctionCacheConfigurationManager<TK1, TK2, TK3, TK4, TV> WithKeyComparer(
            IEqualityComparer<TK4> comparer)
        {
            return WithKeyComparerInternal(comparer);
        }

        public MultiParamFunctionCacheConfigurationManager<TK1, TK2, TK3, TK4, TV> WithTimeToLiveFactory(
            Func<TK1, TK2, TK3, TK4, TV, TimeSpan> timeToLiveFactory)
        {
            return WithTimeToLiveFactory((k, v) => timeToLiveFactory(k.Item1, k.Item2, k.Item3, k.Item4, v));
        }
        
        internal override KeyComparer<(TK1, TK2, TK3, TK4)> GetKeyComparer()
        {
            return TupleKeyHelper.BuildKeyComparer<TK1, TK2, TK3, TK4>(KeyComparers);
        }

        public Func<TK1, TK2, TK3, TK4, Task<TV>> Build()
        {
            var functionCache = BuildFunctionCacheSingle();

            Func<(TK1, TK2, TK3, TK4), Task<TV>> func = functionCache.Get;

            return func.ConvertToMultiParam();
        }
    }
}