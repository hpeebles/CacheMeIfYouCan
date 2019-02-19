using System;

namespace CacheMeIfYouCan.Notifications
{
    public sealed class CachedObjectRefreshException : Exception
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
}