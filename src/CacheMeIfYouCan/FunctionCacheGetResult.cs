﻿namespace CacheMeIfYouCan
{
    public abstract class FunctionCacheGetResult
    {
        public readonly FunctionInfo FunctionInfo;
        public readonly string KeyString;
        public readonly Outcome Outcome;
        public readonly long Start;
        public readonly long Duration;
        public readonly string CacheType;

        protected internal FunctionCacheGetResult(
            FunctionInfo functionInfo,
            string keyString,
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
            TK key,
            TV value,
            string keyString,
            Outcome outcome,
            long start,
            long duration,
            string cacheType)
            : base(functionInfo, keyString, outcome, start, duration, cacheType)
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