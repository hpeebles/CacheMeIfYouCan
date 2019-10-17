using System;

namespace CacheMeIfYouCan.Notifications
{
    public abstract class CachedObjectUpdateException : Exception, ICachedObjectUpdateAttemptResult
    {
        private const string ExceptionMessageFormat =
            "{0} threw an exception while trying to update its value";
        
        internal CachedObjectUpdateException(
            string name,
            Exception exception,
            DateTime start,
            TimeSpan duration,
            int updateAttemptCount,
            int successfulUpdateCount,
            DateTime lastUpdateAttempt,
            DateTime lastSuccessfulUpdate)
            : base(String.Format(ExceptionMessageFormat, name), exception)
        {
            Name = name;
            Start = start;
            Duration = duration;
            UpdateAttemptCount = updateAttemptCount;
            SuccessfulUpdateCount = successfulUpdateCount;
            LastUpdateAttempt = lastUpdateAttempt;
            LastSuccessfulUpdate = lastSuccessfulUpdate;
        }
        
        public string Name { get; }
        public DateTime Start { get; }
        public TimeSpan Duration { get; }
        public int UpdateAttemptCount { get; }
        public int SuccessfulUpdateCount { get; }
        public DateTime LastUpdateAttempt { get; }
        public DateTime LastSuccessfulUpdate { get; }
        bool ICachedObjectUpdateAttemptResult.Success => false;
        Exception ICachedObjectUpdateAttemptResult.Exception => this;
    }

    public abstract class CachedObjectUpdateException<T> : CachedObjectUpdateException
    {
        internal CachedObjectUpdateException(
            string name,
            Exception exception,
            DateTime start,
            TimeSpan duration,
            T currentValue,
            int updateAttemptCount,
            int successfulUpdateCount,
            DateTime lastUpdateAttempt,
            DateTime lastSuccessfulUpdate)
            : base(name, exception, start, duration, updateAttemptCount, successfulUpdateCount, lastUpdateAttempt, lastSuccessfulUpdate)
        {
            CurrentValue = currentValue;
        }

        public T CurrentValue { get; }
    }

    public sealed class CachedObjectUpdateException<T, TUpdates> : CachedObjectUpdateException<T>, ICachedObjectUpdateAttemptResult<T, TUpdates>
    {
        internal CachedObjectUpdateException(
            string name,
            Exception exception,
            DateTime start,
            TimeSpan duration,
            T currentValue,
            TUpdates updates,
            int updateAttemptCount,
            int successfulUpdateCount,
            DateTime lastUpdateAttempt,
            DateTime lastSuccessfulUpdate)
            : base(name, exception, start, duration, currentValue, updateAttemptCount, successfulUpdateCount, lastUpdateAttempt, lastSuccessfulUpdate)
        {
            Updates = updates;
        }
        
        public TUpdates Updates { get; }
        T ICachedObjectUpdateAttemptResult<T>.NewValue => CurrentValue;
    }
}