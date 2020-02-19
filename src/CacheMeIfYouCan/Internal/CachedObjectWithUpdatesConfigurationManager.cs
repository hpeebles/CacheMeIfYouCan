using System;
using System.Threading;
using System.Threading.Tasks;
using CacheMeIfYouCan.Configuration;

namespace CacheMeIfYouCan.Internal
{
    internal sealed class CachedObjectWithUpdatesConfigurationManager<T, TUpdateFuncInput> :
        CachedObjectConfigurationManager<T>,
        ICachedObjectWithUpdatesConfigurationManager<T, TUpdateFuncInput>,
        ICachedObjectWithUpdatesConfigurationManager_WithRefreshInterval<T, TUpdateFuncInput>
    {
        private readonly Func<T, TUpdateFuncInput, CancellationToken, Task<T>> _updateValueFunc;
        private Action<CachedObjectValueUpdatedEvent<T, TUpdateFuncInput>> _onValueUpdatedAction;
        private Action<CachedObjectValueUpdateExceptionEvent<T, TUpdateFuncInput>> _onValueUpdateExceptionAction;

        public CachedObjectWithUpdatesConfigurationManager(
            Func<CancellationToken, Task<T>> getValueFunc,
            Func<T, TUpdateFuncInput, CancellationToken, Task<T>> updateValueFunc)
            : base(getValueFunc)
        {
            _updateValueFunc = updateValueFunc;
        }

        public new ICachedObjectWithUpdatesConfigurationManager_WithRefreshInterval<T, TUpdateFuncInput> WithRefreshInterval(TimeSpan refreshInterval)
        {
            base.WithRefreshInterval(refreshInterval);
            return this;
        }

        public new ICachedObjectWithUpdatesConfigurationManager<T, TUpdateFuncInput> WithRefreshIntervalFactory(Func<TimeSpan> refreshIntervalFactory)
        {
            base.WithRefreshIntervalFactory(refreshIntervalFactory);
            return this;
        }

        public new ICachedObjectWithUpdatesConfigurationManager<T, TUpdateFuncInput> WithRefreshValueFuncTimeout(TimeSpan timeout)
        {
            base.WithRefreshValueFuncTimeout(timeout);
            return this;
        }

        public new ICachedObjectWithUpdatesConfigurationManager<T, TUpdateFuncInput> OnInitialized(Action<ICachedObject<T>> action)
        {
            base.OnInitialized(action);
            return this;
        }

        public new ICachedObjectWithUpdatesConfigurationManager<T, TUpdateFuncInput> OnDisposed(Action<ICachedObject<T>> action)
        {
            base.OnDisposed(action);
            return this;
        }

        public new ICachedObjectWithUpdatesConfigurationManager<T, TUpdateFuncInput> OnValueRefreshed(Action<CachedObjectValueRefreshedEvent<T>> action)
        {
            base.OnValueRefreshed(action);
            return this;
        }

        public new ICachedObjectWithUpdatesConfigurationManager<T, TUpdateFuncInput> OnValueRefreshException(Action<CachedObjectValueRefreshExceptionEvent<T>> action)
        {
            base.OnValueRefreshException(action);
            return this;
        }
        
        public ICachedObjectWithUpdatesConfigurationManager<T, TUpdateFuncInput> OnValueUpdated(
            Action<CachedObjectValueUpdatedEvent<T, TUpdateFuncInput>> action)
        {
            _onValueUpdatedAction += action;
            return this;
        }

        public ICachedObjectWithUpdatesConfigurationManager<T, TUpdateFuncInput> OnValueUpdateException(
            Action<CachedObjectValueUpdateExceptionEvent<T, TUpdateFuncInput>> action)
        {
            _onValueUpdateExceptionAction += action;
            return this;
        }

        ICachedObjectWithUpdatesConfigurationManager<T, TUpdateFuncInput> ICachedObjectWithUpdatesConfigurationManager_WithRefreshInterval<T, TUpdateFuncInput>.WithJitter(double jitterPercentage)
        {
            ((ICachedObjectConfigurationManager_WithRefreshInterval<T>)this).WithJitter(jitterPercentage);
            return this;
        }

        public new ICachedObjectWithUpdates<T, TUpdateFuncInput> Build()
        {
            var refreshIntervalFactory = GetRefreshIntervalFactory();
            
            var cachedObject = new CachedObject<T, TUpdateFuncInput>(
                _getValueFunc,
                _updateValueFunc,
                refreshIntervalFactory,
                _refreshValueFuncTimeout);

            AddOnInitializedAction(cachedObject);
            AddOnDisposedAction(cachedObject);
            AddOnValueRefreshedActions(cachedObject);
            AddOnValueUpdatedActions(cachedObject);
            
            return cachedObject;
        }
        
        private void AddOnValueUpdatedActions(ICachedObjectWithUpdates<T, TUpdateFuncInput> cachedObject)
        {
            if (!(_onValueUpdatedAction is null))
                cachedObject.OnValueUpdated += (_, e) => _onValueUpdatedAction(e);

            if (!(_onValueUpdateExceptionAction is null))
                cachedObject.OnValueUpdateException += (_, e) => _onValueUpdateExceptionAction(e);
        }
    }
}