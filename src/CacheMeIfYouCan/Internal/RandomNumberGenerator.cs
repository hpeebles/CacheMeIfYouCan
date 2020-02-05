using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace CacheMeIfYouCan.Internal
{
    internal static class RandomNumberGenerator
    {
        [ThreadStatic]
        private static Random _random;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double GetNext(double min, double max)
        {
            if (_random == null)
                _random = new Random((int)unchecked(DateTime.UtcNow.Ticks * Thread.CurrentThread.ManagedThreadId));
            
            var multiplier = max - min;
            var offset = min;
            
            return (_random.NextDouble() * multiplier) + offset;
        }
    }
}