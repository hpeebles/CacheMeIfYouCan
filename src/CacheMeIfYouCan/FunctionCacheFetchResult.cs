using System;

namespace CacheMeIfYouCan
{
    public abstract class FunctionCacheFetchResult
    {
        public readonly string CacheName;
        public readonly string KeyString;
        public readonly bool Success;
        public readonly long Duration;
        public readonly bool Duplicate;
        public readonly TimeSpan? ExistingTtl;

        protected internal FunctionCacheFetchResult(string cacheName, string keyString, bool success, long duration, bool duplicate, TimeSpan? existingTtl)
        {
            CacheName = cacheName;
            KeyString = keyString;
            Success = success;
            Duration = duration;
            Duplicate = duplicate;
            ExistingTtl = existingTtl;
        }
    }
    
    public class FunctionCacheFetchResult<TK, TV> : FunctionCacheFetchResult
    {
        public readonly TK Key;
        public readonly TV Value;

        internal FunctionCacheFetchResult(string cacheName, TK key, TV value, string keyString, bool success, long duration, bool duplicate, TimeSpan? existingTtl)
            : base(cacheName, keyString, success, duration, duplicate, existingTtl)
        {
            Key = key;
            Value = value;
        }
    }
}