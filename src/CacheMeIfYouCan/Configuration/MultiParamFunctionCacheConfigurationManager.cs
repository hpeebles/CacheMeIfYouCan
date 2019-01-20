using System;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
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

        protected override Func<(TK1, TK2), string> GetKeySerializer()
        {
            var key1Serializer = GetKeySerializerImpl<TK1>();
            var key2Serializer = GetKeySerializerImpl<TK2>();

            return x => key1Serializer(x.Item1) + _keyParamSeparator + key2Serializer(x.Item2);
        }
        
        protected override Func<string, (TK1, TK2)> GetKeyDeserializer()
        {
            var key1Deserializer = GetKeyDeserializerImpl<TK1>();
            var key2Deserializer = GetKeyDeserializerImpl<TK2>();

            var separator = _keyParamSeparator;
            
            return Deserialize;

            (TK1, TK2) Deserialize(string str)
            {
                var index = str.IndexOf(separator, StringComparison.Ordinal);

                var k1 = str.Substring(0, index - 1);
                var k2 = str.Substring(index + separator.Length);

                return (key1Deserializer(k1), key2Deserializer(k2));
            }
        }
        
        public MultiParamFunctionCacheConfigurationManager<TK1, TK2, TV> WithTimeToLiveFactory(
            Func<TK1, TK2, TV, TimeSpan> timeToLiveFactory)
        {
            return base.WithTimeToLiveFactory((k, v) => timeToLiveFactory(k.Item1, k.Item2, v));
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
        
        protected override Func<(TK1, TK2, TK3), string> GetKeySerializer()
        {
            var key1Serializer = GetKeySerializerImpl<TK1>();
            var key2Serializer = GetKeySerializerImpl<TK2>();
            var key3Serializer = GetKeySerializerImpl<TK3>();

            return x =>
                key1Serializer(x.Item1) +
                _keyParamSeparator +
                key2Serializer(x.Item2) +
                _keyParamSeparator +
                key3Serializer(x.Item3);
        }
        
        protected override Func<string, (TK1, TK2, TK3)> GetKeyDeserializer()
        {
            var key1Deserializer = GetKeyDeserializerImpl<TK1>();
            var key2Deserializer = GetKeyDeserializerImpl<TK2>();
            var key3Deserializer = GetKeyDeserializerImpl<TK3>();

            var separator = _keyParamSeparator;
            
            return Deserialize;

            (TK1, TK2, TK3) Deserialize(string str)
            {
                var parts = str.Split(new[] {separator}, 2, StringSplitOptions.None);
                
                return (key1Deserializer(parts[0]), key2Deserializer(parts[1]), key3Deserializer(parts[2]));
            }
        }
        
        public MultiParamFunctionCacheConfigurationManager<TK1, TK2, TK3, TV> WithTimeToLiveFactory(
            Func<TK1, TK2, TK3, TV, TimeSpan> timeToLiveFactory)
        {
            return base.WithTimeToLiveFactory((k, v) => timeToLiveFactory(k.Item1, k.Item2, k.Item3, v));
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
        
        protected override Func<(TK1, TK2, TK3, TK4), string> GetKeySerializer()
        {
            var key1Serializer = GetKeySerializerImpl<TK1>();
            var key2Serializer = GetKeySerializerImpl<TK2>();
            var key3Serializer = GetKeySerializerImpl<TK3>();
            var key4Serializer = GetKeySerializerImpl<TK4>();

            return x =>
                key1Serializer(x.Item1) +
                _keyParamSeparator +
                key2Serializer(x.Item2) +
                _keyParamSeparator +
                key3Serializer(x.Item3) +
                _keyParamSeparator +
                key4Serializer(x.Item4);
        }
        
        protected override Func<string, (TK1, TK2, TK3, TK4)> GetKeyDeserializer()
        {
            var key1Deserializer = GetKeyDeserializerImpl<TK1>();
            var key2Deserializer = GetKeyDeserializerImpl<TK2>();
            var key3Deserializer = GetKeyDeserializerImpl<TK3>();
            var key4Deserializer = GetKeyDeserializerImpl<TK4>();

            var separator = _keyParamSeparator;
            
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

        public MultiParamFunctionCacheConfigurationManager<TK1, TK2, TK3, TK4, TV> WithTimeToLiveFactory(
            Func<TK1, TK2, TK3, TK4, TV, TimeSpan> timeToLiveFactory)
        {
            return base.WithTimeToLiveFactory((k, v) => timeToLiveFactory(k.Item1, k.Item2, k.Item3, k.Item4, v));
        }

        public Func<TK1, TK2, TK3, TK4, Task<TV>> Build()
        {
            var functionCache = BuildFunctionCacheSingle();

            Func<(TK1, TK2, TK3, TK4), Task<TV>> func = functionCache.Get;

            return func.ConvertToMultiParam();
        }
    }
}