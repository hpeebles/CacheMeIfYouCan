using System.Collections.Generic;

namespace CacheMeIfYouCan.Notifications
{
    public abstract class FunctionCacheGetResult
    {
        public readonly FunctionInfo FunctionInfo;
        public readonly IEnumerable<IFunctionCacheGetResultInner> Results;
        public readonly bool Success;
        public readonly long Start;
        public readonly long Duration;

        protected internal FunctionCacheGetResult(
            FunctionInfo functionInfo,
            IEnumerable<IFunctionCacheGetResultInner> results,
            bool success,
            long start,
            long duration)
        {
            FunctionInfo = functionInfo;
            Results = results;
            Success = success;
            Start = start;
            Duration = duration;
        }
    }

    public class FunctionCacheGetResult<TK, TV> : FunctionCacheGetResult
    {
        internal FunctionCacheGetResult(
            FunctionInfo functionInfo,
            ICollection<FunctionCacheGetResultInner<TK, TV>> results,
            bool success,
            long start,
            long duration)
            : base(functionInfo, results, success, start, duration)
        { }

        public new ICollection<FunctionCacheGetResultInner<TK, TV>> Results => base.Results as ICollection<FunctionCacheGetResultInner<TK, TV>>;
    }

    public interface IFunctionCacheGetResultInner
    {
        string KeyString { get; }
        Outcome Outcome { get; }
        string CacheType { get; }
    }

    public class FunctionCacheGetResultInner<TK, TV> : IFunctionCacheGetResultInner
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
        
        public string KeyString => Key.AsString.Value;
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