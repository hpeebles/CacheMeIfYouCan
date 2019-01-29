using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using CacheMeIfYouCan.Internal;
using CacheMeIfYouCan.Serializers;

namespace CacheMeIfYouCan.Configuration
{
    public sealed class MultiParamEnumerableKeyFunctionCacheConfigurationManagerSync<TKOuter, TKInnerEnumerable, TRes, TKInner, TV>
        : MultiParamEnumerableKeyFunctionCacheConfigurationManagerBase<MultiParamEnumerableKeyFunctionCacheConfigurationManagerSync<TKOuter, TKInnerEnumerable, TRes, TKInner, TV>, TKOuter, TKInner, TV>
        where TKInnerEnumerable : IEnumerable<TKInner>
        where TRes : IDictionary<TKInner, TV>
    {
        internal MultiParamEnumerableKeyFunctionCacheConfigurationManagerSync(
            Func<TKOuter, TKInnerEnumerable, TRes> inputFunc,
            string functionName)
            : base(
                inputFunc
                    .ConvertToAsync()
                    .ConvertInputToEnumerable<TKOuter, TKInnerEnumerable, TRes, TKInner, TV>()
                    .ConvertOutputToDictionary<TKOuter, IEnumerable<TKInner>, TRes, TKInner, TV>(),
                functionName)
        { }

        internal MultiParamEnumerableKeyFunctionCacheConfigurationManagerSync(
            Func<TKOuter, TKInnerEnumerable, TRes> inputFunc,
            CachedProxyConfig interfaceConfig,
            MethodInfo methodInfo)
            : base(
                inputFunc
                    .ConvertToAsync()
                    .ConvertInputToEnumerable<TKOuter, TKInnerEnumerable, TRes, TKInner, TV>()
                    .ConvertOutputToDictionary<TKOuter, IEnumerable<TKInner>, TRes, TKInner, TV>(),
                interfaceConfig,
                methodInfo)
        { }
        
        public new MultiParamEnumerableKeyFunctionCacheConfigurationManagerSync<TKOuter, TKInnerEnumerable, TRes, TKInner, TV> WithKeySerializer(
            ISerializer serializer)
        {
            WithKeySerializerInternal(serializer.Serialize, serializer.Deserialize<TKOuter>);
            WithKeySerializerInternal(serializer.Serialize, serializer.Deserialize<TKInner>);
            return this;
        }
        
        public MultiParamEnumerableKeyFunctionCacheConfigurationManagerSync<TKOuter, TKInnerEnumerable, TRes, TKInner, TV> WithKeySerializer(
            ISerializer<TKOuter> serializer)
        {
            return WithKeySerializer(serializer.Serialize, serializer.Deserialize);
        }
        
        public MultiParamEnumerableKeyFunctionCacheConfigurationManagerSync<TKOuter, TKInnerEnumerable, TRes, TKInner, TV> WithKeySerializer(
            ISerializer<TKInner> serializer)
        {
            return WithKeySerializer(serializer.Serialize, serializer.Deserialize);
        }
        
        public MultiParamEnumerableKeyFunctionCacheConfigurationManagerSync<TKOuter, TKInnerEnumerable, TRes, TKInner, TV> WithKeySerializer(
            Func<TKOuter, string> serializer, Func<string, TKOuter> deserializer = null)
        {
            return WithKeySerializerInternal(serializer, deserializer);
        }
        
        public MultiParamEnumerableKeyFunctionCacheConfigurationManagerSync<TKOuter, TKInnerEnumerable, TRes, TKInner, TV> WithKeySerializer(
            Func<TKInner, string> serializer, Func<string, TKInner> deserializer = null)
        {
            return WithKeySerializerInternal(serializer, deserializer);
        }

        public MultiParamEnumerableKeyFunctionCacheConfigurationManagerSync<TKOuter, TKInnerEnumerable, TRes, TKInner, TV> WithKeyComparer(
            IEqualityComparer<TKOuter> comparer)
        {
            return WithKeyComparerInternal(comparer);
        }
        
        public MultiParamEnumerableKeyFunctionCacheConfigurationManagerSync<TKOuter, TKInnerEnumerable, TRes, TKInner, TV> WithKeyComparer(
            IEqualityComparer<TKInner> comparer)
        {
            return WithKeyComparerInternal(comparer);
        }
        
        public Func<TKOuter, TKInnerEnumerable, TRes> Build()
        {
            var functionCache = BuildMultiParamEnumerableKeyFunctionCache();

            var dictionaryFactoryFunc = DictionaryFactoryFunc ?? DictionaryFactoryFuncResolver.Get<TRes, TKInner, TV>();

            var keyComparer = KeyComparerResolver.GetInner<TKInner>();
            
            Func<TKOuter, IEnumerable<TKInner>, Task<IDictionary<TKInner, TV>>> func = GetResults;
            
            return func
                .ConvertInputFromEnumerable<TKOuter, TKInnerEnumerable, IDictionary<TKInner, TV>, TKInner, TV>()
                .ConvertOutputFromDictionary<TKOuter, TKInnerEnumerable, TRes, TKInner, TV>()
                .ConvertToSync();

            async Task<IDictionary<TKInner, TV>> GetResults(TKOuter outerKey, IEnumerable<TKInner> innerKeys)
            {
                var results = await functionCache.GetMulti(outerKey, innerKeys);

                var dictionary = dictionaryFactoryFunc(keyComparer, results.Count);
                
                foreach (var kv in results)
                    dictionary[kv.Key.Item2] = kv.Value;

                return dictionary;
            }
        }
    }

    public sealed class MultiParamEnumerableKeyFunctionCacheConfigurationManagerSync<TKOuter1, TKOuter2, TKInnerEnumerable, TRes, TKInner, TV>
        : MultiParamEnumerableKeyFunctionCacheConfigurationManagerBase<MultiParamEnumerableKeyFunctionCacheConfigurationManagerSync<TKOuter1, TKOuter2, TKInnerEnumerable, TRes, TKInner, TV>, (TKOuter1, TKOuter2), TKInner, TV>
        where TKInnerEnumerable : IEnumerable<TKInner>
        where TRes : IDictionary<TKInner, TV>
    {
        internal MultiParamEnumerableKeyFunctionCacheConfigurationManagerSync(
            Func<TKOuter1, TKOuter2, TKInnerEnumerable, TRes> inputFunc,
            string functionName)
            : base(
                inputFunc
                    .ConvertFunc()
                    .ConvertToAsync()
                    .ConvertInputToEnumerable<(TKOuter1, TKOuter2), TKInnerEnumerable, TRes, TKInner, TV>()
                    .ConvertOutputToDictionary<(TKOuter1, TKOuter2), IEnumerable<TKInner>, TRes, TKInner, TV>(),
                functionName)
        {
        }

        internal MultiParamEnumerableKeyFunctionCacheConfigurationManagerSync(
            Func<TKOuter1, TKOuter2, TKInnerEnumerable, TRes> inputFunc,
            CachedProxyConfig interfaceConfig,
            MethodInfo methodInfo)
            : base(
                inputFunc
                    .ConvertFunc()
                    .ConvertToAsync()
                    .ConvertInputToEnumerable<(TKOuter1, TKOuter2), TKInnerEnumerable, TRes, TKInner, TV>()
                    .ConvertOutputToDictionary<(TKOuter1, TKOuter2), IEnumerable<TKInner>, TRes, TKInner, TV>(),
                interfaceConfig,
                methodInfo)
        {
        }
        
        public new MultiParamEnumerableKeyFunctionCacheConfigurationManagerSync<TKOuter1, TKOuter2, TKInnerEnumerable, TRes, TKInner, TV> WithKeySerializer(
            ISerializer serializer)
        {
            WithKeySerializerInternal(serializer.Serialize, serializer.Deserialize<TKOuter1>);
            WithKeySerializerInternal(serializer.Serialize, serializer.Deserialize<TKOuter2>);
            WithKeySerializerInternal(serializer.Serialize, serializer.Deserialize<TKInner>);
            return this;
        }

        public MultiParamEnumerableKeyFunctionCacheConfigurationManagerSync<TKOuter1, TKOuter2, TKInnerEnumerable, TRes, TKInner, TV> WithKeySerializer(
            ISerializer<TKOuter1> serializer)
        {
            return WithKeySerializer(serializer.Serialize, serializer.Deserialize);
        }
        
        public MultiParamEnumerableKeyFunctionCacheConfigurationManagerSync<TKOuter1, TKOuter2, TKInnerEnumerable, TRes, TKInner, TV> WithKeySerializer(
            ISerializer<TKOuter2> serializer)
        {
            return WithKeySerializer(serializer.Serialize, serializer.Deserialize);
        }

        public MultiParamEnumerableKeyFunctionCacheConfigurationManagerSync<TKOuter1, TKOuter2, TKInnerEnumerable, TRes, TKInner, TV> WithKeySerializer(
            ISerializer<TKInner> serializer)
        {
            return WithKeySerializer(serializer.Serialize, serializer.Deserialize);
        }

        public MultiParamEnumerableKeyFunctionCacheConfigurationManagerSync<TKOuter1, TKOuter2, TKInnerEnumerable, TRes, TKInner, TV> WithKeySerializer(
            Func<TKOuter1, string> serializer, Func<string, TKOuter1> deserializer = null)
        {
            return WithKeySerializerInternal(serializer, deserializer);
        }
        
        public MultiParamEnumerableKeyFunctionCacheConfigurationManagerSync<TKOuter1, TKOuter2, TKInnerEnumerable, TRes, TKInner, TV> WithKeySerializer(
            Func<TKOuter2, string> serializer, Func<string, TKOuter2> deserializer = null)
        {
            return WithKeySerializerInternal(serializer, deserializer);
        }

        public MultiParamEnumerableKeyFunctionCacheConfigurationManagerSync<TKOuter1, TKOuter2, TKInnerEnumerable, TRes, TKInner, TV> WithKeySerializer(
            Func<TKInner, string> serializer, Func<string, TKInner> deserializer = null)
        {
            return WithKeySerializerInternal(serializer, deserializer);
        }

        public MultiParamEnumerableKeyFunctionCacheConfigurationManagerSync<TKOuter1, TKOuter2, TKInnerEnumerable, TRes, TKInner, TV> WithKeyComparer(
            IEqualityComparer<TKOuter1> comparer)
        {
            return WithKeyComparerInternal(comparer);
        }

        public MultiParamEnumerableKeyFunctionCacheConfigurationManagerSync<TKOuter1, TKOuter2, TKInnerEnumerable, TRes, TKInner, TV> WithKeyComparer(
            IEqualityComparer<TKOuter2> comparer)
        {
            return WithKeyComparerInternal(comparer);
        }
        
        public MultiParamEnumerableKeyFunctionCacheConfigurationManagerSync<TKOuter1, TKOuter2, TKInnerEnumerable, TRes, TKInner, TV> WithKeyComparer(
            IEqualityComparer<TKInner> comparer)
        {
            return WithKeyComparerInternal(comparer);
        }

        public Func<TKOuter1, TKOuter2, TKInnerEnumerable, TRes> Build()
        {
            var functionCache = BuildMultiParamEnumerableKeyFunctionCache(
                TupleKeyHelper.BuildKeyComparer<TKOuter1, TKOuter2>(KeyComparers),
                TupleKeyHelper.BuildKeySerializer<TKOuter1, TKOuter2>(KeySerializers, KeyParamSeparator));

            var dictionaryFactoryFunc = DictionaryFactoryFunc ?? DictionaryFactoryFuncResolver.Get<TRes, TKInner, TV>();

            var keyComparer = KeyComparerResolver.GetInner<TKInner>();

            Func<(TKOuter1, TKOuter2), IEnumerable<TKInner>, Task<IDictionary<TKInner, TV>>> func = GetResults;

            return func
                .ConvertInputFromEnumerable<(TKOuter1, TKOuter2), TKInnerEnumerable, IDictionary<TKInner, TV>, TKInner, TV>()
                .ConvertOutputFromDictionary<(TKOuter1, TKOuter2), TKInnerEnumerable, TRes, TKInner, TV>()
                .ConvertToSync()
                .ConvertFunc();

            async Task<IDictionary<TKInner, TV>> GetResults(
                (TKOuter1, TKOuter2) outerKeys,
                IEnumerable<TKInner> innerKeys)
            {
                var results = await functionCache.GetMulti(outerKeys, innerKeys);

                var dictionary = dictionaryFactoryFunc(keyComparer, results.Count);

                foreach (var kv in results)
                    dictionary[kv.Key.Item2] = kv.Value;

                return dictionary;
            }
        }
    }

    public sealed class MultiParamEnumerableKeyFunctionCacheConfigurationManagerSync<TKOuter1, TKOuter2, TKOuter3, TKInnerEnumerable, TRes, TKInner, TV>
        : MultiParamEnumerableKeyFunctionCacheConfigurationManagerBase<MultiParamEnumerableKeyFunctionCacheConfigurationManagerSync<TKOuter1, TKOuter2, TKOuter3, TKInnerEnumerable, TRes, TKInner, TV>, (TKOuter1, TKOuter2, TKOuter3), TKInner, TV>
        where TKInnerEnumerable : IEnumerable<TKInner>
        where TRes : IDictionary<TKInner, TV>
    {
        internal MultiParamEnumerableKeyFunctionCacheConfigurationManagerSync(
            Func<TKOuter1, TKOuter2, TKOuter3, TKInnerEnumerable, TRes> inputFunc,
            string functionName)
            : base(
                inputFunc
                    .ConvertFunc()
                    .ConvertToAsync()
                    .ConvertInputToEnumerable<(TKOuter1, TKOuter2, TKOuter3), TKInnerEnumerable, TRes, TKInner, TV>()
                    .ConvertOutputToDictionary<(TKOuter1, TKOuter2, TKOuter3), IEnumerable<TKInner>, TRes, TKInner, TV>(),
                functionName)
        {
        }

        internal MultiParamEnumerableKeyFunctionCacheConfigurationManagerSync(
            Func<TKOuter1, TKOuter2, TKOuter3, TKInnerEnumerable, TRes> inputFunc,
            CachedProxyConfig interfaceConfig,
            MethodInfo methodInfo)
            : base(
                inputFunc
                    .ConvertFunc()
                    .ConvertToAsync()
                    .ConvertInputToEnumerable<(TKOuter1, TKOuter2, TKOuter3), TKInnerEnumerable, TRes, TKInner, TV>()
                    .ConvertOutputToDictionary<(TKOuter1, TKOuter2, TKOuter3), IEnumerable<TKInner>, TRes, TKInner, TV>(),
                interfaceConfig,
                methodInfo)
        {
        }

        public new MultiParamEnumerableKeyFunctionCacheConfigurationManagerSync<TKOuter1, TKOuter2, TKOuter3, TKInnerEnumerable, TRes, TKInner, TV> WithKeySerializer(
            ISerializer serializer)
        {
            WithKeySerializerInternal(serializer.Serialize, serializer.Deserialize<TKOuter1>);
            WithKeySerializerInternal(serializer.Serialize, serializer.Deserialize<TKOuter2>);
            WithKeySerializerInternal(serializer.Serialize, serializer.Deserialize<TKOuter3>);
            WithKeySerializerInternal(serializer.Serialize, serializer.Deserialize<TKInner>);
            return this;
        }

        public MultiParamEnumerableKeyFunctionCacheConfigurationManagerSync<TKOuter1, TKOuter2, TKOuter3, TKInnerEnumerable, TRes, TKInner, TV> WithKeySerializer(
            ISerializer<TKOuter1> serializer)
        {
            return WithKeySerializer(serializer.Serialize, serializer.Deserialize);
        }

        public MultiParamEnumerableKeyFunctionCacheConfigurationManagerSync<TKOuter1, TKOuter2, TKOuter3, TKInnerEnumerable, TRes, TKInner, TV> WithKeySerializer(
            ISerializer<TKOuter2> serializer)
        {
            return WithKeySerializer(serializer.Serialize, serializer.Deserialize);
        }

        public MultiParamEnumerableKeyFunctionCacheConfigurationManagerSync<TKOuter1, TKOuter2, TKOuter3, TKInnerEnumerable, TRes, TKInner, TV> WithKeySerializer(
            ISerializer<TKOuter3> serializer)
        {
            return WithKeySerializer(serializer.Serialize, serializer.Deserialize);
        }

        public MultiParamEnumerableKeyFunctionCacheConfigurationManagerSync<TKOuter1, TKOuter2, TKOuter3, TKInnerEnumerable, TRes, TKInner, TV> WithKeySerializer(
            ISerializer<TKInner> serializer)
        {
            return WithKeySerializer(serializer.Serialize, serializer.Deserialize);
        }

        public MultiParamEnumerableKeyFunctionCacheConfigurationManagerSync<TKOuter1, TKOuter2, TKOuter3, TKInnerEnumerable, TRes, TKInner, TV> WithKeySerializer(
            Func<TKOuter1, string> serializer, Func<string, TKOuter1> deserializer = null)
        {
            return WithKeySerializerInternal(serializer, deserializer);
        }

        public MultiParamEnumerableKeyFunctionCacheConfigurationManagerSync<TKOuter1, TKOuter2, TKOuter3, TKInnerEnumerable, TRes, TKInner, TV> WithKeySerializer(
            Func<TKOuter2, string> serializer, Func<string, TKOuter2> deserializer = null)
        {
            return WithKeySerializerInternal(serializer, deserializer);
        }
        
        public MultiParamEnumerableKeyFunctionCacheConfigurationManagerSync<TKOuter1, TKOuter2, TKOuter3, TKInnerEnumerable, TRes, TKInner, TV> WithKeySerializer(
            Func<TKOuter3, string> serializer, Func<string, TKOuter3> deserializer = null)
        {
            return WithKeySerializerInternal(serializer, deserializer);
        }

        public MultiParamEnumerableKeyFunctionCacheConfigurationManagerSync<TKOuter1, TKOuter2, TKOuter3, TKInnerEnumerable, TRes, TKInner, TV> WithKeySerializer(
            Func<TKInner, string> serializer, Func<string, TKInner> deserializer = null)
        {
            return WithKeySerializerInternal(serializer, deserializer);
        }

        public MultiParamEnumerableKeyFunctionCacheConfigurationManagerSync<TKOuter1, TKOuter2, TKOuter3, TKInnerEnumerable, TRes, TKInner, TV> WithKeyComparer(
            IEqualityComparer<TKOuter1> comparer)
        {
            return WithKeyComparerInternal(comparer);
        }

        public MultiParamEnumerableKeyFunctionCacheConfigurationManagerSync<TKOuter1, TKOuter2, TKOuter3, TKInnerEnumerable, TRes, TKInner, TV> WithKeyComparer(
            IEqualityComparer<TKOuter2> comparer)
        {
            return WithKeyComparerInternal(comparer);
        }

        public MultiParamEnumerableKeyFunctionCacheConfigurationManagerSync<TKOuter1, TKOuter2, TKOuter3, TKInnerEnumerable, TRes, TKInner, TV> WithKeyComparer(
            IEqualityComparer<TKOuter3> comparer)
        {
            return WithKeyComparerInternal(comparer);
        }
        
        public MultiParamEnumerableKeyFunctionCacheConfigurationManagerSync<TKOuter1, TKOuter2, TKOuter3, TKInnerEnumerable, TRes, TKInner, TV> WithKeyComparer(
            IEqualityComparer<TKInner> comparer)
        {
            return WithKeyComparerInternal(comparer);
        }

        public Func<TKOuter1, TKOuter2, TKOuter3, TKInnerEnumerable, TRes> Build()
        {
            var functionCache = BuildMultiParamEnumerableKeyFunctionCache(
                TupleKeyHelper.BuildKeyComparer<TKOuter1, TKOuter2, TKOuter3>(KeyComparers),
                TupleKeyHelper.BuildKeySerializer<TKOuter1, TKOuter2, TKOuter3>(KeySerializers, KeyParamSeparator));

            var dictionaryFactoryFunc = DictionaryFactoryFunc ?? DictionaryFactoryFuncResolver.Get<TRes, TKInner, TV>();

            var keyComparer = KeyComparerResolver.GetInner<TKInner>();

            Func<(TKOuter1, TKOuter2, TKOuter3), IEnumerable<TKInner>, Task<IDictionary<TKInner, TV>>>
                func = GetResults;

            return func
                .ConvertInputFromEnumerable<(TKOuter1, TKOuter2, TKOuter3), TKInnerEnumerable, IDictionary<TKInner, TV>, TKInner, TV>()
                .ConvertOutputFromDictionary<(TKOuter1, TKOuter2, TKOuter3), TKInnerEnumerable, TRes, TKInner, TV>()
                .ConvertToSync()
                .ConvertFunc();

            async Task<IDictionary<TKInner, TV>> GetResults(
                (TKOuter1, TKOuter2, TKOuter3) outerKeys,
                IEnumerable<TKInner> innerKeys)
            {
                var results = await functionCache.GetMulti(outerKeys, innerKeys);

                var dictionary = dictionaryFactoryFunc(keyComparer, results.Count);

                foreach (var kv in results)
                    dictionary[kv.Key.Item2] = kv.Value;

                return dictionary;
            }
        }
    }
}