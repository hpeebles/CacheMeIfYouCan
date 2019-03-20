using System;
using System.Threading;

namespace CacheMeIfYouCan.Internal
{
    public class RandomNumberGenerator
    {
        private readonly double _multiplier;
        private readonly double _offset;
        
        [ThreadStatic]
        private static Random _random;
        
        public RandomNumberGenerator(double min, double max)
        {
            _multiplier = max - min;
            _offset = min;
        }

        public double GetNext()
        {
            if (_random == null)
                _random = new Random((int)unchecked(DateTime.UtcNow.Ticks * Thread.CurrentThread.ManagedThreadId));
            
            return (_random.NextDouble() * _multiplier) + _offset;
        }
    }
}