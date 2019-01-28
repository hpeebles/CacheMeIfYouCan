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
            var key1Serializer = GetKeySerializerImpl<TK1>();
            var key2Serializer = GetKeySerializerImpl<TK2>();

            return x => key1Serializer(x.Item1) + KeyParamSeparator + key2Serializer(x.Item2);
        }
        
        internal override Func<string, (TK1, TK2)> GetKeyDeserializer()
        {
            var key1Deserializer = GetKeyDeserializerImpl<TK1>();
            var key2Deserializer = GetKeyDeserializerImpl<TK2>();

            var separator = KeyParamSeparator;
            
            return Deserialize;

            (TK1, TK2) Deserialize(string str)
            {
                var index = str.IndexOf(separator, StringComparison.Ordinal);

                var k1 = str.Substring(0, index - 1);
                var k2 = str.Substring(index + separator.Length);

                return (key1Deserializer(k1), key2Deserializer(k2));
            }
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
            var comparer1 = KeyComparerResolver.GetInner<TK1>(KeyComparers);
            var comparer2 = KeyComparerResolver.GetInner<TK2>(KeyComparers);
            
            var comparer = new ValueTupleComparer<TK1, TK2>(comparer1, comparer2);
            
            return new KeyComparer<(TK1, TK2)>(comparer);
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
            var key1Serializer = GetKeySerializerImpl<TK1>();
            var key2Serializer = GetKeySerializerImpl<TK2>();
            var key3Serializer = GetKeySerializerImpl<TK3>();

            return x =>
                key1Serializer(x.Item1) +
                KeyParamSeparator +
                key2Serializer(x.Item2) +
                KeyParamSeparator +
                key3Serializer(x.Item3);
        }
        
        internal override Func<string, (TK1, TK2, TK3)> GetKeyDeserializer()
        {
            var key1Deserializer = GetKeyDeserializerImpl<TK1>();
            var key2Deserializer = GetKeyDeserializerImpl<TK2>();
            var key3Deserializer = GetKeyDeserializerImpl<TK3>();

            var separator = KeyParamSeparator;
            
            return Deserialize;

            (TK1, TK2, TK3) Deserialize(string str)
            {
                var parts = str.Split(new[] {separator}, 2, StringSplitOptions.None);
                
                return (key1Deserializer(parts[0]), key2Deserializer(parts[1]), key3Deserializer(parts[2]));
            }
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
            var comparer1 = KeyComparerResolver.GetInner<TK1>(KeyComparers);
            var comparer2 = KeyComparerResolver.GetInner<TK2>(KeyComparers);
            var comparer3 = KeyComparerResolver.GetInner<TK3>(KeyComparers);
            
            var comparer = new ValueTupleComparer<TK1, TK2, TK3>(comparer1, comparer2, comparer3);
            
            return new KeyComparer<(TK1, TK2, TK3)>(comparer);
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
            var key1Serializer = GetKeySerializerImpl<TK1>();
            var key2Serializer = GetKeySerializerImpl<TK2>();
            var key3Serializer = GetKeySerializerImpl<TK3>();
            var key4Serializer = GetKeySerializerImpl<TK4>();

            return x =>
                key1Serializer(x.Item1) +
                KeyParamSeparator +
                key2Serializer(x.Item2) +
                KeyParamSeparator +
                key3Serializer(x.Item3) +
                KeyParamSeparator +
                key4Serializer(x.Item4);
        }
        
        internal override Func<string, (TK1, TK2, TK3, TK4)> GetKeyDeserializer()
        {
            var key1Deserializer = GetKeyDeserializerImpl<TK1>();
            var key2Deserializer = GetKeyDeserializerImpl<TK2>();
            var key3Deserializer = GetKeyDeserializerImpl<TK3>();
            var key4Deserializer = GetKeyDeserializerImpl<TK4>();

            var separator = KeyParamSeparator;
            
            return Deserialize;

            (TK1, TK2, TK3, TK4) Deserialize(string str)
            {
                var parts = str.Split(new[] { separator }, 4, StringSplitOptions.None);
                
                return (
                    key1Deserializer(parts[0]),
                    key2Deserializer(parts[1]),
                    key3Deserializer(parts[2]),
                    key4Deserializer(parts[3]));
            }
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
            var comparer1 = KeyComparerResolver.GetInner<TK1>(KeyComparers);
            var comparer2 = KeyComparerResolver.GetInner<TK2>(KeyComparers);
            var comparer3 = KeyComparerResolver.GetInner<TK3>(KeyComparers);
            var comparer4 = KeyComparerResolver.GetInner<TK4>(KeyComparers);
            
            var comparer = new ValueTupleComparer<TK1, TK2, TK3, TK4>(comparer1, comparer2, comparer3, comparer4);
            
            return new KeyComparer<(TK1, TK2, TK3, TK4)>(comparer);
        }

        public Func<TK1, TK2, TK3, TK4, Task<TV>> Build()
        {
            var functionCache = BuildFunctionCacheSingle();

            Func<(TK1, TK2, TK3, TK4), Task<TV>> func = functionCache.Get;

            return func.ConvertToMultiParam();
        }
    }
}