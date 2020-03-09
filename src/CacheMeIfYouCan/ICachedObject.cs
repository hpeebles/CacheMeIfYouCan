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
        event EventHandler OnInitialized;
        event EventHandler OnDisposed;
        event EventHandler<ValueRefreshedEvent<T>> OnValueRefreshed;
        event EventHandler<ValueRefreshExceptionEvent<T>> OnValueRefreshException;
    }
}