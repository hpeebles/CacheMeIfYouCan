using System.Collections.Generic;

namespace CacheMeIfYouCan.Notifications
{
    public abstract class FunctionCacheFetchResult
    {
        public readonly FunctionInfo FunctionInfo;
        public readonly IEnumerable<IFunctionCacheFetchResultInner> Results;
        public readonly bool Success;
        public readonly long Start;
        public readonly long Duration;
        public readonly FetchReason Reason;

        protected internal FunctionCacheFetchResult(
            FunctionInfo functionInfo,
            IEnumerable<IFunctionCacheFetchResultInner> results,
            bool success,
            long start,
            long duration,
            FetchReason reason)
        {
            FunctionInfo = functionInfo;
            Results = results;
            Success = success;
            Start = start;
            Duration = duration;
            Reason = reason;
        }
    }
    
    public class FunctionCacheFetchResult<TK, TV> : FunctionCacheFetchResult
    {
        internal FunctionCacheFetchResult(
            FunctionInfo functionInfo,
            ICollection<FunctionCacheFetchResultInner<TK, TV>> results,
            bool success,
            long start,
            long duration,
            FetchReason reason)
            : base(functionInfo, results, success, start, duration, reason)
        { }
        
        public new ICollection<FunctionCacheFetchResultInner<TK, TV>> Results => base.Results as ICollection<FunctionCacheFetchResultInner<TK, TV>>;
    }
    
    public interface IFunctionCacheFetchResultInner
    {
        string KeyString { get; }
        bool Success { get; }
        bool Duplicate { get; }
        long Duration { get; }
    }
    
    public class FunctionCacheFetchResultInner<TK, TV> : IFunctionCacheFetchResultInner
    {
        internal FunctionCacheFetchResultInner(Key<TK> key, TV value, bool success, bool duplicate, long duration)
        {
            Key = key;
            Value = value;
            Success = success;
            Duplicate = duplicate;
            Duration = duration;
        }
        
        public Key<TK> Key { get; }
        public TV Value { get; }
        
        public string KeyString => Key.AsString;
        public bool Success { get; }
        public bool Duplicate { get; }
        public long Duration { get; }
    }
    
    public enum FetchReason
    {
        OnDemand, // Due to requested key not being in cache, will block client
        EarlyFetch, // Due to requested key being in cache but about to expire, will not block client
        KeysToKeepAliveFunc // Due to the KeysToKeepAlive process triggering a fetch
    }
}