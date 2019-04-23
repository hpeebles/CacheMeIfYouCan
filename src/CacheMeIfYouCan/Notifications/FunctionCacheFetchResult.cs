using System;
using System.Collections.Generic;

namespace CacheMeIfYouCan.Notifications
{
    public abstract class FunctionCacheFetchResult
    {
        internal FunctionCacheFetchResult(
            string functionName,
            IReadOnlyCollection<IFunctionCacheFetchResultInner> results,
            FunctionCacheException exception,
            DateTime start,
            TimeSpan duration)
        {
            FunctionName = functionName;
            Results = results;
            Exception = exception;
            Start = start;
            Duration = duration;
        }
        
        public string FunctionName { get; }
        public IReadOnlyCollection<IFunctionCacheFetchResultInner> Results { get; }
        public bool Success => Exception == null;
        public FunctionCacheException Exception { get; }
        public DateTime Start { get; }
        public TimeSpan Duration { get; }
    }
    
    public sealed class FunctionCacheFetchResult<TK, TV> : FunctionCacheFetchResult
    {
        internal FunctionCacheFetchResult(
            string functionName,
            IReadOnlyCollection<FunctionCacheFetchResultInner<TK, TV>> results,
            FunctionCacheFetchException<TK> exception,
            DateTime start,
            TimeSpan duration)
            : base(functionName, results, exception, start, duration)
        {
            Results = results;
            Exception = exception;
        }
        
        public new IReadOnlyCollection<FunctionCacheFetchResultInner<TK, TV>> Results { get; }
        public new FunctionCacheFetchException<TK> Exception { get; }
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
}