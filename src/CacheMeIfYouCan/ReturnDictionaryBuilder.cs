using System.Collections.Generic;

namespace CacheMeIfYouCan
{
    public abstract class ReturnDictionaryBuilder<TK, TV, TDictionary> : ReturnDictionaryBuilder<TK, TV, TDictionary, TDictionary>
        where TDictionary : IDictionary<TK, TV>
    {
        protected ReturnDictionaryBuilder(IEqualityComparer<TK> keyComparer)
            : base(keyComparer)
        { }

        protected sealed override TDictionary FinalizeDictionary(TDictionary dictionary)
        {
            return dictionary;
        }
    }
    
    public abstract class ReturnDictionaryBuilder<TK, TV, TDictionaryTemp, TDictionary> : IReturnDictionaryBuilder<TK, TV, TDictionary>
        where TDictionaryTemp : IDictionary<TK, TV>
        where TDictionary : IDictionary<TK, TV>
    {
        private readonly IEqualityComparer<TK> _keyComparer;

        protected ReturnDictionaryBuilder(IEqualityComparer<TK> keyComparer)
        {
            _keyComparer = keyComparer;
        }

        public virtual TDictionary BuildResponse(IEnumerable<IKeyValuePair<TK, TV>> values, int count)
        {
            var dictionary = InitializeDictionary(_keyComparer, count);

            foreach (var (key, value) in values)
                dictionary[key] = value;

            return FinalizeDictionary(dictionary);
        }

        protected abstract TDictionaryTemp InitializeDictionary(IEqualityComparer<TK> keyComparer, int count);

        protected abstract TDictionary FinalizeDictionary(TDictionaryTemp dictionary);
    }
}