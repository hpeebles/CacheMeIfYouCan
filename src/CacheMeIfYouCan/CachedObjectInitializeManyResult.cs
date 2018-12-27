using System.Collections.Generic;
using System.Linq;

namespace CacheMeIfYouCan
{
    public readonly struct CachedObjectInitializeManyResult
    {
        private readonly IList<CachedObjectInitializeResult> _results;
        
        internal CachedObjectInitializeManyResult(IList<CachedObjectInitializeResult> results)
        {
            Success = results.Any() && results.All(r => r.Outcome == CachedObjectInitializeOutcome.Success);
            _results = results;
        }
        
        public bool Success { get; }
        public IList<CachedObjectInitializeResult> Results => _results ?? new CachedObjectInitializeResult[0];
    }
}