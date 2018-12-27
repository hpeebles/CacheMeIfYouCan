using System;

namespace CacheMeIfYouCan
{
    public readonly struct CachedObjectInitializeResult
    {
        internal CachedObjectInitializeResult(
            Type cachedObjectType,
            CachedObjectInitializeOutcome outcome,
            TimeSpan duration)
        {
            CachedObjectType = cachedObjectType;
            Outcome = outcome;
            Duration = duration;
        }
        
        public Type CachedObjectType { get; }
        public CachedObjectInitializeOutcome Outcome { get; }
        public TimeSpan Duration { get; }
    }
}