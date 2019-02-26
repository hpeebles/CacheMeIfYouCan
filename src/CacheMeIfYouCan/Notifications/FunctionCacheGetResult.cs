using System;
using System.Collections.Generic;
using CacheMeIfYouCan.Internal;

namespace CacheMeIfYouCan.Notifications
{
    public abstract class FunctionCacheGetResult
    {
        internal FunctionCacheGetResult(
            string functionName,
            IReadOnlyCollection<IFunctionCacheGetResultInner> results,
            bool success,
            DateTime start,
            TimeSpan duration)
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
        public DateTime Start { get; }
        public TimeSpan Duration { get; }
    }

    public sealed class FunctionCacheGetResult<TK, TV> : FunctionCacheGetResult
    {
        internal FunctionCacheGetResult(
            string functionName,
            IReadOnlyCollection<FunctionCacheGetResultInner<TK, TV>> results,
            bool success,
            DateTime start,
            TimeSpan duration)
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

    public sealed class FunctionCacheGetResultInner<TK, TV> : IFunctionCacheGetResultInner, IKeyValuePair<TK, TV>
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

        TK IKeyValuePair<TK, TV>.Key => Key;
    }
    
    public enum Outcome
    {
        Error,
        FromCache,
        Fetch
    }
}