using System;
using System.Collections.Generic;

namespace CacheMeIfYouCan.Events.CachedFunction.EnumerableKeys
{
    public readonly struct ExceptionEvent<TKey>
    {
        internal ExceptionEvent(in ExceptionEvent_MultiParam<Unit, TKey> result)
        {
            Keys = result.Keys;
            Start = result.Start;
            Duration = result.Duration;
            Exception = result.Exception;
        }
        
        public IReadOnlyCollection<TKey> Keys { get; }
        public DateTime Start { get; }
        public TimeSpan Duration { get; }
        public Exception Exception { get; }
    }
    
    public readonly struct ExceptionEvent_1ExtraParam<TParam, TKey>
    {
        internal ExceptionEvent_1ExtraParam(in ExceptionEvent_MultiParam<TParam, TKey> result)
        {
            Parameter = result.Parameters;
            Keys = result.Keys;
            Start = result.Start;
            Duration = result.Duration;
            Exception = result.Exception;
        }
        
        public TParam Parameter { get; }
        public IReadOnlyCollection<TKey> Keys { get; }
        public DateTime Start { get; }
        public TimeSpan Duration { get; }
        public Exception Exception { get; }
    }

    public readonly struct ExceptionEvent_MultiParam<TParams, TKey>
    {
        internal ExceptionEvent_MultiParam(
            TParams parameters,
            IReadOnlyCollection<TKey> keys,
            DateTime start,
            TimeSpan duration,
            Exception exception)
        {
            Parameters = parameters;
            Keys = keys;
            Start = start;
            Duration = duration;
            Exception = exception;
        }

        public TParams Parameters { get; }
        public IReadOnlyCollection<TKey> Keys { get; }
        public DateTime Start { get; }
        public TimeSpan Duration { get; }
        public Exception Exception { get; }
    }
}