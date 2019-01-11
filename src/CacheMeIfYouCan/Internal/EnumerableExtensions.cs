using System.Collections.Generic;
using System.Linq;

namespace CacheMeIfYouCan.Internal
{
    internal static class EnumerableExtensions
    {
        public static T[] AsArray<T>(this IEnumerable<T> input)
        {
            return input is T[] array
                ? array
                : input.ToArray();
        }
        
        public static List<T> AsList<T>(this IEnumerable<T> input)
        {
            return input is List<T> list
                ? list
                : input.ToList();
        }
        
        public static IList<T> AsIList<T>(this IEnumerable<T> input)
        {
            return input is IList<T> list
                ? list
                : input.ToArray();
        }

        public static ICollection<T> AsICollection<T>(this IEnumerable<T> input)
        {
            return input is ICollection<T> collection
                ? collection
                : input.ToArray();
        }
        
        public static ISet<T> AsISet<T>(this IEnumerable<T> input)
        {
            return input is ISet<T> collection
                ? collection
                : new HashSet<T>(input);
        }
        
        public static HashSet<T> AsHashSet<T>(this IEnumerable<T> input)
        {
            return input is HashSet<T> collection
                ? collection
                : new HashSet<T>(input);
        }
        
        public static SortedSet<T> AsSortedSet<T>(this IEnumerable<T> input)
        {
            return input is SortedSet<T> collection
                ? collection
                : new SortedSet<T>(input);
        }
    }
}