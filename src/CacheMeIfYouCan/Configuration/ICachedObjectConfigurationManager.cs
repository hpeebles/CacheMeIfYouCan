using System;
using System.Threading;
using System.Threading.Tasks;

namespace CacheMeIfYouCan.Configuration
{
    public interface ICachedObjectConfigurationManager<T>
    {
        ICachedObjectConfigurationManager_WithRefreshInterval<T> WithRefreshInterval(TimeSpan refreshInterval);
        ICachedObjectConfigurationManager<T> WithRefreshIntervalFactory(Func<TimeSpan> refreshIntervalFactory);
        ICachedObjectConfigurationManager<T> WithRefreshValueFuncTimeout(TimeSpan timeout);
        ICachedObjectConfigurationManager<T> OnInitialized(Action<ICachedObject<T>> action);
        ICachedObjectConfigurationManager<T> OnDisposed(Action<ICachedObject<T>> action);
        ICachedObjectConfigurationManager<T> OnValueRefreshed(Action<CachedObjectValueRefreshedEvent<T>> action);
        ICachedObjectConfigurationManager<T> OnValueRefreshException(Action<CachedObjectValueRefreshExceptionEvent<T>> action);
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