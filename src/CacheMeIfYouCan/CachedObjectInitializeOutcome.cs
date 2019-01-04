namespace CacheMeIfYouCan
{
    /// <summary>
    /// Represents the outcome of an attempt to initialize an <see cref="ICachedObject{T}"/> instance
    /// </summary>
    public enum CachedObjectInitializeOutcome
    {
        /// <summary>
        /// The initialization attempt succeeded, or the <see cref="ICachedObject{T}"/> was already initialized
        /// </summary>
        Success,
        
        /// <summary>
        /// The initialization attempt failed
        /// </summary>
        Failure,
        
        /// <summary>
        /// The <see cref="ICachedObject{T}"/> instance is disposed and therefore cannot be used
        /// </summary>
        Disposed
    }
}