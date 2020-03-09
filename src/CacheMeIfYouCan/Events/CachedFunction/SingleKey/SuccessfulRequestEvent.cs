using System;

namespace CacheMeIfYouCan.Events.CachedFunction.SingleKey
{
    public readonly struct SuccessfulRequestEvent<TKey, TValue>
    {
        internal SuccessfulRequestEvent(in SuccessfulRequestEvent_MultiParam<TKey, TKey, TValue> result)
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
    
    public readonly struct SuccessfulRequestEvent_1Param<TParam, TKey, TValue>
    {
        internal SuccessfulRequestEvent_1Param(in SuccessfulRequestEvent_MultiParam<TParam, TKey, TValue> result)
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

    public readonly struct SuccessfulRequestEvent_MultiParam<TParams, TKey, TValue>
    {
        internal SuccessfulRequestEvent_MultiParam(
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