using System;

namespace CacheMeIfYouCan.Notifications
{
    public abstract class CachedObjectUpdateExceptionEventArgs<T> : EventArgs
    {
        internal CachedObjectUpdateExceptionEventArgs(CachedObjectUpdateException<T> exception)
        {
            Exception = exception;
        }
        
        public CachedObjectUpdateException<T> Exception { get; }
    }
    
    public sealed class CachedObjectUpdateExceptionEventArgs<T, TUpdates> : CachedObjectUpdateExceptionEventArgs<T>
    {
        internal CachedObjectUpdateExceptionEventArgs(CachedObjectUpdateException<T, TUpdates> exception)
            : base(exception)
        {
            Exception = exception;
        }
        
        public new CachedObjectUpdateException<T, TUpdates> Exception { get; }
    }
}