using System.Collections.Generic;
using CacheMeIfYouCan.Notifications;

namespace CacheMeIfYouCan.Internal
{
    internal interface IFunctionCacheResultsContainer<TK, TV>
    {
        FunctionCacheGetResultInner<TK, TV> this[Key<TK> key] { set; }
        IEnumerable<Key<TK>> Keys { get; }
        IEnumerable<FunctionCacheGetResultInner<TK, TV>> Values { get; }
    }

    internal struct SingleKeyFunctionCacheResultsContainer<TK, TV> : IFunctionCacheResultsContainer<TK, TV>
    {
        private Key<TK> _key;
        private FunctionCacheGetResultInner<TK, TV> _value;
        
        public FunctionCacheGetResultInner<TK, TV> this[Key<TK> key]
        {
            set
            {
                _key = key;
                _value = value;
            }
        }

        public IEnumerable<Key<TK>> Keys
        {
            get { yield return _key; }
        }
        
        public IEnumerable<FunctionCacheGetResultInner<TK, TV>> Values
        {
            get { yield return _value; }
        }
    }
    
    internal readonly struct MultiKeyFunctionCacheResultsContainer<TK, TV> : IFunctionCacheResultsContainer<TK, TV>
    {
        private readonly IDictionary<Key<TK>, FunctionCacheGetResultInner<TK, TV>> _results;
        
        public MultiKeyFunctionCacheResultsContainer(int keyCount, IEqualityComparer<Key<TK>> keyComparer)
        {
            _results = new Dictionary<Key<TK>, FunctionCacheGetResultInner<TK, TV>>(keyCount, keyComparer);
        }
        
        public FunctionCacheGetResultInner<TK, TV> this[Key<TK> key]
        {
            set => _results[key] = value;
        }

        public IEnumerable<Key<TK>> Keys => _results.Keys;
        public IEnumerable<FunctionCacheGetResultInner<TK, TV>> Values => _results.Values;
    }
}