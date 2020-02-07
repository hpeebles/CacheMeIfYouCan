using System;

namespace CacheMeIfYouCan.Configuration
{
    public interface ICachedObjectConfigurationManager<T>
    {
        ICachedObjectConfigurationManager_WithRefreshInterval<T> WithRefreshInterval(TimeSpan refreshInterval);
        ICachedObjectConfigurationManager<T> WithRefreshIntervalFactory(Func<TimeSpan> refreshIntervalFactory);
        ICachedObjectConfigurationManager<T> WithRefreshValueFuncTimeout(TimeSpan timeout);
        ICachedObjectConfigurationManager<T> OnValueRefreshed(Action<CachedObjectValueRefreshedEvent<T>> action);
        ICachedObjectConfigurationManager<T> OnValueRefreshException(Action<CachedObjectValueRefreshExceptionEvent<T>> action);
        ICachedObject<T> Build();
    }
    
    public interface ICachedObjectConfigurationManager_WithRefreshInterval<T> : ICachedObjectConfigurationManager<T>
    {
        ICachedObjectConfigurationManager<T> WithJitter(double jitterPercentage);
    }
}