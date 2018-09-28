using System.Collections.Generic;

namespace CacheMeIfYouCan.Internal
{
    internal interface IKeySet<TK>
    {
        bool Add(Key<TK> key);

        bool Contains(Key<TK> key);
    }
    
    internal class StringKeySet<TK> : IKeySet<TK>
    {
        private readonly ISet<string> _set = new HashSet<string>();
        
        public bool Add(Key<TK> key)
        {
            return _set.Add(key.AsString.Value);
        }

        public bool Contains(Key<TK> key)
        {
            return _set.Contains(key.AsString.Value);
        }
    }
    
    internal class GenericKeySet<TK> : IKeySet<TK>
    {
        private readonly ISet<TK> _set = new HashSet<TK>();
        
        public bool Add(Key<TK> key)
        {
            return _set.Add(key.AsObject);
        }
        
        public bool Contains(Key<TK> key)
        {
            return _set.Contains(key.AsObject);
        }
    }
}