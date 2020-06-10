using System;
using System.Threading;
using System.Threading.Tasks;
using CacheMeIfYouCan.Configuration;
using CacheMeIfYouCan.Events.CachedObject;

namespace CacheMeIfYouCan.Internal.CachedObjects
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
        
        public ICachedObjectConfigurationManager<T> OnValueRefresh(
            Action<ValueRefreshedEvent<T>> onSuccess,
            Action<ValueRefreshExceptionEvent<T>> onException)
        {
            if (!(onSuccess is null))
                _onValueRefreshedAction += onSuccess;
            
            if (!(onException is null))
                _onValueRefreshExceptionAction += onException;
            
            return this;
        }
        
        public IIncrementalCachedObjectConfigurationManager<T, TUpdates> WithUpdates<TUpdates>(
            Func<T, TUpdates> getUpdatesFunc,
            Func<T, TUpdates, T> applyUpdatesFunc)
        {
            return WithUpdatesAsync(
                v => Task.FromResult(getUpdatesFunc(v)),
                (v, u) => Task.FromResult(applyUpdatesFunc(v, u)));
        }
        
        public IIncrementalCachedObjectConfigurationManager<T, TUpdates> WithUpdatesAsync<TUpdates>(
            Func<T, Task<TUpdates>> getUpdatesFunc,
            Func<T, TUpdates, Task<T>> applyUpdatesFunc)
        {
            return WithUpdatesAsync(
                (v, _) => getUpdatesFunc(v),
                (v, u, _) => applyUpdatesFunc(v, u));
        }

        public IIncrementalCachedObjectConfigurationManager<T, TUpdates> WithUpdatesAsync<TUpdates>(
            Func<T, CancellationToken, Task<TUpdates>> getUpdatesFunc,
            Func<T, TUpdates, CancellationToken, Task<T>> applyUpdatesFunc)
        {
            return new IncrementalCachedObjectConfigurationManager<T, TUpdates>(_getValueFunc, getUpdatesFunc, applyUpdatesFunc)
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

        public IUpdateableCachedObjectConfigurationManager<T, TUpdates> WithUpdates<TUpdates>(
            Func<T, TUpdates, T> applyUpdatesFunc)
        {
            return WithUpdatesAsync<TUpdates>((v, u) => Task.FromResult(applyUpdatesFunc(v, u)));
        }
        
        public IUpdateableCachedObjectConfigurationManager<T, TUpdates> WithUpdatesAsync<TUpdates>(
            Func<T, TUpdates, Task<T>> applyUpdatesFunc)
        {
            return WithUpdatesAsync<TUpdates>((v, u, _) => applyUpdatesFunc(v, u));
        }

        public IUpdateableCachedObjectConfigurationManager<T, TUpdates> WithUpdatesAsync<TUpdates>(
            Func<T, TUpdates, CancellationToken, Task<T>> applyUpdatesFunc)
        {
            return new UpdateableCachedObjectConfigurationManager<T, TUpdates>(_getValueFunc, applyUpdatesFunc)
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

        public ICachedObject<T> Build()
        {
            var refreshIntervalFactory = GetRefreshIntervalFactory();
            
            var cachedObject = new CachedObject<T>(
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