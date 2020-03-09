using System;
using System.Threading;
using System.Threading.Tasks;
using CacheMeIfYouCan.Events.CachedObject;

namespace CacheMeIfYouCan.Configuration
{
    public interface ICachedObjectConfigurationManager<T>
    {
        ICachedObjectConfigurationManager_WithRefreshInterval<T> WithRefreshInterval(TimeSpan refreshInterval);
        ICachedObjectConfigurationManager<T> WithRefreshIntervalFactory(Func<TimeSpan> refreshIntervalFactory);
        ICachedObjectConfigurationManager<T> WithRefreshValueFuncTimeout(TimeSpan timeout);
        ICachedObjectConfigurationManager<T> OnInitialized(Action<ICachedObject<T>> action);
        ICachedObjectConfigurationManager<T> OnDisposed(Action<ICachedObject<T>> action);
        ICachedObjectConfigurationManager<T> OnValueRefreshed(Action<ValueRefreshedEvent<T>> action);
        ICachedObjectConfigurationManager<T> OnValueRefreshException(Action<ValueRefreshExceptionEvent<T>> action);
        ICachedObjectWithUpdatesConfigurationManager<T, TUpdateFuncInput> WithUpdates<TUpdateFuncInput>(Func<T, TUpdateFuncInput, CancellationToken, Task<T>> updateValueFunc);
        ICachedObjectWithUpdatesConfigurationManager<T, TUpdateFuncInput> WithUpdates<TUpdateFuncInput>(Func<T, TUpdateFuncInput, CancellationToken, T> updateValueFunc);
        ICachedObjectWithUpdatesConfigurationManager<T, TUpdateFuncInput> WithUpdates<TUpdateFuncInput>(Func<T, TUpdateFuncInput, Task<T>> updateValueFunc);
        ICachedObjectWithUpdatesConfigurationManager<T, TUpdateFuncInput> WithUpdates<TUpdateFuncInput>(Func<T, TUpdateFuncInput, T> updateValueFunc);
        ICachedObject<T> Build();
    }
    
    public interface ICachedObjectConfigurationManager_WithRefreshInterval<T> : ICachedObjectConfigurationManager<T>
    {
        ICachedObjectConfigurationManager<T> WithJitter(double jitterPercentage);
    }
}