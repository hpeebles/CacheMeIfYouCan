using System;
using System.Threading;
using System.Threading.Tasks;

namespace CacheMeIfYouCan
{
    public interface ICachedObject<T> : IDisposable
    {
        T Value { get; }
        
        CachedObjectState State { get; }
        
        long Version { get; }

        void Initialize(CancellationToken cancellationToken = default);

        Task InitializeAsync(CancellationToken cancellationToken = default);

        void RefreshValue(CancellationToken cancellationToken = default);
        
        Task RefreshValueAsync(CancellationToken cancellationToken = default);
        
        event EventHandler<CachedObjectValueRefreshedEvent<T>> OnValueRefreshed;
        
        event EventHandler<CachedObjectValueRefreshExceptionEvent<T>> OnValueRefreshException;
    }
}