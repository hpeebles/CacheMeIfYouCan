using System;

namespace CacheMeIfYouCan.Notifications
{
    public interface ICachedObjectUpdateAttemptResult
    {
        string Name { get; }
        DateTime Start { get; }
        TimeSpan Duration { get; }
        bool Success { get; }
        Exception Exception { get; }
        int UpdateAttemptCount { get; }
        int SuccessfulUpdateCount { get; }
        DateTime LastUpdateAttempt { get; }
        DateTime LastSuccessfulUpdate { get; }
    }

    public interface ICachedObjectUpdateAttemptResult<out T> : ICachedObjectUpdateAttemptResult
    {
        T NewValue { get; }
    }
    
    public interface ICachedObjectUpdateAttemptResult<out T, out TUpdates> : ICachedObjectUpdateAttemptResult<T>
    {
        TUpdates Updates { get; }
    }
}