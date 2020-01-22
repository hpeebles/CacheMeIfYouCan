using System.Collections;
using System.Collections.Generic;

namespace CacheMeIfYouCan.Internal
{
    internal sealed class SinglyLinkedList<T> : IEnumerable<T>
    {
        private Node _head;
        private Node _tail;

        public void Append(T value)
        {
            var node = new Node(value);

            if (_tail is null)
                _head = node;
            else
                _tail.Next = node;
            
            _tail = node;
        }

        public IEnumerator<T> GetEnumerator() => new Enumerator(this);
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        
        private class Node
        {
            public Node(T value)
            {
                Value = value;
            }
            
            public readonly T Value;
            public Node Next;
        }

        private struct Enumerator : IEnumerator<T>
        {
            private readonly SinglyLinkedList<T> _list;
            private Node _current;
            
            public Enumerator(SinglyLinkedList<T> list)
            {
                _list = list;
                _current = null;
            }
            
            public bool MoveNext()
            {
                if (_current is null)
                {
                    _current = _list._head;
                    return !(_current is null);
                }

                if (_current.Next is null)
                    return false;

                _current = _current.Next;
                return true;
            }

            public T Current => _current.Value;
            public void Reset() => _current = null;
            public void Dispose() { }
            object IEnumerator.Current => Current;
        }
    }
}