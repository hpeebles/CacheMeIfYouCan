using System;
using System.Diagnostics;

namespace CacheMeIfYouCan.Internal
{
    public static class TimingsHelper
    {
        private static readonly double Multiplier = (double) TimeSpan.TicksPerSecond / Stopwatch.Frequency;
        
        public static long Start()
        {
            return Stopwatch.GetTimestamp();
        }

        public static long GetDuration(long start)
        {
            return (long) ((Stopwatch.GetTimestamp() - start) * Multiplier);
        }
    }
}