using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CacheMeIfYouCan.Internal
{
    internal class CachedFunctionConfiguration<TKey, TValue>
    {
        public CachedFunctionConfiguration(Func<TKey, Task<TValue>> originalFunc)
        {
            OriginalFunction = originalFunc;
        }
        
        public Func<TKey, Task<TValue>> OriginalFunction { get; }
        public Func<TKey, TimeSpan> TimeToLiveFactory { get; set; }
        public ILocalCache<TKey, TValue> LocalCache { get; set; }
        public IEqualityComparer<TKey> KeyComparer { get; set; }
    }
}