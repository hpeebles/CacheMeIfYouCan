using System.Collections.Generic;

namespace CacheMeIfYouCan.Internal
{
    internal static class ReadOnlyListExtensions
    {
        public static IEnumerable<IReadOnlyList<T>> Batch<T>(this IReadOnlyList<T> list, int batchSize)
        {
            var remaining = list.Count;
            var offset = 0;
            do
            {
                if (remaining < batchSize)
                {
                    yield return new ReadOnlyListSegment<T>(list, offset, remaining);
                    break;
                }

                yield return new ReadOnlyListSegment<T>(list, offset, batchSize);

                offset += batchSize;
                remaining -= batchSize;
            }
            while (remaining > 0);
        }
    }
}