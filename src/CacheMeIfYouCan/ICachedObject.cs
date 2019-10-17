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
    
    public interface ICachedObject<T> : ICachedObject, IDisposable
    {
        /// <summary>
        /// Gets the current value of the <see cref="ICachedObject{T}"/>
        /// </summary>
        T Value { get; }
        
        /// <summary>
        /// Creates a new <see cref="ICachedObject{T}"/> whose value is calculated by applying the <see cref="mapFunc"/>
        /// function to the value of the source <see cref="ICachedObject{T}" /> each time the source value is updated
        /// </summary>
        ICachedObject<TOut, T> Map<TOut>(Func<T, TOut> mapFunc, string name = null);
        
        event EventHandler<CachedObjectSuccessfulUpdateResultEventArgs<T>> OnValueUpdated;
        
        event EventHandler<CachedObjectUpdateExceptionEventArgs<T>> OnException;
    }

    public interface ICachedObject<T, TUpdates> : ICachedObject<T>
    {
        new event EventHandler<CachedObjectSuccessfulUpdateResultEventArgs<T, TUpdates>> OnValueUpdated;
        
        new event EventHandler<CachedObjectUpdateExceptionEventArgs<T, TUpdates>> OnException;
    }
}