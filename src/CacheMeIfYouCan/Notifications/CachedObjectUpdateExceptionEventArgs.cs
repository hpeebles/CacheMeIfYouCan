using System;

namespace CacheMeIfYouCan.Notifications
{
    public class CachedObjectUpdateExceptionEventArgs : EventArgs
    {
        internal CachedObjectUpdateExceptionEventArgs(CachedObjectUpdateException exception)
        {
            Exception = exception;
        }
        
        public CachedObjectUpdateException Exception { get; }
    }
}