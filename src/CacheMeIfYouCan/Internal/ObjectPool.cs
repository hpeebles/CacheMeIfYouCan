using System;
using System.Collections.Generic;
using System.Threading;

namespace CacheMeIfYouCan.Internal
{
    internal sealed class ObjectPool<T> where T : class
    {
        private readonly Func<T> _factory;
        private readonly T[] _items;
        private T _firstItem;

        public ObjectPool(Func<T> factory, int capacity)
        {
            if (capacity <= 0)
                throw new ArgumentOutOfRangeException(nameof(capacity));
            
            _factory = factory;
            _items = new T[capacity - 1];
        }
        
        public T Rent()
        {
            var item = _firstItem;
            if (item != null && Interlocked.CompareExchange(ref _firstItem, null, item) == item)
                return item;
            
            var items = _items;
            for (var i = 0; i < items.Length; i++)
            {
                item = items[i];
                if (item != null && Interlocked.CompareExchange(ref items[i], null, item) == item)
                    return item;
            }

            item = _factory();

            return item;
        }

        public void Return(T item)
        {
            if (_firstItem == null && Interlocked.CompareExchange(ref _firstItem, item, null) == null)
                return;
            
            var items = _items;
            for (var i = 0; i < items.Length; i++)
            {
                if (Interlocked.CompareExchange(ref items[i], item, null) == null)
                    return;
            }
        }

        // Only use this for tests!
        public List<T> PeekAll()
        {
            var list = new List<T> { _firstItem };
            list.AddRange(_items);
            list.RemoveAll(x => x is null);
            return list;
        }
    }
}