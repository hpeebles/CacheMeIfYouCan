namespace CacheMeIfYouCan
{
    public struct FunctionCacheGetResult<T>
    {
        public readonly string Key;
        public readonly T Value;
        public readonly Outcome Outcome;
        public readonly long Duration;

        public FunctionCacheGetResult(string key, T value, Outcome outcome, long duration)
        {
            Key = key;
            Value = value;
            Outcome = outcome;
            Duration = duration;
        }
    }
    
    public enum Outcome
    {
        FromMemory,
        FromRedis,
        Fetch,
        Error
    }
}