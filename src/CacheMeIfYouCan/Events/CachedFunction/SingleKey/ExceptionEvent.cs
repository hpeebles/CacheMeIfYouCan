using System;

namespace CacheMeIfYouCan.Events.CachedFunction.SingleKey
{
    public readonly struct ExceptionEvent<TKey>
    {
        internal ExceptionEvent(in ExceptionEvent_MultiParam<TKey, TKey> result)
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
    
    public readonly struct ExceptionEvent_1Param<TParam, TKey>
    {
        internal ExceptionEvent_1Param(in ExceptionEvent_MultiParam<TParam, TKey> result)
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

    public readonly struct ExceptionEvent_MultiParam<TParams, TKey>
    {
        internal ExceptionEvent_MultiParam(
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