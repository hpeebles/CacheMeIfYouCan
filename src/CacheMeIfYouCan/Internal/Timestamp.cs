using System;

namespace CacheMeIfYouCan.Internal
{
    public static class Timestamp
    {
        private static readonly long UnixEpochTicks = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).Ticks;

        public static long Now => DateTime.UtcNow.Ticks - UnixEpochTicks;
    }
}