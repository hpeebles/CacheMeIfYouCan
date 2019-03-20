using System;

namespace CacheMeIfYouCan.Internal
{
    public class JitterHandler
    {
        private readonly Func<TimeSpan> _func;
        private readonly RandomNumberGenerator _rng;
        
        public JitterHandler(Func<TimeSpan> func, double jitterPercentage)
        {
            if (jitterPercentage < 0 || jitterPercentage >= 100)
                throw new ArgumentOutOfRangeException(nameof(jitterPercentage));
            
            _func = func;
            _rng = new RandomNumberGenerator(1 - (jitterPercentage / 100), 1 + (jitterPercentage / 100));
        }

        public TimeSpan GetNext()
        {
            return new TimeSpan((long)(_func().Ticks * _rng.GetNext()));
        }
    }

    public class JitterHandler<T>
    {
        private readonly Func<T, TimeSpan> _func;
        private readonly RandomNumberGenerator _rng;
        
        public JitterHandler(Func<T, TimeSpan> func, double jitterPercentage)
        {
            if (jitterPercentage < 0 || jitterPercentage >= 100)
                throw new ArgumentOutOfRangeException(nameof(jitterPercentage));
            
            _func = func;
            _rng = new RandomNumberGenerator(1 - (jitterPercentage / 100), 1 + (jitterPercentage / 100));
        }

        public TimeSpan GetNext(T input)
        {
            return new TimeSpan((long)(_func(input).Ticks * _rng.GetNext()));
        }
    }
    
    public class JitterHandler<T1, T2>
    {
        private readonly Func<T1, T2, TimeSpan> _func;
        private readonly RandomNumberGenerator _rng;
        
        public JitterHandler(Func<T1, T2, TimeSpan> func, double jitterPercentage)
        {
            if (jitterPercentage < 0 || jitterPercentage >= 100)
                throw new ArgumentOutOfRangeException(nameof(jitterPercentage));
            
            _func = func;
            _rng = new RandomNumberGenerator(1 - (jitterPercentage / 100), 1 + (jitterPercentage / 100));
        }

        public TimeSpan GetNext(T1 input1, T2 input2)
        {
            return new TimeSpan((long)(_func(input1, input2).Ticks * _rng.GetNext()));
        }
    }
}