using System;
using System.Collections.Generic;

namespace CacheMeIfYouCan.Notifications
{
    public abstract class FunctionCacheFetchResult
    {
        internal FunctionCacheFetchResult(
            string functionName,
            IReadOnlyCollection<IFunctionCacheFetchResultInner> results,
            bool success,
            long start,
            TimeSpan duration,
            FetchReason reason)
        {
            FunctionName = functionName;
            Results = results;
            Success = success;
            Start = start;
            Duration = duration;
            Reason = reason;
        }
        
        public string FunctionName { get; }
        public IReadOnlyCollection<IFunctionCacheFetchResultInner> Results { get; }
        public bool Success { get; }
        public long Start { get; }
        public TimeSpan Duration { get; }
        public FetchReason Reason { get; }
    }
    
    public sealed class FunctionCacheFetchResult<TK, TV> : FunctionCacheFetchResult
    {
        internal FunctionCacheFetchResult(
            string functionName,
            IReadOnlyCollection<FunctionCacheFetchResultInner<TK, TV>> results,
            bool success,
            long start,
            TimeSpan duration,
            FetchReason reason)
            : base(functionName, results, success, start, duration, reason)
        {
            Results = results;
        }
        
        public new IReadOnlyCollection<FunctionCacheFetchResultInner<TK, TV>> Results { get; }
    }
    
    public interface IFunctionCacheFetchResultInner
    {
        string KeyString { get; }
        bool Success { get; }
        bool Duplicate { get; }
        TimeSpan Duration { get; }
    }
    
    public sealed class FunctionCacheFetchResultInner<TK, TV> : IFunctionCacheFetchResultInner
    {
        internal FunctionCacheFetchResultInner(Key<TK> key, TV value, bool success, bool duplicate, TimeSpan duration)
        {
            Key = key;
            Value = value;
            Success = success;
            Duplicate = duplicate;
            Duration = duration;
        }
        
        public Key<TK> Key { get; }
        public TV Value { get; }
        
        public string KeyString => Key.AsStringSafe;
        public bool Success { get; }
        public bool Duplicate { get; }
        public TimeSpan Duration { get; }
    }
    
    public enum FetchReason
    {
        OnDemand, // Due to requested key not being in cache, will block client
        EarlyFetch, // Due to requested key being in cache but about to expire, will not block client
        KeysToKeepAliveFunc // Due to the KeysToKeepAlive process triggering a fetch
    }
}