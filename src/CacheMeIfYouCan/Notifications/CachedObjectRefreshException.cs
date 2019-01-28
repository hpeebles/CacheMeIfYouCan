using System;
using CacheMeIfYouCan.Internal;

namespace CacheMeIfYouCan.Notifications
{
    public sealed class CachedObjectRefreshException<T> : CachedObjectRefreshException
    {
        private static readonly string ExceptionMessage =
            $"{nameof(CachedObject<T>)} threw an exception while trying to refresh its value";
        
        internal CachedObjectRefreshException(Exception exception)
            : base(ExceptionMessage, exception)
        { }
    }

    public abstract class CachedObjectRefreshException : Exception
    {
        internal CachedObjectRefreshException(string message, Exception ex)
            : base(message, ex)
        { }

        public long Timestamp { get; } = CacheMeIfYouCan.Timestamp.Now;
    }
}