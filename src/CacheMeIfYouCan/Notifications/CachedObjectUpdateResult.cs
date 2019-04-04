using System;

namespace CacheMeIfYouCan.Notifications
{
    public abstract class CachedObjectUpdateResult
    {
        internal CachedObjectUpdateResult(
            string name,
            DateTime start,
            TimeSpan duration,
            CachedObjectUpdateException exception,
            int updateAttemptCount,
            int successfulUpdateCount,
            DateTime lastUpdateAttempt,
            DateTime lastSuccessfulUpdate)
        {
            Name = name;
            Start = start;
            Duration = duration;
            Success = exception == null;
            Exception = exception;
            UpdateAttemptCount = updateAttemptCount;
            SuccessfulUpdateCount = successfulUpdateCount;
            LastUpdateAttempt = lastUpdateAttempt;
            LastSuccessfulUpdate = lastSuccessfulUpdate;
        }
        
        public string Name { get; }
        public DateTime Start { get; }
        public TimeSpan Duration { get; }
        public bool Success { get; }
        public CachedObjectUpdateException Exception { get; }
        public int UpdateAttemptCount { get; }
        public int SuccessfulUpdateCount { get; }
        public DateTime LastUpdateAttempt { get; }
        public DateTime LastSuccessfulUpdate { get; }
    }

    public sealed class CachedObjectUpdateResult<T, TUpdates> : CachedObjectUpdateResult
    {
        internal CachedObjectUpdateResult(
            string name,
            DateTime start,
            TimeSpan duration,
            T newValue,
            TUpdates updates,
            CachedObjectUpdateException exception,
            int updateAttemptCount,
            int successfulUpdateCount,
            DateTime lastUpdateAttempt,
            DateTime lastSuccessfulUpdate)
            : base(name, start, duration, exception, updateAttemptCount, successfulUpdateCount, lastUpdateAttempt, lastSuccessfulUpdate)
        {
            NewValue = newValue;
            Updates = updates;
        }
        
        public T NewValue { get; }
        public TUpdates Updates { get; }
    }
}