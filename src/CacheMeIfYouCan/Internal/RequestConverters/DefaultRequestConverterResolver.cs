using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace CacheMeIfYouCan.Internal.RequestConverters
{
    internal static class DefaultRequestConverterResolver
    {
        public static Func<ReadOnlyMemory<TKey>, TRequest> Get<TKey, TRequest>(IEqualityComparer<TKey> keyComparer)
            where TRequest : IEnumerable<TKey>
        {
            IRequestConverter<TKey, TRequest> converter;
            if (typeof(TRequest) == typeof(ArraySegment<TKey>))
                converter = Unsafe.As<IRequestConverter<TKey, TRequest>>(new ArraySegmentConverter<TKey>());
            else if (typeof(TRequest).IsAssignableFrom(typeof(IList<TKey>)))
                converter = Unsafe.As<IRequestConverter<TKey, TRequest>>(new IListConverter<TKey>());
            else if (typeof(TRequest).IsAssignableFrom(typeof(IReadOnlyList<TKey>)))
                converter = Unsafe.As<IRequestConverter<TKey, TRequest>>(new IReadOnlyListConverter<TKey>());
            else if (typeof(TRequest).IsAssignableFrom(typeof(List<TKey>)))
                converter = Unsafe.As<IRequestConverter<TKey, TRequest>>(new ListConverter<TKey>());
            else if (typeof(TRequest).IsAssignableFrom(typeof(TKey[])))
                converter = Unsafe.As<IRequestConverter<TKey, TRequest>>(new ArrayConverter<TKey>());
            else if (typeof(TRequest).IsAssignableFrom(typeof(HashSet<TKey>)))
                converter = Unsafe.As<IRequestConverter<TKey, TRequest>>(new HashSetConverter<TKey>(keyComparer));
            else
                throw new Exception($"No request converter defined for type '{typeof(TRequest)}'");

            return converter.Convert;
        }
    }
}