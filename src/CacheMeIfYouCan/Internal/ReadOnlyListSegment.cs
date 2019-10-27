using System;
using System.Collections;
using System.Collections.Generic;

namespace CacheMeIfYouCan.Internal
{
    public readonly struct ReadOnlyListSegment<T> : IReadOnlyList<T>
    {
        private readonly IReadOnlyList<T> _list;
        private readonly int _offset;
        private readonly int _count;
        
        public ReadOnlyListSegment(IReadOnlyList<T> list)
        {
            _list = list ?? throw new ArgumentNullException(nameof(list));
            _offset = 0;
            _count = list.Count;
        }
 
        public ReadOnlyListSegment(IReadOnlyList<T> list, int offset, int count)
        {
            if (list == null)
                throw new ArgumentNullException(nameof(list));
            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset));
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));
            if (list.Count - offset < count)
                throw new ArgumentException();
 
            _list = list;
            _offset = offset;
            _count = count;
        }
        
        public T this[int index]
        {
            get
            {
                if (index < 0 || index >= _count)
                    throw new ArgumentOutOfRangeException(nameof(index));
 
                return _list[_offset + index];
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new ReadOnlyListSegmentEnumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count => _count;
        
        private sealed class ReadOnlyListSegmentEnumerator : IEnumerator<T>
        {
            private readonly IReadOnlyList<T> _list;
            private readonly int _start;
            private readonly int _end;
            private int _current;
 
            internal ReadOnlyListSegmentEnumerator(ReadOnlyListSegment<T> listSegment)
            {
                _list = listSegment._list;
                _start = listSegment._offset;
                _end = _start + listSegment._count;
                _current = _start - 1;
            }
 
            public bool MoveNext()
            {
                if (_current < _end)
                {
                    _current++;
                    return (_current < _end);
                }
                return false;
            }
 
            public T Current
            {
                get
                {
                    if (_current < _start) throw new InvalidOperationException();
                    if (_current >= _end) throw new InvalidOperationException();
                    return _list[_current];
                }
            }
 
            object IEnumerator.Current => Current;

            void IEnumerator.Reset()
            {
                _current = _start - 1;
            }
 
            public void Dispose()
            {
            }
        }
    }
}