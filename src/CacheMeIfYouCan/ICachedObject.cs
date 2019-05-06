using System;
using System.Threading.Tasks;
using CacheMeIfYouCan.Notifications;

namespace CacheMeIfYouCan
{
    public interface ICachedObject<out T> : ICachedObjectInitializer, IDisposable
    {
        T Value { get; }
        event EventHandler<CachedObjectUpdateExceptionEventArgs> OnException;
    }

    public interface ICachedObject<T, TUpdates> : ICachedObject<T>
    {
        event EventHandler<CachedObjectUpdateResultEventArgs<T, TUpdates>> OnUpdate;
    }

    public interface ICachedObjectInitializer
    {
        string Name { get; }
        
        Task<CachedObjectInitializeOutcome> Initialize();
    }
}