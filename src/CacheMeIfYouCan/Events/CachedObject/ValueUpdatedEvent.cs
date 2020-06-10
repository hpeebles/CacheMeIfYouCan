using System;

namespace CacheMeIfYouCan.Events.CachedObject
{
    public readonly struct ValueUpdatedEvent<T, TUpdates>
    {
        internal ValueUpdatedEvent(
            T newValue,
            T previousValue,
            TUpdates updates,
            TimeSpan duration,
            long version)
        {
            NewValue = newValue;
            PreviousValue = previousValue;
            Updates = updates;
            Duration = duration;
            Version = version;
        }
        
        public T NewValue { get; }
        public T PreviousValue { get; }
        public TUpdates Updates { get; }
        public TimeSpan Duration { get; }
        public long Version { get; }
    }
}