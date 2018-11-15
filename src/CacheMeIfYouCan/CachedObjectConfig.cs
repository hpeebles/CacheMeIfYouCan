using System;
using System.Threading.Tasks;
using CacheMeIfYouCan.Configuration;
using CacheMeIfYouCan.Internal;

namespace CacheMeIfYouCan
{
    public class CachedObjectConfig<T>
    {
        private readonly Func<Task<T>> _getValueFunc;
        private Func<TimeSpan> _intervalFunc;
        private double _jitterPercentage;
        private Action<Exception> _onError;

        internal CachedObjectConfig(Func<Task<T>> getValueFunc)
        {
            _getValueFunc = getValueFunc;

            if (DefaultCachedObjectConfig.Configuration.RefreshInterval.HasValue)
                WithRefreshInterval(DefaultCachedObjectConfig.Configuration.RefreshInterval.Value);

            if (DefaultCachedObjectConfig.Configuration.JitterPercentage.HasValue)
                WithJitterPercentage(DefaultCachedObjectConfig.Configuration.JitterPercentage.Value);
        }
        
        public CachedObjectConfig<T> WithRefreshInterval(TimeSpan interval)
        {
            if (interval == TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(nameof(interval));

            _intervalFunc = () => interval;
            return this;
        }

        public CachedObjectConfig<T> WithRefreshInterval(Func<TimeSpan> intervalFunc)
        {
            _intervalFunc = intervalFunc;
            return this;
        }
        
        public CachedObjectConfig<T> WithJitterPercentage(double percentage)
        {
            if (percentage < 0 || percentage > 100)
                throw new ArgumentOutOfRangeException(nameof(percentage));

            _jitterPercentage = percentage;
            return this;
        }

        public CachedObjectConfig<T> OnError(Action<Exception> onError)
        {
            _onError = onError;
            return this;
        }

        public ICachedObject<T> Build(bool registerGlobally = true)
        {
            Func<TimeSpan> intervalFunc;

            if (_jitterPercentage.Equals(0))
            {
                intervalFunc = _intervalFunc;
            }
            else
            {
                var random = new Random();

                var jitterPercentage = _jitterPercentage;

                // This gives a uniformly distributed value between +/- percentage
                double JitterFunc() => (random.NextDouble() - 0.5) * 2 * jitterPercentage;

                intervalFunc = () => TimeSpan.FromTicks((long)(_intervalFunc().Ticks * (1 + (JitterFunc() / 100))));
            }

            var cachedObject = new CachedObject<T>(_getValueFunc, intervalFunc, _onError);
            
            if (registerGlobally)
                CachedObjectInitialiser.Register(cachedObject);

            return cachedObject;
        }
    }
}