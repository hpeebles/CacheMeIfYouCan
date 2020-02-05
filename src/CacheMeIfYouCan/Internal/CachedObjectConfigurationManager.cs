using System;
using System.Threading;
using System.Threading.Tasks;

namespace CacheMeIfYouCan.Internal
{
    internal sealed class CachedObjectConfigurationManager<T> : ICachedObjectConfigurationManager_WithRefreshInterval<T>, ICachedObjectConfigurationManager<T>
    {
        private readonly Func<CancellationToken, Task<T>> _getValueFunc;
        private TimeSpan? _refreshInterval;
        private Func<TimeSpan> _refreshIntervalFactory;
        private TimeSpan? _refreshValueFuncTimeout;

        internal CachedObjectConfigurationManager(Func<CancellationToken, Task<T>> getValueFunc)
        {
            _getValueFunc = getValueFunc;
        }

        public ICachedObjectConfigurationManager_WithRefreshInterval<T> WithRefreshInterval(TimeSpan refreshInterval)
        {
            if (refreshInterval <= TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(nameof(refreshInterval));

            SetRefreshInterval(refreshInterval);
            return this;
        }

        public ICachedObjectConfigurationManager<T> WithRefreshIntervalFactory(Func<TimeSpan> refreshIntervalFactory)
        {
            if (refreshIntervalFactory is null)
                throw new ArgumentNullException(nameof(refreshIntervalFactory));

            SetRefreshIntervalFactory(refreshIntervalFactory);
            return this;
        }

        public ICachedObjectConfigurationManager<T> WithRefreshValueFuncTimeout(TimeSpan timeout)
        {
            _refreshValueFuncTimeout = timeout;
            return this;
        }
        
        public ICachedObject<T> Build()
        {
            var refreshIntervalFactory = _refreshIntervalFactory;
            if (refreshIntervalFactory is null && _refreshInterval.HasValue)
            {
                var refreshInterval = _refreshInterval.Value;
                refreshIntervalFactory = () => refreshInterval;
            }
            
            if (refreshIntervalFactory is null)
                throw new Exception("No refresh interval has been configured");
            
            return new CachedObject<T>(
                _getValueFunc,
                refreshIntervalFactory,
                _refreshValueFuncTimeout);
        }

        ICachedObjectConfigurationManager<T> ICachedObjectConfigurationManager_WithRefreshInterval<T>.WithJitter(double jitterPercentage)
        {
            if (jitterPercentage < 0 || jitterPercentage >= 100)
                throw new ArgumentOutOfRangeException(nameof(jitterPercentage));

            if (!_refreshInterval.HasValue)
                throw new InvalidOperationException($"You can only use {nameof(ICachedObjectConfigurationManager_WithRefreshInterval<T>.WithJitter)} after first using {nameof(WithRefreshInterval)}");
            
            var jitterHandler = new JitterHandler(_refreshInterval.Value, jitterPercentage);

            SetRefreshIntervalFactory(() => jitterHandler.GetNext());
            return this;
        }

        private void SetRefreshInterval(TimeSpan refreshInterval)
        {
            _refreshIntervalFactory = null;
            _refreshInterval = refreshInterval;
        }

        private void SetRefreshIntervalFactory(Func<TimeSpan> refreshIntervalFactory)
        {
            _refreshInterval = null;
            _refreshIntervalFactory = refreshIntervalFactory;
        }
    }
}