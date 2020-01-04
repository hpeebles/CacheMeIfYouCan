using System.Collections;
using System.Collections.Generic;

namespace CacheMeIfYouCan.Internal
{
    internal sealed class ReadOnlyCollectionKeyValuePairKeys<TKey, TValue> : IReadOnlyCollection<TKey>
    {
        private readonly IReadOnlyCollection<KeyValuePair<TKey, TValue>> _collection;

        public ReadOnlyCollectionKeyValuePairKeys(IReadOnlyCollection<KeyValuePair<TKey, TValue>> collection)
        {
            _collection = collection;
        }

        public IEnumerator<TKey> GetEnumerator() => new Enumerator(_collection);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public int Count => _collection.Count;

        public struct Enumerator : IEnumerator<TKey>
        {
            private readonly IEnumerator<KeyValuePair<TKey, TValue>> _parentEnumerator;

            public Enumerator(IReadOnlyCollection<KeyValuePair<TKey, TValue>> collection)
            {
                _parentEnumerator = collection.GetEnumerator();
            }

            public bool MoveNext() => _parentEnumerator.MoveNext();

            public void Reset() => _parentEnumerator.Reset();

            public TKey Current => _parentEnumerator.Current.Key;

            object IEnumerator.Current => Current;

            public void Dispose() => _parentEnumerator.Dispose();
        }
    }
}
