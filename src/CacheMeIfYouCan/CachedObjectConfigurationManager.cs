using System;
using System.Threading;
using System.Threading.Tasks;
using CacheMeIfYouCan.Internal;

namespace CacheMeIfYouCan
{
    public sealed class CachedObjectConfigurationManager<T>
    {
        private readonly Func<CancellationToken, Task<T>> _getValueFunc;
        private Func<TimeSpan> _refreshIntervalFactory;
        private TimeSpan? _refreshValueFuncTimeout;

        internal CachedObjectConfigurationManager(Func<CancellationToken, Task<T>> getValueFunc)
        {
            _getValueFunc = getValueFunc;
        }

        public CachedObjectConfigurationManager<T> WithRefreshInterval(TimeSpan refreshInterval)
        {
            if (refreshInterval <= TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(nameof(refreshInterval));

            _refreshIntervalFactory = () => refreshInterval;
            return this;
        }

        public CachedObjectConfigurationManager<T> WithRefreshIntervalFactory(Func<TimeSpan> refreshIntervalFactory)
        {
            if (refreshIntervalFactory is null)
                throw new ArgumentNullException(nameof(refreshIntervalFactory));

            _refreshIntervalFactory = refreshIntervalFactory;
            return this;
        }

        public CachedObjectConfigurationManager<T> WithRefreshValueFuncTimeout(TimeSpan timeout)
        {
            _refreshValueFuncTimeout = timeout;
            return this;
        }

        public ICachedObject<T> Build()
        {
            return new CachedObject<T>(_getValueFunc, _refreshIntervalFactory, _refreshValueFuncTimeout);
        }
    }
}