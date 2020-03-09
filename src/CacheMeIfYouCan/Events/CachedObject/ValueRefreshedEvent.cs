using System;

namespace CacheMeIfYouCan.Events.CachedObject
{
    public readonly struct ValueRefreshedEvent<T>
    {
        internal ValueRefreshedEvent(
            T newValue,
            T previousValue,
            TimeSpan duration,
            DateTime dateOfPreviousSuccessfulRefresh,
            long version)
        {
            NewValue = newValue;
            PreviousValue = previousValue;
            Duration = duration;
            DateOfPreviousSuccessfulRefresh = dateOfPreviousSuccessfulRefresh;
            Version = version;
        }
        
        public T NewValue { get; }
        public T PreviousValue { get; }
        public TimeSpan Duration { get; }
        public DateTime DateOfPreviousSuccessfulRefresh { get; }
        public long Version { get; }
        public bool IsResultOfInitialization => Version == 1;
    }
}