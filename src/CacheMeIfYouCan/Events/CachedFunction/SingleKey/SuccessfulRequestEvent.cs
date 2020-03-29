using System;

namespace CacheMeIfYouCan.Events.CachedFunction.SingleKey
{
    public readonly struct SuccessfulRequestEvent<TParams, TKey, TValue>
    {
        internal SuccessfulRequestEvent(
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