using System.Collections.Generic;
using System.Linq;

namespace CacheMeIfYouCan.Internal
{
    internal static class IEnumerableExtensions
    {
        public static IReadOnlyCollection<T> ToReadOnlyCollection<T>(this IEnumerable<T> source)
        {
            if (source is IReadOnlyCollection<T> collection)
                return collection;

            return source.ToList();
        }
    }
}