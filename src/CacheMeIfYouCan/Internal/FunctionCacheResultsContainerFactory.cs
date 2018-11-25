using System.Collections.Generic;

namespace CacheMeIfYouCan.Internal
{
    internal static class FunctionCacheResultsContainerFactory
    {
        public static IFunctionCacheResultsContainer<TK, TV> Build<TK, TV>(
            int keyCount,
            IEqualityComparer<Key<TK>> keyComparer)
        {
            return keyCount == 1
                ? (IFunctionCacheResultsContainer<TK, TV>)new SingleKeyFunctionCacheResultsContainer<TK, TV>()
                : new MultiKeyFunctionCacheResultsContainer<TK, TV>(keyCount, keyComparer);
        }
    }
}