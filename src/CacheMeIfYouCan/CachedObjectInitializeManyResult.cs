using System.Collections.Generic;
using System.Linq;

namespace CacheMeIfYouCan
{
    /// <summary>
    /// Represents the result of a call to initialize a list of <see cref="ICachedObject{T}"/> instances
    /// </summary>
    public readonly struct CachedObjectInitializeManyResult
    {
        private readonly IList<CachedObjectInitializeResult> _results;
        
        internal CachedObjectInitializeManyResult(IList<CachedObjectInitializeResult> results)
        {
            Success = results.Any() && results.All(r => r.Outcome == CachedObjectInitializeOutcome.Success);
            _results = results;
        }
        
        /// <summary>
        /// True if all instances are now initialized, False if any failed
        /// </summary>
        public bool Success { get; }
        
        /// <summary>
        /// The results of each call to <see cref="ICachedObject{T}.Initialize"/>
        /// </summary>
        public IList<CachedObjectInitializeResult> Results => _results ?? new CachedObjectInitializeResult[0];
    }
}