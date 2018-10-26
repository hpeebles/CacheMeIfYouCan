using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using CacheMeIfYouCan.Internal;

namespace CacheMeIfYouCan.Caches
{
    public class CachedObjectConfig<T>
    {
        private readonly Func<Task<T>> _getValueFunc;
        private Func<TimeSpan> _intervalFunc;
        private Action<Exception> _onError;

        internal CachedObjectConfig(Func<Task<T>> getValueFunc)
        {
            _getValueFunc = getValueFunc;
        }
        
        public CachedObjectConfig<T> RefreshInterval(TimeSpan interval)
        {
            _intervalFunc = () => interval;
            return this;
        }

        public CachedObjectConfig<T> RefreshInterval(Func<TimeSpan> intervalFunc)
        {
            _intervalFunc = intervalFunc;
            return this;
        }
        
        public CachedObjectConfig<T> JitterPercentage(double percentage)
        {
            if (percentage < 0 || percentage > 100)
                throw new ArgumentOutOfRangeException(nameof(percentage));
            
            var random = new Random();

            // This gives a uniformly distributed value between +/- percentage
            double JitterFunc()
            {
                var j = (random.NextDouble() - 0.5) * 2 * percentage;

                return j;
            }

            var intervalFunc = _intervalFunc;
            
            _intervalFunc = () => TimeSpan.FromTicks((long) (intervalFunc().Ticks * (1 + (JitterFunc() / 100))));

            return this;
        }

        public CachedObjectConfig<T> OnError(Action<Exception> onError)
        {
            _onError = onError;
            return this;
        }

        public ICachedObject<T> Build(bool registerGlobally = true)
        {
            var cachedObject = new CachedObject<T>(_getValueFunc, _intervalFunc, _onError);
            
            if (registerGlobally)
                CachedObjectInitialiser.Register(cachedObject);

            return cachedObject;
        }
    }
}