using System;

namespace CacheMeIfYouCan
{
    /// <summary>
    /// Represents the result of an attempt to initialize an <see cref="ICachedObject{T}"/> instance
    /// </summary>
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
        
        /// <summary>
        /// The type, T, of the object cached by the <see cref="ICachedObject{T}"/>
        /// </summary>
        public Type CachedObjectType { get; }
        
        /// <summary>
        /// The outcome of the call to <see cref="ICachedObject{T}.Initialize"/>
        /// </summary>
        public CachedObjectInitializeOutcome Outcome { get; }
        
        /// <summary>
        /// The duration of the call to <see cref="ICachedObject{T}.Initialize"/>
        /// </summary>
        public TimeSpan Duration { get; }
    }
}