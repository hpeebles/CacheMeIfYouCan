using System;

namespace Samples.AspNetCoreApp
{
    public interface ILogger
    {
        void LogError(Exception ex, string message);
    }
    
    public class Logger : ILogger
    {
        public void LogError(Exception ex, string message)
        {
        }
    }
}