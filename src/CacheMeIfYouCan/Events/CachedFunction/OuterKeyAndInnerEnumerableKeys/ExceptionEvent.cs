using System;
using System.Collections.Generic;

namespace CacheMeIfYouCan.Events.CachedFunction.OuterKeyAndInnerEnumerableKeys
{
    public readonly struct ExceptionEvent<TParams, TOuterKey, TInnerKey>
    {
        internal ExceptionEvent(
            TParams parameters,
            TOuterKey outerKey,
            IReadOnlyCollection<TInnerKey> innerKeys,
            DateTime start,
            TimeSpan duration,
            Exception exception)
        {
            Parameters = parameters;
            OuterKey = outerKey;
            InnerKeys = innerKeys;
            Start = start;
            Duration = duration;
            Exception = exception;
        }
        
        public TParams Parameters { get; }
        public TOuterKey OuterKey { get; }
        public IReadOnlyCollection<TInnerKey> InnerKeys { get; }
        public DateTime Start { get; }
        public TimeSpan Duration { get; }
        public Exception Exception { get; }
    }
}