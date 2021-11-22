using System;
using System.Threading.Tasks;

namespace CacheMeIfYouCan
{
    public interface IRedisTelemetry
    {
        Task<T> CallRedisAsync<T>(Func<Task<T>> func, string command, string key);
    }
}