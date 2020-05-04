using System;
using System.Diagnostics;

namespace CacheMeIfYouCan.Internal
{
    internal readonly struct StopwatchStruct
    {
        private readonly long _startTime;
        private static readonly double Multiplier = (double)TimeSpan.TicksPerSecond / Stopwatch.Frequency;
        
        private StopwatchStruct(long startTime)
        {
            _startTime = startTime;
        }
        
        public static StopwatchStruct StartNew()
        {
            return new StopwatchStruct(Stopwatch.GetTimestamp());
        }

        public TimeSpan Elapsed
        {
            get
            {
                var stopwatchTicks = Stopwatch.GetTimestamp() - _startTime;

                var dateTimeTicks = (long)(stopwatchTicks * Multiplier);
                
                return new TimeSpan(dateTimeTicks);
            }
        }
    }
}