using System;

namespace CacheMeIfYouCan
{
    public readonly struct CachedFunctionWithSingleKeyResult_Exception<TKey>
    {
        internal CachedFunctionWithSingleKeyResult_Exception(
            in CachedFunctionWithSingleKeyResult_MultiParam_Exception<TKey, TKey> result)
        {
            Key = result.Key;
            Start = result.Start;
            Duration = result.Duration;
            Exception = result.Exception;
        }
        
        public TKey Key { get; }
        public DateTime Start { get; }
        public TimeSpan Duration { get; }
        public Exception Exception { get; }
    }
    
    public readonly struct CachedFunctionWithSingleKeyResult_1Param_Exception<TParam, TKey>
    {
        internal CachedFunctionWithSingleKeyResult_1Param_Exception(
            in CachedFunctionWithSingleKeyResult_MultiParam_Exception<TParam, TKey> result)
        {
            Parameter = result.Parameters;
            Key = result.Key;
            Start = result.Start;
            Duration = result.Duration;
            Exception = result.Exception;
        }
        
        public TParam Parameter { get; }
        public TKey Key { get; }
        public DateTime Start { get; }
        public TimeSpan Duration { get; }
        public Exception Exception { get; }
    }

    public readonly struct CachedFunctionWithSingleKeyResult_MultiParam_Exception<TParams, TKey>
    {
        internal CachedFunctionWithSingleKeyResult_MultiParam_Exception(
            TParams parameters,
            TKey key,
            DateTime start,
            TimeSpan duration,
            Exception exception)
        {
            Parameters = parameters;
            Key = key;
            Start = start;
            Duration = duration;
            Exception = exception;
        }

        public TParams Parameters { get; }
        public TKey Key { get; }
        public DateTime Start { get; }
        public TimeSpan Duration { get; }
        public Exception Exception { get; }
    }
}