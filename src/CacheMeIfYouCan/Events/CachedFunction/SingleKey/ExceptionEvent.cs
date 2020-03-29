using System;

namespace CacheMeIfYouCan.Events.CachedFunction.SingleKey
{
    public readonly struct ExceptionEvent<TParams, TKey>
    {
        internal ExceptionEvent(TParams parameters, TKey key, DateTime start, TimeSpan duration, Exception exception)
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