namespace CacheMeIfYouCan
{
    public struct FunctionCacheFetchResult<T>
    {
        public readonly string Key;
        public readonly T Value;
        public readonly bool Success;
        public readonly long Duration;
        
        public FunctionCacheFetchResult(string key, T value, bool success, long duration)
        {
            Key = key;
            Value = value;
            Success = success;
            Duration = duration;
        }
    }
}