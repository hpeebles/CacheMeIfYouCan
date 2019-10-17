using System;

namespace CacheMeIfYouCan.Notifications
{
    public abstract class CachedObjectSuccessfulUpdateResultEventArgs<T> : EventArgs
    {
        internal CachedObjectSuccessfulUpdateResultEventArgs(CachedObjectSuccessfulUpdateResult<T> result)
        {
            Result = result;
        }
        
        public CachedObjectSuccessfulUpdateResult<T> Result { get; }
    }
    
    public sealed class CachedObjectSuccessfulUpdateResultEventArgs<T, TUpdates> : CachedObjectSuccessfulUpdateResultEventArgs<T>
    {
        internal CachedObjectSuccessfulUpdateResultEventArgs(CachedObjectSuccessfulUpdateResult<T, TUpdates> result)
            : base(result)
        {
            Result = result;
        }
        
        public new CachedObjectSuccessfulUpdateResult<T, TUpdates> Result { get; }
    }
}