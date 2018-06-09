using System;

namespace CacheMeIfYouCan
{
    public interface ILogger
    {
        void Error(Exception ex, string message);
    }
}