using System;

namespace CacheMeIfYouCan
{
    public readonly struct CachedFunctionWithSingleKeyResult_Success<TKey, TValue>
    {
        internal CachedFunctionWithSingleKeyResult_Success(
            in CachedFunctionWithSingleKeyResult_MultiParam_Success<TKey, TKey, TValue> result)
        {
            Key = result.Key;
            Value = result.Value;
            Start = result.Start;
            Duration = result.Duration;
            WasCached = result.WasCached;
        }
        
        public TKey Key { get; }
        public TValue Value { get; }
        public DateTime Start { get; }
        public TimeSpan Duration { get; }
        public bool WasCached { get; }
    }
    
    public readonly struct CachedFunctionWithSingleKeyResult_1Param_Success<TParam, TKey, TValue>
    {
        internal CachedFunctionWithSingleKeyResult_1Param_Success(
            in CachedFunctionWithSingleKeyResult_MultiParam_Success<TParam, TKey, TValue> result)
        {
            Parameter = result.Parameters;
            Key = result.Key;
            Value = result.Value;
            Start = result.Start;
            Duration = result.Duration;
            WasCached = result.WasCached;
        }
        
        public TParam Parameter { get; }
        public TKey Key { get; }
        public TValue Value { get; }
        public DateTime Start { get; }
        public TimeSpan Duration { get; }
        public bool WasCached { get; }
    }

    public readonly struct CachedFunctionWithSingleKeyResult_MultiParam_Success<TParams, TKey, TValue>
    {
        internal CachedFunctionWithSingleKeyResult_MultiParam_Success(
            TParams parameters,
            TKey key,
            TValue value,
            DateTime start,
            TimeSpan duration,
            bool wasCached)
        {
            Parameters = parameters;
            Key = key;
            Value = value;
            Start = start;
            Duration = duration;
            WasCached = wasCached;
        }

        public TParams Parameters { get; }
        public TKey Key { get; }
        public TValue Value { get; }
        public DateTime Start { get; }
        public TimeSpan Duration { get; }
        public bool WasCached { get; }
    }
}