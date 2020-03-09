using System;
using System.Threading;
using System.Threading.Tasks;
using CacheMeIfYouCan.Configuration;
using CacheMeIfYouCan.Events.CachedObject;

namespace CacheMeIfYouCan.Internal
{
    internal class CachedObjectConfigurationManager<T> :
        ICachedObjectConfigurationManager<T>,
        ICachedObjectConfigurationManager_WithRefreshInterval<T>
    {
        protected readonly Func<CancellationToken, Task<T>> _getValueFunc;
        protected TimeSpan? _refreshValueFuncTimeout;
        private TimeSpan? _refreshInterval;
        private Func<TimeSpan> _refreshIntervalFactory;
        private Action<ICachedObject<T>> _onInitializedAction;
        private Action<ICachedObject<T>> _onDisposedAction;
        private Action<ValueRefreshedEvent<T>> _onValueRefreshedAction;
        private Action<ValueRefreshExceptionEvent<T>> _onValueRefreshExceptionAction;

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

        public ICachedObjectConfigurationManager<T> OnInitialized(Action<ICachedObject<T>> action)
        {
            _onInitializedAction += action;
            return this;
        }
        
        public ICachedObjectConfigurationManager<T> OnDisposed(Action<ICachedObject<T>> action)
        {
            _onDisposedAction += action;
            return this;
        }
        
        public ICachedObjectConfigurationManager<T> OnValueRefreshed(Action<ValueRefreshedEvent<T>> action)
        {
            _onValueRefreshedAction += action;
            return this;
        }
        
        public ICachedObjectConfigurationManager<T> OnValueRefreshException(Action<ValueRefreshExceptionEvent<T>> action)
        {
            _onValueRefreshExceptionAction += action;
            return this;
        }

        public ICachedObjectWithUpdatesConfigurationManager<T, TUpdateFuncInput> WithUpdates<TUpdateFuncInput>(
            Func<T, TUpdateFuncInput, CancellationToken, Task<T>> updateValueFunc)
        {
            return new CachedObjectWithUpdatesConfigurationManager<T, TUpdateFuncInput>(_getValueFunc, updateValueFunc)
            {
                _refreshValueFuncTimeout = _refreshValueFuncTimeout,
                _refreshInterval = _refreshInterval,
                _refreshIntervalFactory = _refreshIntervalFactory,
                _onInitializedAction = _onInitializedAction,
                _onDisposedAction = _onDisposedAction,
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

            AddOnInitializedAction(cachedObject);
            AddOnDisposedAction(cachedObject);
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
        
        protected void AddOnInitializedAction(ICachedObject<T> cachedObject)
        {
            if (_onInitializedAction is null)
                return;

            cachedObject.OnInitialized += (obj, _) => _onInitializedAction((ICachedObject<T>)obj);
        }
        
        protected void AddOnDisposedAction(ICachedObject<T> cachedObject)
        {
            if (_onDisposedAction is null)
                return;

            cachedObject.OnDisposed += (obj, _) => _onDisposedAction((ICachedObject<T>)obj);
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