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
        public IReadOnlyCollection<IFunctionCacheFetchResultInner> Results { get; }
        public bool Success { get; }
        public DateTime Start { get; }
        public TimeSpan Duration { get; }
    }
    
    public sealed class FunctionCacheFetchResult<TK, TV> : FunctionCacheFetchResult
    {
        internal FunctionCacheFetchResult(
            string functionName,
            IReadOnlyCollection<FunctionCacheFetchResultInner<TK, TV>> results,
            bool success,
            DateTime start,
            TimeSpan duration)
            : base(functionName, results, success, start, duration)
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
}