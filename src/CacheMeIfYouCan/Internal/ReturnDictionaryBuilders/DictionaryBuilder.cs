using System.Collections.Generic;

namespace CacheMeIfYouCan.Internal.ReturnDictionaryBuilders
{
    public class DictionaryBuilder<TK, TV> : ReturnDictionaryBuilder<TK, TV, Dictionary<TK, TV>>, IReturnDictionaryBuilder<TK, TV, IDictionary<TK, TV>>
    {
        public DictionaryBuilder(IEqualityComparer<TK> keyComparer)
            : base(keyComparer)
        { }

        protected override Dictionary<TK, TV> InitializeDictionary(IEqualityComparer<TK> keyComparer, int count)
        {
            return new Dictionary<TK, TV>(count, keyComparer);
        }

        IDictionary<TK, TV> IReturnDictionaryBuilder<TK, TV, IDictionary<TK, TV>>.BuildResponse(IEnumerable<IKeyValuePair<TK, TV>> values, int count)
        {
            return base.BuildResponse(values, count);
        }
    }
}