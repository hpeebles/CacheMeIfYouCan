using System;

namespace CacheMeIfYouCan
{
    public abstract class FunctionCacheFetchResult
    {
        public readonly string CacheName;
        public readonly string Key;
        public readonly bool Success;
        public readonly long Duration;
        public readonly bool Duplicate;
        public readonly TimeSpan? ExistingTtl;

        internal FunctionCacheFetchResult(string cacheName, string key, bool success, long duration, bool duplicate, TimeSpan? existingTtl)
        {
            CacheName = cacheName;
            Key = key;
            Success = success;
            Duration = duration;
            Duplicate = duplicate;
            ExistingTtl = existingTtl;
        }
    }
    
    public class FunctionCacheFetchResult<T> : FunctionCacheFetchResult
    {
        public readonly T Value;

        internal FunctionCacheFetchResult(string cacheName, string key, T value, bool success, long duration, bool duplicate, TimeSpan? existingTtl)
            : base(cacheName, key, success, duration, duplicate, existingTtl)
        {
            Value = value;
        }
    }
}