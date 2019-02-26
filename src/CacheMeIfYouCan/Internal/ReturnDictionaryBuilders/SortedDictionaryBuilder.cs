using System.Collections.Generic;

namespace CacheMeIfYouCan.Internal.ReturnDictionaryBuilders
{
    public class SortedDictionaryBuilder<TK, TV> : ReturnDictionaryBuilder<TK, TV, SortedDictionary<TK, TV>>
    {
        public SortedDictionaryBuilder(IEqualityComparer<TK> keyComparer)
            : base(keyComparer)
        { }

        protected override SortedDictionary<TK, TV> InitializeDictionary(IEqualityComparer<TK> keyComparer, int count)
        {
            return new SortedDictionary<TK, TV>();
        }
    }
}