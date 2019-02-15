using System;

namespace CacheMeIfYouCan.Tests.Common
{
    public class TestException : Exception
    {
        public TestException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}