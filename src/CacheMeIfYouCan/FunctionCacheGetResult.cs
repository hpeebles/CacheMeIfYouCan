namespace CacheMeIfYouCan
{
    public abstract class FunctionCacheGetResult
    {
        public readonly string CacheName;
        public readonly string KeyString;
        public readonly Outcome Outcome;
        public readonly long Duration;
        public readonly string CacheType;

        protected internal FunctionCacheGetResult(string cacheName, string keyString, Outcome outcome, long duration, string cacheType)
        {
            CacheName = cacheName;
            KeyString = keyString;
            Outcome = outcome;
            Duration = duration;
            CacheType = cacheType;
        }
    }

    public class FunctionCacheGetResult<TK, TV> : FunctionCacheGetResult
    {
        public readonly TK Key;
        public readonly TV Value;

        internal FunctionCacheGetResult(string cacheName, TK key, TV value, string keyString, Outcome outcome, long duration, string cacheType)
            : base(cacheName, keyString, outcome, duration, cacheType)
        {
            Key = key;
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