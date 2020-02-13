using System;

namespace CacheMeIfYouCan
{
    public readonly struct CachedObjectValueUpdateExceptionEvent<T, TInput>
    {
        internal CachedObjectValueUpdateExceptionEvent(
            Exception exception,
            T currentValue,
            TInput updateFuncInput,
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
        public TInput UpdateFuncInput { get; }
        public TimeSpan Duration { get; }
        public DateTime DateOfPreviousSuccessfulRefresh { get; }
        public long Version { get; }
    }
}