using System;

namespace CacheMeIfYouCan.Notifications
{
    public class CachedObjectUpdateResultEventArgs<T, TUpdates> : EventArgs
    {
        internal CachedObjectUpdateResultEventArgs(CachedObjectUpdateResult<T, TUpdates> result)
        {
            Result = result;
        }
        
        public CachedObjectUpdateResult<T, TUpdates> Result { get; }
    }
}