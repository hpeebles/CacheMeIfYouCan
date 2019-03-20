using System;

namespace CacheMeIfYouCan.Internal
{
    public static class FunctionExtensions
    {
        public static Func<TimeSpan> WithJitter(this Func<TimeSpan> func, double jitterPercentage)
        {
            var jitterHandler = new JitterHandler(func, jitterPercentage);

            return jitterHandler.GetNext;
        }
        
        public static Func<T, TimeSpan> WithJitter<T>(this Func<T, TimeSpan> func, double jitterPercentage)
        {
            var jitterHandler = new JitterHandler<T>(func, jitterPercentage);

            return jitterHandler.GetNext;
        }
        
        public static Func<T1, T2, TimeSpan> WithJitter<T1, T2>(this Func<T1, T2, TimeSpan> func, double jitterPercentage)
        {
            var jitterHandler = new JitterHandler<T1, T2>(func, jitterPercentage);

            return jitterHandler.GetNext;
        }
    }
}