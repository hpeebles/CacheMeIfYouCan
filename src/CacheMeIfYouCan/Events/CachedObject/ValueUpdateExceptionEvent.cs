using System;

namespace CacheMeIfYouCan.Events.CachedObject
{
    public readonly struct ValueUpdateExceptionEvent<T, TUpdates>
    {
        internal ValueUpdateExceptionEvent(
            Exception exception,
            T currentValue,
            TUpdates updates,
            TimeSpan duration,
            long version)
        {
            Exception = exception;
            CurrentValue = currentValue;
            Updates = updates;
            Duration = duration;
            Version = version;
        }

        public Exception Exception { get; }
        public T CurrentValue { get; }
        public TUpdates Updates { get; }
        public TimeSpan Duration { get; }
        public long Version { get; }
    }
}