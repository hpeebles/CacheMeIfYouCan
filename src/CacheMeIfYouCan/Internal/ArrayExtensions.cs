using System;
using System.Collections.Generic;

namespace CacheMeIfYouCan.Internal
{
    internal static class ArrayExtensions
    {
        public static IEnumerable<IList<T>> Batch<T>(this T[] array, int batchSize)
        {
            var remaining = array.Length;
            var offset = 0;
            do
            {
                if (remaining < batchSize)
                {
                    yield return new ArraySegment<T>(array, offset, remaining);
                    break;
                }

                yield return new ArraySegment<T>(array, offset, batchSize);

                offset += batchSize;
                remaining -= batchSize;
            }
            while (remaining > 0);
        }
    }
}