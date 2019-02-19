using System;

namespace CacheMeIfYouCan.Notifications
{
    public abstract class CachedObjectRefreshResult
    {
        internal CachedObjectRefreshResult(
            string name,
            DateTime start,
            TimeSpan duration,
            CachedObjectRefreshException exception,
            int refreshAttemptCount,
            int successfulRefreshCount,
            DateTime lastRefreshAttempt,
            DateTime lastSuccessfulRefresh,
            TimeSpan nextRefreshInterval)
        {
            Name = name;
            Start = start;
            Duration = duration;
            Success = exception == null;
            Exception = exception;
            RefreshAttemptCount = refreshAttemptCount;
            SuccessfulRefreshCount = successfulRefreshCount;
            LastRefreshAttempt = lastRefreshAttempt;
            LastSuccessfulRefresh = lastSuccessfulRefresh;
            NextRefreshInterval = nextRefreshInterval;
        }
        
        public string Name { get; }
        public DateTime Start { get; }
        public TimeSpan Duration { get; }
        public bool Success { get; }
        public CachedObjectRefreshException Exception { get; }
        public int RefreshAttemptCount { get; }
        public int SuccessfulRefreshCount { get; }
        public DateTime LastRefreshAttempt { get; }
        public DateTime LastSuccessfulRefresh { get; }
        public TimeSpan NextRefreshInterval { get; }
    }

    public sealed class CachedObjectRefreshResult<T> : CachedObjectRefreshResult
    {
        internal CachedObjectRefreshResult(
            string name,
            DateTime start,
            TimeSpan duration,
            CachedObjectRefreshException exception,
            T newValue,
            int refreshAttemptCount,
            int successfulRefreshCount,
            DateTime lastRefreshAttempt,
            DateTime lastSuccessfulRefresh,
            TimeSpan nextRefreshInterval)
            : base(name, start, duration, exception, refreshAttemptCount, successfulRefreshCount, lastRefreshAttempt, lastSuccessfulRefresh, nextRefreshInterval)
        {
            NewValue = newValue;
        }
        
        public T NewValue { get; }
    }
}