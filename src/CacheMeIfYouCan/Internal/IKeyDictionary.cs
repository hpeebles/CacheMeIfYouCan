using System;
using System.Collections.Concurrent;

namespace CacheMeIfYouCan.Internal
{
    internal interface IKeyDictionary<TK, TV>
    {
        TV GetOrAdd(Key<TK> key, Func<Key<TK>, TV> func);

        void Remove(Key<TK> key);
    }

    internal class StringKeyDictionary<TK, TV> : IKeyDictionary<TK, TV>
    {
        private readonly ConcurrentDictionary<string, TV> _dictionary = new ConcurrentDictionary<string, TV>();
        
        public TV GetOrAdd(Key<TK> key, Func<Key<TK>, TV> func)
        {
            return _dictionary.GetOrAdd(key.AsString.Value, k => func(key));
        }

        public void Remove(Key<TK> key)
        {
            _dictionary.TryRemove(key.AsString.Value, out _);
        }
    }
    
    internal class GenericKeyDictionary<TK, TV> : IKeyDictionary<TK, TV>
    {
        private readonly ConcurrentDictionary<TK, TV> _activeFetches = new ConcurrentDictionary<TK, TV>();
        
        public TV GetOrAdd(Key<TK> key, Func<Key<TK>, TV> func)
        {
            return _activeFetches.GetOrAdd(key.AsObject, k => func(key));
        }

        public void Remove(Key<TK> key)
        {
            _activeFetches.TryRemove(key.AsObject, out _);
        }
    }
    
    internal class EmptyKeyDictionary<TK, TV> : IKeyDictionary<TK, TV>
    {
        public TV GetOrAdd(Key<TK> key, Func<Key<TK>, TV> func)
        {
            return func(key);
        }

        public void Remove(Key<TK> key)
        { }
    }
}