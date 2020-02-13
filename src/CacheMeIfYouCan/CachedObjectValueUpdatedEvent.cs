using System;

namespace CacheMeIfYouCan
{
    public readonly struct CachedObjectValueUpdatedEvent<T, TInput>
    {
        internal CachedObjectValueUpdatedEvent(
            T newValue,
            T previousValue,
            TInput updateFuncInput,
            TimeSpan duration,
            DateTime dateOfPreviousSuccessfulRefresh,
            long version)
        {
            NewValue = newValue;
            PreviousValue = previousValue;
            UpdateFuncInput = updateFuncInput;
            Duration = duration;
            DateOfPreviousSuccessfulRefresh = dateOfPreviousSuccessfulRefresh;
            Version = version;
        }
        
        public T NewValue { get; }
        public T PreviousValue { get; }
        public TInput UpdateFuncInput { get; }
        public TimeSpan Duration { get; }
        public DateTime DateOfPreviousSuccessfulRefresh { get; }
        public long Version { get; }
    }
}