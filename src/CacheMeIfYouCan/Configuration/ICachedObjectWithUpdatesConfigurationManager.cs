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
        ICachedObjectWithUpdatesConfigurationManager<T, TUpdateFuncInput> OnValueRefreshed(Action<ValueRefreshedEvent<T>> action);
        ICachedObjectWithUpdatesConfigurationManager<T, TUpdateFuncInput> OnValueRefreshException(Action<ValueRefreshExceptionEvent<T>> action);
        ICachedObjectWithUpdatesConfigurationManager<T, TUpdateFuncInput> OnValueUpdated(Action<ValueUpdatedEvent<T, TUpdateFuncInput>> action);
        ICachedObjectWithUpdatesConfigurationManager<T, TUpdateFuncInput> OnValueUpdateException(Action<ValueUpdateExceptionEvent<T, TUpdateFuncInput>> action);
        ICachedObjectWithUpdates<T, TUpdateFuncInput> Build();
    }

    public interface ICachedObjectWithUpdatesConfigurationManager_WithRefreshInterval<T, TUpdateFuncInput> :
        ICachedObjectWithUpdatesConfigurationManager<T, TUpdateFuncInput>
    {
        ICachedObjectWithUpdatesConfigurationManager<T, TUpdateFuncInput> WithJitter(double jitterPercentage);
    }
}