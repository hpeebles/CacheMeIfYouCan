namespace CacheMeIfYouCan
{
    public abstract class FunctionCacheGetResult
    {
        public readonly string CacheName;
        public readonly string Key;
        public readonly Outcome Outcome;
        public readonly long Duration;
        public readonly string CacheType;

        internal FunctionCacheGetResult(string cacheName, string key, Outcome outcome, long duration, string cacheType)
        {
            CacheName = cacheName;
            Key = key;
            Outcome = outcome;
            Duration = duration;
            CacheType = cacheType;
        }
    }

    public class FunctionCacheGetResult<T> : FunctionCacheGetResult
    {
        public readonly T Value;

        internal FunctionCacheGetResult(string cacheName, string key, T value, Outcome outcome, long duration, string cacheType)
            : base(cacheName, key, outcome, duration, cacheType)
        {
            Value = value;
        }
    }
    
    public enum Outcome
    {
        Error,
        FromCache,
        Fetch
    }
}