namespace CacheMeIfYouCan
{
    public struct FunctionCacheGetResult<T>
    {
        public readonly string Key;
        public readonly T Value;
        public readonly Outcome Outcome;
        public readonly long Duration;
        public readonly string CacheType;

        internal FunctionCacheGetResult(string key, T value, Outcome outcome, long duration, string cacheType)
        {
            Key = key;
            Value = value;
            Outcome = outcome;
            Duration = duration;
            CacheType = cacheType;
        }
    }
    
    public enum Outcome
    {
        Error,
        FromCache,
        Fetch
    }
}