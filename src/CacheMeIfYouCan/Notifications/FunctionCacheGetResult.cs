using System.Collections.Generic;

namespace CacheMeIfYouCan.Notifications
{
    public abstract class FunctionCacheGetResult
    {
        public readonly string FunctionName;
        public readonly IEnumerable<IFunctionCacheGetResultInner> Results;
        public readonly bool Success;
        public readonly long Start;
        public readonly long Duration;

        internal FunctionCacheGetResult(
            string functionName,
            IEnumerable<IFunctionCacheGetResultInner> results,
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
    }

    public sealed class FunctionCacheGetResult<TK, TV> : FunctionCacheGetResult
    {
        internal FunctionCacheGetResult(
            string functionName,
            IEnumerable<FunctionCacheGetResultInner<TK, TV>> results,
            bool success,
            long start,
            long duration)
            : base(functionName, results, success, start, duration)
        { }

        public new IEnumerable<FunctionCacheGetResultInner<TK, TV>> Results => base.Results as IEnumerable<FunctionCacheGetResultInner<TK, TV>>;
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