using System;
using System.Threading;
using System.Threading.Tasks;
using CacheMeIfYouCan.Configuration;
using CacheMeIfYouCan.Events.CachedObject;

namespace CacheMeIfYouCan.Internal.CachedObjects
{
    internal sealed class UpdateableCachedObjectConfigurationManager<T, TUpdates> :
        CachedObjectConfigurationManager<T>,
        IUpdateableCachedObjectConfigurationManager<T, TUpdates>,
        IUpdateableCachedObjectConfigurationManager_WithRefreshInterval<T, TUpdates>
    {
        private readonly Func<T, TUpdates, CancellationToken, Task<T>> _applyUpdatesFunc;
        private Action<ValueUpdatedEvent<T, TUpdates>> _onValueUpdatedAction;
        private Action<ValueUpdateExceptionEvent<T, TUpdates>> _onValueUpdateExceptionAction;

        public UpdateableCachedObjectConfigurationManager(
            Func<CancellationToken, Task<T>> getValueFunc,
            Func<T, TUpdates, CancellationToken, Task<T>> applyUpdatesFunc)
            : base(getValueFunc)
        {
            _applyUpdatesFunc = applyUpdatesFunc;
        }

        public new IUpdateableCachedObjectConfigurationManager_WithRefreshInterval<T, TUpdates> WithRefreshInterval(TimeSpan refreshInterval)
        {
            base.WithRefreshInterval(refreshInterval);
            return this;
        }

        public new IUpdateableCachedObjectConfigurationManager<T, TUpdates> WithRefreshIntervalFactory(Func<TimeSpan> refreshIntervalFactory)
        {
            base.WithRefreshIntervalFactory(refreshIntervalFactory);
            return this;
        }

        public new IUpdateableCachedObjectConfigurationManager<T, TUpdates> WithRefreshValueFuncTimeout(TimeSpan timeout)
        {
            base.WithRefreshValueFuncTimeout(timeout);
            return this;
        }

        public IUpdateableCachedObjectConfigurationManager<T, TUpdates> WithUpdateIntervalFactory(Func<TimeSpan> updateIntervalFactory)
        {
            throw new NotImplementedException();
        }

        public new IUpdateableCachedObjectConfigurationManager<T, TUpdates> OnInitialized(Action<ICachedObject<T>> action)
        {
            base.OnInitialized(action);
            return this;
        }

        public new IUpdateableCachedObjectConfigurationManager<T, TUpdates> OnDisposed(Action<ICachedObject<T>> action)
        {
            base.OnDisposed(action);
            return this;
        }

        public new IUpdateableCachedObjectConfigurationManager<T, TUpdates> OnValueRefresh(
            Action<ValueRefreshedEvent<T>> onSuccess,
            Action<ValueRefreshExceptionEvent<T>> onException)
        {
            base.OnValueRefresh(onSuccess, onException);
            return this;
        }
        
        public IUpdateableCachedObjectConfigurationManager<T, TUpdates> OnValueUpdate(
            Action<ValueUpdatedEvent<T, TUpdates>> onSuccess,
            Action<ValueUpdateExceptionEvent<T, TUpdates>> onException)
        {
            if (!(onSuccess is null))
                _onValueUpdatedAction += onSuccess;
            
            if (!(onException is null))
                _onValueUpdateExceptionAction += onException;
            
            return this;
        }

        IUpdateableCachedObjectConfigurationManager<T, TUpdates> IUpdateableCachedObjectConfigurationManager_WithRefreshInterval<T, TUpdates>.WithJitter(double jitterPercentage)
        {
            ((ICachedObjectConfigurationManager_WithRefreshInterval<T>)this).WithJitter(jitterPercentage);
            return this;
        }

        public new IUpdateableCachedObject<T, TUpdates> Build()
        {
            var refreshIntervalFactory = GetRefreshIntervalFactory();
            
            var cachedObject = new UpdateableCachedObject<T, TUpdates>(
                _getValueFunc,
                _applyUpdatesFunc,
                refreshIntervalFactory,
                _refreshValueFuncTimeout);

            AddOnInitializedAction(cachedObject);
            AddOnDisposedAction(cachedObject);
            AddOnValueRefreshedActions(cachedObject);
            AddOnValueUpdatedActions(cachedObject);
            
            return cachedObject;
        }
        
        private void AddOnValueUpdatedActions(IUpdateableCachedObject<T, TUpdates> cachedObject)
        {
            if (!(_onValueUpdatedAction is null))
                cachedObject.OnValueUpdated += (_, e) => _onValueUpdatedAction(e);

            if (!(_onValueUpdateExceptionAction is null))
                cachedObject.OnValueUpdateException += (_, e) => _onValueUpdateExceptionAction(e);
        }
    }
}