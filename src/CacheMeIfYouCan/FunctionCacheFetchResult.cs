using System;

namespace CacheMeIfYouCan
{
    public struct FunctionCacheFetchResult<T>
    {
        public readonly string Key;
        public readonly T Value;
        public readonly bool Success;
        public readonly long Duration;
        public readonly bool Duplicate;
        public readonly TimeSpan? ExistingTtl;

        public FunctionCacheFetchResult(string key, T value, bool success, long duration, bool duplicate, TimeSpan? existingTtl)
        {
            Key = key;
            Value = value;
            Success = success;
            Duration = duration;
            Duplicate = duplicate;
            ExistingTtl = existingTtl;
        }
    }
}