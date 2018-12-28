using System;

namespace CacheMeIfYouCan.Tests.Cache.Helpers
{
    public class CrazyException : Exception
    {
        public CrazyException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}