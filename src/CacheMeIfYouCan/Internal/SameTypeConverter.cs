using System;

namespace CacheMeIfYouCan.Internal
{
    internal static class SameTypeConverter
    {
        public static TTo Convert<TFrom, TTo>(TFrom value)
        {
            return value is TTo result
                ? result
                : throw new ArgumentException(nameof(value));
        }
    }
}