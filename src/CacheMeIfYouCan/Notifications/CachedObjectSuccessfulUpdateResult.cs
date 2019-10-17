using System;

namespace CacheMeIfYouCan.Notifications
{
    public abstract class CachedObjectSuccessfulUpdateResult : ICachedObjectUpdateAttemptResult
    {
        internal CachedObjectSuccessfulUpdateResult(
            string name,
            DateTime start,
            TimeSpan duration,
            int updateAttemptCount,
            int successfulUpdateCount,
            DateTime lastUpdateAttempt,
            DateTime lastSuccessfulUpdate)
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
        bool ICachedObjectUpdateAttemptResult.Success => true;
        Exception ICachedObjectUpdateAttemptResult.Exception => null;
    }

    public abstract class CachedObjectSuccessfulUpdateResult<T> : CachedObjectSuccessfulUpdateResult, ICachedObjectUpdateAttemptResult<T>
    {
        internal CachedObjectSuccessfulUpdateResult(
            string name,
            DateTime start,
            TimeSpan duration,
            T newValue,
            int updateAttemptCount,
            int successfulUpdateCount,
            DateTime lastUpdateAttempt,
            DateTime lastSuccessfulUpdate)
            : base(name, start, duration, updateAttemptCount, successfulUpdateCount, lastUpdateAttempt, lastSuccessfulUpdate)
        {
            NewValue = newValue;
        }

        public T NewValue { get; }
    }

    public sealed class CachedObjectSuccessfulUpdateResult<T, TUpdates> : CachedObjectSuccessfulUpdateResult<T>, ICachedObjectUpdateAttemptResult<T, TUpdates>
    {
        internal CachedObjectSuccessfulUpdateResult(
            string name,
            DateTime start,
            TimeSpan duration,
            T newValue,
            TUpdates updates,
            int updateAttemptCount,
            int successfulUpdateCount,
            DateTime lastUpdateAttempt,
            DateTime lastSuccessfulUpdate)
            : base(name, start, duration, newValue, updateAttemptCount, successfulUpdateCount, lastUpdateAttempt, lastSuccessfulUpdate)
        {
            Updates = updates;
        }
        
        public TUpdates Updates { get; }
    }
}