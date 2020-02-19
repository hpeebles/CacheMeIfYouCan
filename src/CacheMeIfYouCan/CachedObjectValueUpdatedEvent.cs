using System;

namespace CacheMeIfYouCan
{
    public readonly struct CachedObjectValueUpdatedEvent<T, TUpdateFuncInput>
    {
        internal CachedObjectValueUpdatedEvent(
            T newValue,
            T previousValue,
            TUpdateFuncInput updateFuncInput,
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
        public TUpdateFuncInput UpdateFuncInput { get; }
        public TimeSpan Duration { get; }
        public DateTime DateOfPreviousSuccessfulRefresh { get; }
        public long Version { get; }
    }
}