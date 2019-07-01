using System;
using System.Threading.Tasks;
using CacheMeIfYouCan.Notifications;

namespace CacheMeIfYouCan
{
    public interface ICachedObject
    {
        string Name { get; }
        CachedObjectState State { get; }
        Task<CachedObjectInitializeOutcome> Initialize();
    }
    
    public interface ICachedObject<out T> : ICachedObject, IDisposable
    {
        T Value { get; }
        event EventHandler<CachedObjectUpdateExceptionEventArgs> OnException;
    }

    public interface ICachedObject<T, TUpdates> : ICachedObject<T>
    {
        event EventHandler<CachedObjectUpdateResultEventArgs<T, TUpdates>> OnUpdate;
    }
}