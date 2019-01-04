namespace CacheMeIfYouCan
{
    /// <summary>
    /// Defines how an incoming item should be added to a list of existing items
    /// </summary>
    public enum AdditionBehaviour
    {
        /// <summary>
        /// Add the new item to the end of the existing list
        /// </summary>
        Append,
        
        /// <summary>
        /// Insert the new item at the beginning of the existing list
        /// </summary>
        Prepend,
        
        /// <summary>
        /// Replace the existing list with a list containing only the new item
        /// </summary>
        Overwrite
    }
}