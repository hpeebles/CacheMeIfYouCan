using System;

namespace CacheMeIfYouCan
{
    public abstract class FunctionCacheFetchResult
    {
        public readonly FunctionInfo FunctionInfo;
        public readonly string KeyString;
        public readonly bool Success;
        public readonly long Start;
        public readonly long Duration;
        public readonly bool Duplicate;
        public readonly TimeSpan? ExistingTtl;

        protected internal FunctionCacheFetchResult(
            FunctionInfo functionInfo,
            string keyString,
            bool success,
            long start,
            long duration,
            bool duplicate,
            TimeSpan? existingTtl)
        {
            FunctionInfo = functionInfo;
            KeyString = keyString;
            Success = success;
            Start = start;
            Duration = duration;
            Duplicate = duplicate;
            ExistingTtl = existingTtl;
        }
    }
    
    public class FunctionCacheFetchResult<TK, TV> : FunctionCacheFetchResult
    {
        public readonly TK Key;
        public readonly TV Value;

        internal FunctionCacheFetchResult(
            FunctionInfo functionInfo,
            TK key,
            TV value,
            string keyString,
            bool success,
            long start,
            long duration,
            bool duplicate,
            TimeSpan? existingTtl)
            : base(functionInfo, keyString, success, start, duration, duplicate, existingTtl)
        {
            Key = key;
            Value = value;
        }
    }
}