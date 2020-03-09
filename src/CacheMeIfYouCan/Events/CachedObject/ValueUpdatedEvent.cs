using System;

namespace CacheMeIfYouCan.Events.CachedObject
{
    public readonly struct ValueUpdatedEvent<T, TUpdateFuncInput>
    {
        internal ValueUpdatedEvent(
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