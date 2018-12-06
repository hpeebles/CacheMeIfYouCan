using System;

namespace CacheMeIfYouCan.Notifications
{
    public abstract class CachedObjectRefreshResult
    {
        public DateTime Start { get; }
        public TimeSpan Duration { get; }
        public bool Success { get; }
        public Exception Exception { get; }
        public int RefreshAttemptCount { get; }
        public int SuccessfulRefreshCount { get; }
        public DateTime LastRefreshAttempt { get; }
        public DateTime LastSuccessfulRefresh { get; }

        private protected CachedObjectRefreshResult(
            DateTime start,
            TimeSpan duration,
            Exception exception,
            int refreshAttemptCount,
            int successfulRefreshCount,
            DateTime lastRefreshAttempt,
            DateTime lastSuccessfulRefresh)
        {
            Start = start;
            Duration = duration;
            Success = exception == null;
            Exception = exception;
            RefreshAttemptCount = refreshAttemptCount;
            SuccessfulRefreshCount = successfulRefreshCount;
            LastRefreshAttempt = lastRefreshAttempt;
            LastSuccessfulRefresh = lastSuccessfulRefresh;
        }
    }

    public sealed class CachedObjectRefreshResult<T> : CachedObjectRefreshResult
    {
        public T NewValue { get; }

        internal CachedObjectRefreshResult(
            DateTime start,
            TimeSpan duration,
            Exception exception,
            T newValue,
            int refreshAttemptCount,
            int successfulRefreshCount,
            DateTime lastRefreshAttempt,
            DateTime lastSuccessfulRefresh)
            : base(start, duration, exception, refreshAttemptCount, successfulRefreshCount, lastRefreshAttempt, lastSuccessfulRefresh)
        {
            NewValue = newValue;
        }
    }
}