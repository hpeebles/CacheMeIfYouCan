using System;
using System.Collections.Generic;

namespace CacheMeIfYouCan.Events.CachedFunction.EnumerableKeys
{
    public readonly struct ExceptionEvent<TParams, TKey>
    {
        internal ExceptionEvent(
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