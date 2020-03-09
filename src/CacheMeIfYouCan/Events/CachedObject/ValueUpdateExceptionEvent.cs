using System;

namespace CacheMeIfYouCan.Events.CachedObject
{
    public readonly struct ValueUpdateExceptionEvent<T, TUpdateFuncInput>
    {
        internal ValueUpdateExceptionEvent(
            Exception exception,
            T currentValue,
            TUpdateFuncInput updateFuncInput,
            TimeSpan duration,
            DateTime dateOfPreviousSuccessfulRefresh,
            long version)
        {
            Exception = exception;
            CurrentValue = currentValue;
            UpdateFuncInput = updateFuncInput;
            Duration = duration;
            DateOfPreviousSuccessfulRefresh = dateOfPreviousSuccessfulRefresh;
            Version = version;
        }

        public Exception Exception { get; }
        public T CurrentValue { get; }
        public TUpdateFuncInput UpdateFuncInput { get; }
        public TimeSpan Duration { get; }
        public DateTime DateOfPreviousSuccessfulRefresh { get; }
        public long Version { get; }
    }
}