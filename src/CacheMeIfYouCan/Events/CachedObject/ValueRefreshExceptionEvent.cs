using System;

namespace CacheMeIfYouCan.Events.CachedObject
{
    public readonly struct ValueRefreshExceptionEvent<T>
    {
        internal ValueRefreshExceptionEvent(
            Exception exception,
            T currentValue,
            TimeSpan duration,
            DateTime dateOfPreviousSuccessfulRefresh,
            long version)
        {
            Exception = exception;
            CurrentValue = currentValue;
            Duration = duration;
            DateOfPreviousSuccessfulRefresh = dateOfPreviousSuccessfulRefresh;
            Version = version;
        }

        public Exception Exception { get; }
        public T CurrentValue { get; }
        public TimeSpan Duration { get; }
        public DateTime DateOfPreviousSuccessfulRefresh { get; }
        public long Version { get; }
    }
}