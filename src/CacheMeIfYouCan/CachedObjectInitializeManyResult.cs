using System;
using System.Collections.Generic;
using System.Linq;

namespace CacheMeIfYouCan
{
    /// <summary>
    /// Represents the result of a call to initialize a list of <see cref="ICachedObject"/> instances
    /// </summary>
    public readonly struct CachedObjectInitializeManyResult
    {
        private readonly IList<CachedObjectInitializeResult> _results;
        
        internal CachedObjectInitializeManyResult(IList<CachedObjectInitializeResult> results, TimeSpan duration)
        {
            Success = results.All(r => r.Outcome == CachedObjectInitializeOutcome.Success);
            Duration = duration;
            _results = results;
        }
        
        /// <summary>
        /// True if all <see cref="ICachedObject"/> instances are now initialized, False if any failed
        /// </summary>
        public bool Success { get; }
        
        /// <summary>
        /// The time taken to initialize all of the <see cref="ICachedObject"/> instances
        /// </summary>
        public TimeSpan Duration { get; }
        
        /// <summary>
        /// The results of each call to <see cref="ICachedObject.Initialize"/>
        /// </summary>
        public IList<CachedObjectInitializeResult> Results => _results ?? new CachedObjectInitializeResult[0];
    }
}