using System;
using System.Runtime.CompilerServices;
using CacheMeIfYouCan.Events.CachedFunction.SingleKey;

namespace CacheMeIfYouCan.Internal.CachedFunctions
{
    [Flags]
    internal enum CacheGetFlags : byte
    {
        LocalCache_Enabled = 0b_0001,
        LocalCache_KeyRequested = 0b_0010,
        LocalCache_Skipped = 0b_0100,
        LocalCache_Hit = 0b_1000,
        DistributedCache_Enabled = 0b_0001_0000,
        DistributedCache_KeyRequested = 0b_0010_0000,
        DistributedCache_Skipped = 0b_0100_0000,
        DistributedCache_Hit = 0b_1000_0000
    }

    internal static class CacheGetFlagsExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static CacheGetStats ToStats(this CacheGetFlags flags)
        {
            return new CacheGetStats(flags);
        }
    }
}