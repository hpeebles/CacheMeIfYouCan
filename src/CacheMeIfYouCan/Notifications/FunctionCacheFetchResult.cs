using System;

namespace CacheMeIfYouCan.Notifications
{
    public abstract class FunctionCacheFetchResult
    {
        public readonly FunctionInfo FunctionInfo;
        public readonly Lazy<string> KeyString;
        public readonly bool Success;
        public readonly long Start;
        public readonly long Duration;
        public readonly bool Duplicate;
        public readonly FetchReason Reason;
        public readonly TimeSpan? ExistingTtl;

        protected internal FunctionCacheFetchResult(
            FunctionInfo functionInfo,
            Lazy<string> keyString,
            bool success,
            long start,
            long duration,
            bool duplicate,
            FetchReason reason,
            TimeSpan? existingTtl)
        {
            FunctionInfo = functionInfo;
            KeyString = keyString;
            Success = success;
            Start = start;
            Duration = duration;
            Duplicate = duplicate;
            Reason = reason;
            ExistingTtl = existingTtl;
        }
    }
    
    public class FunctionCacheFetchResult<TK, TV> : FunctionCacheFetchResult
    {
        public readonly TK Key;
        public readonly TV Value;

        internal FunctionCacheFetchResult(
            FunctionInfo functionInfo,
            Key<TK> key,
            TV value,
            bool success,
            long start,
            long duration,
            bool duplicate,
            FetchReason reason,
            TimeSpan? existingTtl)
            : base(functionInfo, key.AsString, success, start, duration, duplicate, reason, existingTtl)
        {
            Key = key;
            Value = value;
        }
    }

    public enum FetchReason
    {
        OnDemand, // Due to requested key not being in cache, will block client
        EarlyFetch, // Due to requested key being in cache but about to expire, will not block client
        KeysToKeepAliveFunc // Due to the KeysToKeepAlive process triggering a fetch
    }
}