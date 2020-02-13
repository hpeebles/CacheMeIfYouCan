using System;

namespace CacheMeIfYouCan.Configuration
{
    public interface ICachedObjectWithUpdatesConfigurationManager<T, TUpdateFuncInput>
    {
        ICachedObjectWithUpdatesConfigurationManager_WithRefreshInterval<T, TUpdateFuncInput> WithRefreshInterval(TimeSpan refreshInterval);
        ICachedObjectWithUpdatesConfigurationManager<T, TUpdateFuncInput> WithRefreshIntervalFactory(Func<TimeSpan> refreshIntervalFactory);
        ICachedObjectWithUpdatesConfigurationManager<T, TUpdateFuncInput> WithRefreshValueFuncTimeout(TimeSpan timeout);
        ICachedObjectWithUpdatesConfigurationManager<T, TUpdateFuncInput> OnValueRefreshed(Action<CachedObjectValueRefreshedEvent<T>> action);
        ICachedObjectWithUpdatesConfigurationManager<T, TUpdateFuncInput> OnValueRefreshException(Action<CachedObjectValueRefreshExceptionEvent<T>> action);
        ICachedObjectWithUpdatesConfigurationManager<T, TUpdateFuncInput> OnValueUpdated(Action<CachedObjectValueUpdatedEvent<T, TUpdateFuncInput>> action);
        ICachedObjectWithUpdatesConfigurationManager<T, TUpdateFuncInput> OnValueUpdateException(Action<CachedObjectValueUpdateExceptionEvent<T, TUpdateFuncInput>> action);
        ICachedObjectWithUpdates<T, TUpdateFuncInput> Build();
    }

    public interface ICachedObjectWithUpdatesConfigurationManager_WithRefreshInterval<T, TUpdateFuncInput> :
        ICachedObjectWithUpdatesConfigurationManager<T, TUpdateFuncInput>
    {
        ICachedObjectWithUpdatesConfigurationManager<T, TUpdateFuncInput> WithJitter(double jitterPercentage);
    }
}