using System;
using System.Threading.Tasks;

namespace CacheMeIfYouCan
{
    public interface IDistributedCacheTelemetry
    {
        Task<T> CallAsync<T>(Func<Task<T>> func, string command, string key);
    }
}