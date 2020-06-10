using System;
using System.Threading;
using System.Threading.Tasks;
using CacheMeIfYouCan.Configuration;
using CacheMeIfYouCan.Events.CachedObject;

namespace CacheMeIfYouCan.Internal.CachedObjects
{
    internal sealed class IncrementalCachedObjectConfigurationManager<T, TUpdates> :
        CachedObjectConfigurationManager<T>,
        IIncrementalCachedObjectConfigurationManager<T, TUpdates>,
        IIncrementalCachedObjectConfigurationManager_WithRefreshInterval<T, TUpdates>,
        IIncrementalCachedObjectConfigurationManager_WithUpdateInterval<T, TUpdates>
    {
        private readonly Func<T, CancellationToken, Task<TUpdates>> _getUpdatesFunc;
        private readonly Func<T, TUpdates, CancellationToken, Task<T>> _applyUpdatesFunc;
        private TimeSpan? _updateInterval;
        private Func<TimeSpan> _updateIntervalFactory;
        private Action<ValueUpdatedEvent<T, TUpdates>> _onValueUpdatedAction;
        private Action<ValueUpdateExceptionEvent<T, TUpdates>> _onValueUpdateExceptionAction;

        public IncrementalCachedObjectConfigurationManager(
            Func<CancellationToken, Task<T>> getValueFunc,
            Func<T, CancellationToken, Task<TUpdates>> getUpdatesFunc,
            Func<T, TUpdates, CancellationToken, Task<T>> applyUpdatesFunc)
            : base(getValueFunc)
        {
            _getUpdatesFunc = getUpdatesFunc;
            _applyUpdatesFunc = applyUpdatesFunc;
        }

        public new IIncrementalCachedObjectConfigurationManager_WithRefreshInterval<T, TUpdates> WithRefreshInterval(TimeSpan refreshInterval)
        {
            base.WithRefreshInterval(refreshInterval);
            return this;
        }

        public new IIncrementalCachedObjectConfigurationManager<T, TUpdates> WithRefreshIntervalFactory(Func<TimeSpan> refreshIntervalFactory)
        {
            base.WithRefreshIntervalFactory(refreshIntervalFactory);
            return this;
        }

        public new IIncrementalCachedObjectConfigurationManager<T, TUpdates> WithRefreshValueFuncTimeout(TimeSpan timeout)
        {
            base.WithRefreshValueFuncTimeout(timeout);
            return this;
        }

        public IIncrementalCachedObjectConfigurationManager_WithUpdateInterval<T, TUpdates> WithUpdateInterval(TimeSpan updateInterval)
        {
            SetUpdateInterval(updateInterval);
            return this;
        }

        public IIncrementalCachedObjectConfigurationManager<T, TUpdates> WithUpdateIntervalFactory(Func<TimeSpan> updateIntervalFactory)
        {
            SetUpdateIntervalFactory(updateIntervalFactory);
            return this;
        }

        public new IIncrementalCachedObjectConfigurationManager<T, TUpdates> OnInitialized(Action<ICachedObject<T>> action)
        {
            base.OnInitialized(action);
            return this;
        }

        public new IIncrementalCachedObjectConfigurationManager<T, TUpdates> OnDisposed(Action<ICachedObject<T>> action)
        {
            base.OnDisposed(action);
            return this;
        }

        public new IIncrementalCachedObjectConfigurationManager<T, TUpdates> OnValueRefresh(
            Action<ValueRefreshedEvent<T>> onSuccess,
            Action<ValueRefreshExceptionEvent<T>> onException)
        {
            base.OnValueRefresh(onSuccess, onException);
            return this;
        }
        
        public IIncrementalCachedObjectConfigurationManager<T, TUpdates> OnValueUpdate(
            Action<ValueUpdatedEvent<T, TUpdates>> onSuccess,
            Action<ValueUpdateExceptionEvent<T, TUpdates>> onException)
        {
            if (!(onSuccess is null))
                _onValueUpdatedAction += onSuccess;
            
            if (!(onException is null))
                _onValueUpdateExceptionAction += onException;
            
            return this;
        }

        IIncrementalCachedObjectConfigurationManager<T, TUpdates> IIncrementalCachedObjectConfigurationManager_WithRefreshInterval<T, TUpdates>.WithJitter(double jitterPercentage)
        {
            ((ICachedObjectConfigurationManager_WithRefreshInterval<T>)this).WithJitter(jitterPercentage);
            return this;
        }

        IIncrementalCachedObjectConfigurationManager<T, TUpdates> IIncrementalCachedObjectConfigurationManager_WithUpdateInterval<T, TUpdates>.WithJitter(double jitterPercentage)
        {
                if (jitterPercentage < 0 || jitterPercentage >= 100)
                    throw new ArgumentOutOfRangeException(nameof(jitterPercentage));

                if (!_updateInterval.HasValue)
                    throw new InvalidOperationException($"You can only use {nameof(IIncrementalCachedObjectConfigurationManager_WithUpdateInterval<T, TUpdates>.WithJitter)} after first using {nameof(WithUpdateInterval)}");
            
                var jitterHandler = new JitterHandler(_updateInterval.Value, jitterPercentage);

                SetUpdateIntervalFactory(() => jitterHandler.GetNext());
                return this;
        }

        public new IIncrementalCachedObject<T, TUpdates> Build()
        {
            var refreshIntervalFactory = GetRefreshIntervalFactory();
            var updateIntervalFactory = GetUpdateIntervalFactory();
            
            var cachedObject = new IncrementalCachedObject<T, TUpdates>(
                _getValueFunc,
                _getUpdatesFunc,
                _applyUpdatesFunc,
                refreshIntervalFactory,
                updateIntervalFactory,
                _refreshValueFuncTimeout);

            AddOnInitializedAction(cachedObject);
            AddOnDisposedAction(cachedObject);
            AddOnValueRefreshedActions(cachedObject);
            AddOnValueUpdatedActions(cachedObject);
            
            return cachedObject;
        }
        
        private void AddOnValueUpdatedActions(IIncrementalCachedObject<T, TUpdates> cachedObject)
        {
            if (!(_onValueUpdatedAction is null))
                cachedObject.OnValueUpdated += (_, e) => _onValueUpdatedAction(e);

            if (!(_onValueUpdateExceptionAction is null))
                cachedObject.OnValueUpdateException += (_, e) => _onValueUpdateExceptionAction(e);
        }
        
        private Func<TimeSpan> GetUpdateIntervalFactory()
        {
            var updateIntervalFactory = _updateIntervalFactory;
            if (updateIntervalFactory is null && _updateInterval.HasValue)
            {
                var updateInterval = _updateInterval.Value;
                updateIntervalFactory = () => updateInterval;
            }

            return updateIntervalFactory;
        }

        private void SetUpdateInterval(TimeSpan updateInterval)
        {
            _updateIntervalFactory = null;
            _updateInterval = updateInterval;
        }

        private void SetUpdateIntervalFactory(Func<TimeSpan> updateIntervalFactory)
        {
            _updateInterval = null;
            _updateIntervalFactory = updateIntervalFactory;
        }
    }
}