using System;
using System.Threading;
using System.Threading.Tasks;
using CacheMeIfYouCan.Configuration;

namespace CacheMeIfYouCan.Internal
{
    internal class CachedObjectConfigurationManager<T> :
        ICachedObjectConfigurationManager_WithRefreshInterval<T>,
        ICachedObjectConfigurationManager<T>
    {
        protected readonly Func<CancellationToken, Task<T>> _getValueFunc;
        protected TimeSpan? _refreshInterval;
        protected Func<TimeSpan> _refreshIntervalFactory;
        protected TimeSpan? _refreshValueFuncTimeout;
        protected Action<CachedObjectValueRefreshedEvent<T>> _onValueRefreshedAction;
        protected Action<CachedObjectValueRefreshExceptionEvent<T>> _onValueRefreshExceptionAction;

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
        
        public ICachedObjectConfigurationManager<T> OnValueRefreshed(Action<CachedObjectValueRefreshedEvent<T>> action)
        {
            _onValueRefreshedAction += action;
            return this;
        }
        
        public ICachedObjectConfigurationManager<T> OnValueRefreshException(Action<CachedObjectValueRefreshExceptionEvent<T>> action)
        {
            _onValueRefreshExceptionAction += action;
            return this;
        }

        public ICachedObjectWithUpdatesConfigurationManager<T, TUpdateFuncInput> WithUpdates<TUpdateFuncInput>(
            Func<T, TUpdateFuncInput, CancellationToken, Task<T>> updateValueFunc)
        {
            return new CachedObjectWithUpdatesConfigurationManager<T, TUpdateFuncInput>(_getValueFunc, updateValueFunc)
            {
                _refreshInterval = _refreshInterval,
                _refreshIntervalFactory = _refreshIntervalFactory,
                _refreshValueFuncTimeout = _refreshValueFuncTimeout,
                _onValueRefreshedAction = _onValueRefreshedAction,
                _onValueRefreshExceptionAction = _onValueRefreshExceptionAction
            };
        }

        public ICachedObjectWithUpdatesConfigurationManager<T, TUpdateFuncInput> WithUpdates<TUpdateFuncInput>(Func<T, TUpdateFuncInput, CancellationToken, T> updateValueFunc)
        {
            return WithUpdates<TUpdateFuncInput>((current, input, cancellationToken) =>
                Task.FromResult(updateValueFunc(current, input, cancellationToken)));
        }

        public ICachedObjectWithUpdatesConfigurationManager<T, TUpdateFuncInput> WithUpdates<TUpdateFuncInput>(Func<T, TUpdateFuncInput, Task<T>> updateValueFunc)
        {
            return WithUpdates<TUpdateFuncInput>((current, input, cancellationToken) =>
                updateValueFunc(current, input));
        }

        public ICachedObjectWithUpdatesConfigurationManager<T, TUpdateFuncInput> WithUpdates<TUpdateFuncInput>(Func<T, TUpdateFuncInput, T> updateValueFunc)
        {
            return WithUpdates<TUpdateFuncInput>((current, input, cancellationToken) =>
                Task.FromResult(updateValueFunc(current, input)));
        }

        public ICachedObject<T> Build()
        {
            var refreshIntervalFactory = GetRefreshIntervalFactory();
            
            var cachedObject = new CachedObject<T, Unit>(
                _getValueFunc,
                refreshIntervalFactory,
                _refreshValueFuncTimeout);

            AddOnValueRefreshedActions(cachedObject);
            
            return cachedObject;
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

        protected Func<TimeSpan> GetRefreshIntervalFactory()
        {
            var refreshIntervalFactory = _refreshIntervalFactory;
            if (refreshIntervalFactory is null && _refreshInterval.HasValue)
            {
                var refreshInterval = _refreshInterval.Value;
                refreshIntervalFactory = () => refreshInterval;
            }

            return refreshIntervalFactory;
        }

        protected void AddOnValueRefreshedActions(ICachedObject<T> cachedObject)
        {
            if (!(_onValueRefreshedAction is null))
                cachedObject.OnValueRefreshed += (_, e) => _onValueRefreshedAction(e);

            if (!(_onValueRefreshExceptionAction is null))
                cachedObject.OnValueRefreshException += (_, e) => _onValueRefreshExceptionAction(e);
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