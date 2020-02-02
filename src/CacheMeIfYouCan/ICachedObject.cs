using System;
using System.Threading;
using System.Threading.Tasks;

namespace CacheMeIfYouCan
{
    public interface ICachedObject<out T> : IDisposable
    {
        T Value { get; }
        
        CachedObjectState State { get; }
        
        long Version { get; }

        void Initialize(CancellationToken cancellationToken = default);

        Task InitializeAsync(CancellationToken cancellationToken = default);
    }
}