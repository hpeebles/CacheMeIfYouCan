using System;

namespace CacheMeIfYouCan.Notifications
{
    public sealed class CachedObjectUpdateException : Exception
    {
        private const string ExceptionMessageFormat =
            "{0} threw an exception while trying to update its value";
        
        internal CachedObjectUpdateException(string name, Exception exception)
            : base(String.Format(ExceptionMessageFormat, name), exception)
        {
            Name = name;
        }
        
        public string Name { get; }
    }
}