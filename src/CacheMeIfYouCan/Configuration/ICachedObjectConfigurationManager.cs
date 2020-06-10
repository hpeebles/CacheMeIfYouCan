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
        ICachedObjectConfigurationManager<T> OnValueRefresh(
            Action<ValueRefreshedEvent<T>> onSuccess = null, Action<ValueRefreshExceptionEvent<T>> onException = null);

        IIncrementalCachedObjectConfigurationManager<T, TUpdates> WithUpdates<TUpdates>(
            Func<T, TUpdates> getUpdatesFunc,
            Func<T, TUpdates, T> applyUpdatesFunc);
        
        IIncrementalCachedObjectConfigurationManager<T, TUpdates> WithUpdatesAsync<TUpdates>(
            Func<T, Task<TUpdates>> getUpdatesFunc,
            Func<T, TUpdates, Task<T>> applyUpdatesFunc);

        IIncrementalCachedObjectConfigurationManager<T, TUpdates> WithUpdatesAsync<TUpdates>(
            Func<T, CancellationToken, Task<TUpdates>> getUpdatesFunc,
            Func<T, TUpdates, CancellationToken, Task<T>> applyUpdatesFunc);

        IUpdateableCachedObjectConfigurationManager<T, TUpdates> WithUpdates<TUpdates>(
            Func<T, TUpdates, T> applyUpdatesFunc);
        
        IUpdateableCachedObjectConfigurationManager<T, TUpdates> WithUpdatesAsync<TUpdates>(
            Func<T, TUpdates, Task<T>> applyUpdatesFunc);

        IUpdateableCachedObjectConfigurationManager<T, TUpdates> WithUpdatesAsync<TUpdates>(
            Func<T, TUpdates, CancellationToken, Task<T>> applyUpdatesFunc);
        
        ICachedObject<T> Build();
    }

    public interface ICachedObjectConfigurationManager_WithRefreshInterval<T> : ICachedObjectConfigurationManager<T>
    {
        ICachedObjectConfigurationManager<T> WithJitter(double jitterPercentage);
    }
}