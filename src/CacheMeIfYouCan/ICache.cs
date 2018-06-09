using System;
using System.Threading.Tasks;

namespace CacheMeIfYouCan
{
    public interface ICache<T>
    {
        Task<GetFromCacheResult<T>> Get(string key);

        Task Set(string key, T value, TimeSpan timeToLive);
    }

    public struct GetFromCacheResult<T>
    {
        public bool Success;
        public T Value;
        public TimeSpan TimeToLive;
        public string CacheType;

        public GetFromCacheResult(T value, TimeSpan timeToLive, string cacheType)
        {
            Success = true;
            Value = value;
            TimeToLive = timeToLive;
            CacheType = cacheType;
        }

        public static GetFromCacheResult<T> NotFound => new GetFromCacheResult<T>();
    }
}