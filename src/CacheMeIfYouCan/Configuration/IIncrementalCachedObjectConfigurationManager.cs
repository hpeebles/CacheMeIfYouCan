using System;
using CacheMeIfYouCan.Events.CachedObject;

namespace CacheMeIfYouCan.Configuration
{
    public interface IIncrementalCachedObjectConfigurationManager<T, TUpdates>
    {
        IIncrementalCachedObjectConfigurationManager_WithRefreshInterval<T, TUpdates> WithRefreshInterval(TimeSpan refreshInterval);
        IIncrementalCachedObjectConfigurationManager<T, TUpdates> WithRefreshIntervalFactory(Func<TimeSpan> refreshIntervalFactory);
        IIncrementalCachedObjectConfigurationManager<T, TUpdates> WithRefreshValueFuncTimeout(TimeSpan timeout);
        IIncrementalCachedObjectConfigurationManager_WithUpdateInterval<T, TUpdates> WithUpdateInterval(TimeSpan updateInterval);
        IIncrementalCachedObjectConfigurationManager<T, TUpdates> WithUpdateIntervalFactory(Func<TimeSpan> updateIntervalFactory);
        IIncrementalCachedObjectConfigurationManager<T, TUpdates> OnInitialized(Action<ICachedObject<T>> action);
        IIncrementalCachedObjectConfigurationManager<T, TUpdates> OnDisposed(Action<ICachedObject<T>> action);
        IIncrementalCachedObjectConfigurationManager<T, TUpdates> OnValueRefresh(Action<ValueRefreshedEvent<T>> onSuccess = null, Action<ValueRefreshExceptionEvent<T>> onException = null);
        IIncrementalCachedObjectConfigurationManager<T, TUpdates> OnValueUpdate(Action<ValueUpdatedEvent<T, TUpdates>> onSuccess = null, Action<ValueUpdateExceptionEvent<T, TUpdates>> onException = null);
        IIncrementalCachedObject<T, TUpdates> Build();
    }

    public interface IIncrementalCachedObjectConfigurationManager_WithRefreshInterval<T, TUpdates> :
        IIncrementalCachedObjectConfigurationManager<T, TUpdates>
    {
        IIncrementalCachedObjectConfigurationManager<T, TUpdates> WithJitter(double jitterPercentage);
    }
    
    public interface IIncrementalCachedObjectConfigurationManager_WithUpdateInterval<T, TUpdates> :
        IIncrementalCachedObjectConfigurationManager<T, TUpdates>
    {
        IIncrementalCachedObjectConfigurationManager<T, TUpdates> WithJitter(double jitterPercentage);
    }
}