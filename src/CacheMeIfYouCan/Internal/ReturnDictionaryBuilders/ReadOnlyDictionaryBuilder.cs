using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace CacheMeIfYouCan.Internal.ReturnDictionaryBuilders
{
    public class ReadOnlyDictionaryBuilder<TK, TV> : ReturnDictionaryBuilder<TK, TV, Dictionary<TK, TV>, ReadOnlyDictionary<TK, TV>>
    {
        public ReadOnlyDictionaryBuilder(IEqualityComparer<TK> keyComparer)
            : base(keyComparer)
        { }

        protected override Dictionary<TK, TV> InitializeDictionary(IEqualityComparer<TK> keyComparer, int count)
        {
            return new Dictionary<TK, TV>(count, keyComparer);
        }

        protected override ReadOnlyDictionary<TK, TV> FinalizeDictionary(Dictionary<TK, TV> dictionary)
        {
            return new ReadOnlyDictionary<TK, TV>(dictionary);
        }
    }
}