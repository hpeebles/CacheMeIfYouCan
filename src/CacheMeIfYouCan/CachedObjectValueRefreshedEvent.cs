using System;

namespace CacheMeIfYouCan
{
    public readonly struct CachedObjectValueRefreshedEvent<T>
    {
        internal CachedObjectValueRefreshedEvent(
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
    }
}