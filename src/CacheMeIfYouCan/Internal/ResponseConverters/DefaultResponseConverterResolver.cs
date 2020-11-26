using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace CacheMeIfYouCan.Internal.ResponseConverters
{
    internal static class DefaultResponseConverterResolver
    {
        public static Func<Dictionary<TKey, TValue>, TResponse> Get<TKey, TValue, TResponse>(IEqualityComparer<TKey> keyComparer)
            where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            keyComparer ??= EqualityComparer<TKey>.Default;
            
            IResponseConverter<TKey, TValue, TResponse> converter;
            if (typeof(TResponse).IsAssignableFrom(typeof(Dictionary<TKey, TValue>)))
                converter = Unsafe.As<IResponseConverter<TKey, TValue, TResponse>>(new DictionaryConverter<TKey, TValue>());
            else if (typeof(TResponse).IsAssignableFrom(typeof(ConcurrentDictionary<TKey, TValue>)))
                converter = Unsafe.As<IResponseConverter<TKey, TValue, TResponse>>(new ConcurrentDictionaryConverter<TKey, TValue>(keyComparer));
            else if (typeof(TResponse).IsAssignableFrom(typeof(List<KeyValuePair<TKey, TValue>>)))
                converter = Unsafe.As<IResponseConverter<TKey, TValue, TResponse>>(new ListKeyValuePairConverter<TKey, TValue>());
            else if (typeof(TResponse).IsAssignableFrom(typeof(KeyValuePair<TKey, TValue>[])))
                converter = Unsafe.As<IResponseConverter<TKey, TValue, TResponse>>(new ArrayKeyValuePairConverter<TKey, TValue>());
            else
                throw new Exception($"No response converter defined for type '{typeof(TResponse)}'");

            return converter.Convert;
        }
    }
}