using System.Collections.Generic;

namespace CacheMeIfYouCan.Notifications
{
    public abstract class FunctionCacheGetResult
    {
        internal FunctionCacheGetResult(
            string functionName,
            IReadOnlyCollection<IFunctionCacheGetResultInner> results,
            bool success,
            long start,
            long duration)
        {
            FunctionName = functionName;
            Results = results;
            Success = success;
            Start = start;
            Duration = duration;
        }
        
        public string FunctionName { get; }
        public IReadOnlyCollection<IFunctionCacheGetResultInner> Results { get; }
        public bool Success { get; }
        public long Start { get; }
        public long Duration { get; }
    }

    public sealed class FunctionCacheGetResult<TK, TV> : FunctionCacheGetResult
    {
        internal FunctionCacheGetResult(
            string functionName,
            IReadOnlyCollection<FunctionCacheGetResultInner<TK, TV>> results,
            bool success,
            long start,
            long duration)
            : base(functionName, results, success, start, duration)
        {
            Results = results;
        }

        public new IReadOnlyCollection<FunctionCacheGetResultInner<TK, TV>> Results { get; }
    }

    public interface IFunctionCacheGetResultInner
    {
        string KeyString { get; }
        Outcome Outcome { get; }
        string CacheType { get; }
    }

    public sealed class FunctionCacheGetResultInner<TK, TV> : IFunctionCacheGetResultInner
    {
        internal FunctionCacheGetResultInner(Key<TK> key, TV value, Outcome outcome, string cacheType)
        {
            Key = key;
            Value = value;
            Outcome = outcome;
            CacheType = cacheType;
        }

        public Key<TK> Key { get; }
        public TV Value { get; }
        
        public string KeyString => Key.AsStringSafe;
        public Outcome Outcome { get; }
        public string CacheType { get; }
    }
    
    public enum Outcome
    {
        Error,
        FromCache,
        Fetch
    }
}