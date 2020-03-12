using System;
using CacheMeIfYouCan.Events.CachedObject;

namespace CacheMeIfYouCan.Configuration
{
    public interface ICachedObjectWithUpdatesConfigurationManager<T, TUpdateFuncInput>
    {
        ICachedObjectWithUpdatesConfigurationManager_WithRefreshInterval<T, TUpdateFuncInput> WithRefreshInterval(TimeSpan refreshInterval);
        ICachedObjectWithUpdatesConfigurationManager<T, TUpdateFuncInput> WithRefreshIntervalFactory(Func<TimeSpan> refreshIntervalFactory);
        ICachedObjectWithUpdatesConfigurationManager<T, TUpdateFuncInput> WithRefreshValueFuncTimeout(TimeSpan timeout);
        ICachedObjectWithUpdatesConfigurationManager<T, TUpdateFuncInput> OnInitialized(Action<ICachedObject<T>> action);
        ICachedObjectWithUpdatesConfigurationManager<T, TUpdateFuncInput> OnDisposed(Action<ICachedObject<T>> action);
        ICachedObjectWithUpdatesConfigurationManager<T, TUpdateFuncInput> OnValueRefresh(Action<ValueRefreshedEvent<T>> onSuccess = null, Action<ValueRefreshExceptionEvent<T>> onException = null);
        ICachedObjectWithUpdatesConfigurationManager<T, TUpdateFuncInput> OnValueUpdate(Action<ValueUpdatedEvent<T, TUpdateFuncInput>> onSuccess = null, Action<ValueUpdateExceptionEvent<T, TUpdateFuncInput>> onException = null);
        ICachedObjectWithUpdates<T, TUpdateFuncInput> Build();
    }

    public interface ICachedObjectWithUpdatesConfigurationManager_WithRefreshInterval<T, TUpdateFuncInput> :
        ICachedObjectWithUpdatesConfigurationManager<T, TUpdateFuncInput>
    {
        ICachedObjectWithUpdatesConfigurationManager<T, TUpdateFuncInput> WithJitter(double jitterPercentage);
    }
}