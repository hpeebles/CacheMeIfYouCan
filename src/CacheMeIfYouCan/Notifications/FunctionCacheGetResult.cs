using System;

namespace CacheMeIfYouCan.Notifications
{
    public abstract class FunctionCacheGetResult
    {
        public readonly FunctionInfo FunctionInfo;
        public readonly Lazy<string> KeyString;
        public readonly Outcome Outcome;
        public readonly long Start;
        public readonly long Duration;
        public readonly string CacheType;

        protected internal FunctionCacheGetResult(
            FunctionInfo functionInfo,
            Lazy<string> keyString,
            Outcome outcome,
            long start,
            long duration,
            string cacheType)
        {
            FunctionInfo = functionInfo;
            KeyString = keyString;
            Outcome = outcome;
            Start = start;
            Duration = duration;
            CacheType = cacheType;
        }
    }

    public class FunctionCacheGetResult<TK, TV> : FunctionCacheGetResult
    {
        public readonly TK Key;
        public readonly TV Value;

        internal FunctionCacheGetResult(
            FunctionInfo functionInfo,
            Key<TK> key,
            TV value,
            Outcome outcome,
            long start,
            long duration,
            string cacheType)
            : base(functionInfo, key.AsString, outcome, start, duration, cacheType)
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