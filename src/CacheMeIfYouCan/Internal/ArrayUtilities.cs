using System;
using System.Buffers;

namespace CacheMeIfYouCan.Internal
{
    public static class ArrayUtilities
    {
        public static void GrowPooledArray<TKey>(ref TKey[] array, int maxSize)
        {
            var newArrayLength = Math.Min(array.Length * 2, maxSize);
            var newArray = ArrayPool<TKey>.Shared.Rent(newArrayLength);
            Array.Copy(array, newArray, array.Length);
            ArrayPool<TKey>.Shared.Return(array);
            array = newArray;
        }
    }
}