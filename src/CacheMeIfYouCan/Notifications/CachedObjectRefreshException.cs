using System;

namespace CacheMeIfYouCan.Notifications
{
    public sealed class CachedObjectRefreshException<T> : CachedObjectRefreshException
    {
        private const string ExceptionMessageFormat =
            "{0} threw an exception while trying to refresh its value";
        
        internal CachedObjectRefreshException(string name, Exception exception)
            : base(String.Format(ExceptionMessageFormat, name), exception)
        {
            Name = name;
        }
        
        public string Name { get; }
    }

    public abstract class CachedObjectRefreshException : Exception
    {
        internal CachedObjectRefreshException(string message, Exception ex)
            : base(message, ex)
        { }
    }
}