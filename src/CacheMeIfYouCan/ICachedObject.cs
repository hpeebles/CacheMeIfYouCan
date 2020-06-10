using System;
using System.Threading;
using System.Threading.Tasks;
using CacheMeIfYouCan.Events.CachedObject;

namespace CacheMeIfYouCan
{
    public interface ICachedObject<T> : IDisposable
    {
        T Value { get; }
        CachedObjectState State { get; }
        long Version { get; }
        void Initialize(CancellationToken cancellationToken = default);
        Task InitializeAsync(CancellationToken cancellationToken = default);
        void RefreshValue(TimeSpan skipIfPreviousRefreshStartedWithinTimeFrame = default, CancellationToken cancellationToken = default);
        Task RefreshValueAsync(TimeSpan skipIfPreviousRefreshStartedWithinTimeFrame = default, CancellationToken cancellationToken = default);
        ICachedObject<TOut> Map<TOut>(Func<T, TOut> map);
        ICachedObject<TOut> MapAsync<TOut>(Func<T, Task<TOut>> map);
        event EventHandler OnInitialized;
        event EventHandler OnDisposed;
        event EventHandler<ValueRefreshedEvent<T>> OnValueRefreshed;
        event EventHandler<ValueRefreshExceptionEvent<T>> OnValueRefreshException;
    }

    public interface ICachedObject<T, TUpdates> : ICachedObject<T>
    {
        new ICachedObject<TOut, TUpdates> Map<TOut>(Func<T, TOut> map);
        new ICachedObject<TOut, TUpdates> MapAsync<TOut>(Func<T, Task<TOut>> map);
        ICachedObject<TOut, TUpdates> Map<TOut>(Func<T, TOut> map, Func<TOut, T, TUpdates, TOut> mapUpdatesFunc);
        ICachedObject<TOut, TUpdates> MapAsync<TOut>(Func<T, Task<TOut>> map, Func<TOut, T, TUpdates, Task<TOut>> mapUpdatesFunc);
        event EventHandler<ValueUpdatedEvent<T, TUpdates>> OnValueUpdated;
        event EventHandler<ValueUpdateExceptionEvent<T, TUpdates>> OnValueUpdateException;
    }
}