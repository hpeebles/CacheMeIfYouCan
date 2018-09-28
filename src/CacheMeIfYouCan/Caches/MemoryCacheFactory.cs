using System;
using System.Collections.Generic;
using System.Text;

namespace CacheMeIfYouCan.Caches
{
    public class MemoryCacheFactory : ILocalCacheFactory
    {
        public bool RequiresStringKeys => true;

        public ILocalCache<TK, TV> Build<TK, TV>()
        {
            return new MemoryCache<TK, TV>();
        }
    }
}
