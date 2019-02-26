using System.Collections.Concurrent;
using System.Collections.Generic;

namespace CacheMeIfYouCan.Internal.ReturnDictionaryBuilders
{
    public class ConcurrentDictionaryBuilder<TK, TV> : ReturnDictionaryBuilder<TK, TV, ConcurrentDictionary<TK, TV>>
    {
        public ConcurrentDictionaryBuilder(IEqualityComparer<TK> keyComparer)
            : base(keyComparer)
        { }

        protected override ConcurrentDictionary<TK, TV> InitializeDictionary(IEqualityComparer<TK> keyComparer, int count)
        {
            return new ConcurrentDictionary<TK, TV>(keyComparer);
        }
    }
}