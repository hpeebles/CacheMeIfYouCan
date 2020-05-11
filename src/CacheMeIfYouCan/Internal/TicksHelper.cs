using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace CacheMeIfYouCan.Internal
{
    public static class TicksHelper
    {
        private static long _iterationCount;
        private static bool _previousWasPositive;
        private static readonly object Lock = new Object();
        
        /// <summary>
        /// Get the number of milliseconds elapsed since the system started.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long GetTicks64()
        {
            var ticks = Environment.TickCount;

            var currentIsPositive = ticks > 0;
            var previousWasPositive = _previousWasPositive;

            if (currentIsPositive != previousWasPositive)
                HandleSignChange(currentIsPositive);

            return (_iterationCount << 32) + ticks;
        }

        // No inlining because this is really rare (approx. once every 25 days)
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void HandleSignChange(bool currentIsPositive)
        {
            lock (Lock)
            {
                var previousWasPositive = _previousWasPositive;
                
                if (currentIsPositive == previousWasPositive)
                    return;

                if (!currentIsPositive)
                    _iterationCount++;
                
                Volatile.Write(ref _previousWasPositive, currentIsPositive);
            }
        }
    }
}