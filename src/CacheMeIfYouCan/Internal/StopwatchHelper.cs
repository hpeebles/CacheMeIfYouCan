using System;
using System.Diagnostics;

namespace CacheMeIfYouCan.Internal
{
    // When calling GetDuration you must pass in a value in Stopwatch ticks and not DateTime ticks.
    internal static class StopwatchHelper
    {
        private static readonly double Multiplier = (double) TimeSpan.TicksPerSecond / Stopwatch.Frequency;
        
        public static TimeSpan GetDuration(long start)
        {
            return GetDuration(start, Stopwatch.GetTimestamp());
        }

        public static TimeSpan GetDuration(long start, long end)
        {
            return TimeSpan.FromTicks((long)((end - start) * Multiplier));
        }
    }
}