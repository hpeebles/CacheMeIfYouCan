using System;

namespace CacheMeIfYouCan.Internal
{
    internal sealed class JitterHandler
    {
        private readonly TimeSpan _interval;
        private readonly double _minMultiplier;
        private readonly double _maxMultiplier;
        
        public JitterHandler(TimeSpan interval, double jitterPercentage)
        {
            if (jitterPercentage < 0 || jitterPercentage >= 100)
                throw new ArgumentOutOfRangeException(nameof(jitterPercentage));
            
            _interval = interval;
            _minMultiplier = 1 - (jitterPercentage / 100);
            _maxMultiplier = 1 + (jitterPercentage / 100);
        }

        public TimeSpan GetNext()
        {
            return new TimeSpan((long)(_interval.Ticks * RandomNumberGenerator.GetNext(_minMultiplier, _maxMultiplier)));
        }
    }
}