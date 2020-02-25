using System;

namespace CacheMeIfYouCan.Internal
{
    internal static class Errors
    {
        public static ArgumentOutOfRangeException LocalCache_DestinationArrayTooSmall(string paramName)
        {
            throw new ArgumentOutOfRangeException(paramName, "Destination array is too small to fit all potential results");
        }
    }
}