using System.Collections.Generic;

namespace CacheMeIfYouCan.Internal
{
    internal sealed class MinHeap<T>
    {
        private readonly SegmentedList<T> _list = new SegmentedList<T>();
        private readonly IComparer<T> _comparer;
        
        public MinHeap(IComparer<T> comparer)
        {
            _comparer = comparer;
        }

        public void Add(T value)
        {
            _list.Add(value);
            
            BubbleUp(_list.Count - 1);
        }

        public bool TryPeek(out T value)
        {
            if (_list.Count == 0)
            {
                value = default;
                return false;
            }

            value = _list[0];
            return true;
        }

        public bool TryTake(out T value)
        {
            if (_list.Count == 0)
            {
                value = default;
                return false;
            }

            value = _list[0];
            _list[0] = _list[_list.Count - 1];
            _list.TrimEnd();
            
            BubbleDown(0);

            return true;
        }

        private void BubbleUp(int childIndex)
        {
            while (childIndex > 0)
            {
                var parentIndex = GetParent(childIndex);
                
                var child = _list[childIndex];
                var parent = _list[parentIndex];

                if (_comparer.Compare(child, parent) >= 0)
                    return;

                _list[childIndex] = parent;
                _list[parentIndex] = child;

                childIndex = parentIndex;
            }
        }

        private void BubbleDown(int parentIndex)
        {
            var count = _list.Count;
            
            while (true)
            {
                var (leftChildIndex, rightChildIndex) = GetChildren(parentIndex);

                T bestChild;
                int bestChildIndex;
                if (rightChildIndex < count)
                {
                    var leftChild = _list[leftChildIndex];
                    var rightChild = _list[rightChildIndex];

                    if (_comparer.Compare(leftChild, rightChild) > 0)
                    {
                        // right child wins
                        bestChild = rightChild;
                        bestChildIndex = rightChildIndex;
                    }
                    else
                    {
                        // left child wins
                        bestChild = leftChild;
                        bestChildIndex = leftChildIndex;
                    }
                }
                else if (leftChildIndex < count)
                {
                    var leftChild = _list[leftChildIndex];
                    bestChild = leftChild;
                    bestChildIndex = leftChildIndex;
                }
                else
                {
                    return;
                }

                var parent = _list[parentIndex];
                if (_comparer.Compare(parent, bestChild) <= 0)
                    return;

                _list[parentIndex] = bestChild;
                _list[bestChildIndex] = parent;
                parentIndex = bestChildIndex;
            }
        }

        private static int GetParent(int index)
        {
            return (index - 1) >> 1;
        }

        private static (int LeftChild, int RightChild) GetChildren(int parent)
        {
            var leftChild = (parent << 1) + 1;
            var rightChild = leftChild + 1;

            return (leftChild, rightChild);
        }
    }
}