using System;
using CacheMeIfYouCan.Events.CachedObject;

namespace CacheMeIfYouCan.Configuration
{
    public interface IUpdateableCachedObjectConfigurationManager<T, TUpdates>
    {
        IUpdateableCachedObjectConfigurationManager_WithRefreshInterval<T, TUpdates> WithRefreshInterval(TimeSpan refreshInterval);
        IUpdateableCachedObjectConfigurationManager<T, TUpdates> WithRefreshIntervalFactory(Func<TimeSpan> refreshIntervalFactory);
        IUpdateableCachedObjectConfigurationManager<T, TUpdates> WithRefreshValueFuncTimeout(TimeSpan timeout);
        IUpdateableCachedObjectConfigurationManager<T, TUpdates> OnInitialized(Action<IUpdateableCachedObject<T, TUpdates>> action);
        IUpdateableCachedObjectConfigurationManager<T, TUpdates> OnDisposed(Action<IUpdateableCachedObject<T, TUpdates>> action);
        IUpdateableCachedObjectConfigurationManager<T, TUpdates> OnValueRefresh(Action<ValueRefreshedEvent<T>> onSuccess = null, Action<ValueRefreshExceptionEvent<T>> onException = null);
        IUpdateableCachedObjectConfigurationManager<T, TUpdates> OnValueUpdate(Action<ValueUpdatedEvent<T, TUpdates>> onSuccess = null, Action<ValueUpdateExceptionEvent<T, TUpdates>> onException = null);
        IUpdateableCachedObject<T, TUpdates> Build();
    }

    public interface IUpdateableCachedObjectConfigurationManager_WithRefreshInterval<T, TUpdates> :
        IUpdateableCachedObjectConfigurationManager<T, TUpdates>
    {
        IUpdateableCachedObjectConfigurationManager<T, TUpdates> WithJitter(double jitterPercentage);
    }
}