using System.Collections.Generic;

namespace CacheMeIfYouCan.Internal
{
    internal sealed class SegmentedList<T>
    {
        private const int SegmentSizeBits = 8;
        private const int SegmentSize = 1 << SegmentSizeBits;
        private readonly List<T[]> _segments = new List<T[]>();
        private int _count;
        private int _capacity;
        
        public void Add(T item)
        {
            if (_count == _capacity)
                AddSegment();

            this[_count++] = item;
        }

        public void TrimEnd()
        {
            if (_count == 0)
                return;

            this[--_count] = default;
        }

        public int Count => _count;

        public T this[int index]
        {
            get
            {
                var (segmentIndex, indexWithinSegment) = (Index)index;
                return _segments[segmentIndex][indexWithinSegment];
            }
            set
            {
                var (segmentIndex, indexWithinSegment) = (Index)index;
                _segments[segmentIndex][indexWithinSegment] = value;
            }
        }
        
        private void AddSegment()
        {
            _segments.Add(new T[SegmentSize]);
            _capacity += SegmentSize;
        }

        private readonly struct Index
        {
            private const int IndexWithinSegmentMask = (1 << SegmentSizeBits) - 1;
            
            public Index(int segmentIndex, int indexWithinSegment)
            {
                SegmentIndex = segmentIndex;
                IndexWithinSegment = indexWithinSegment;
            }
            
            public int SegmentIndex { get; }
            public int IndexWithinSegment { get; }

            public static implicit operator int(Index index)
            {
                var (segmentIndex, indexWithinSegment) = index;
                return (segmentIndex << SegmentSizeBits) + indexWithinSegment;
            }

            public static implicit operator Index(int value)
            {
                var segmentIndex = value >> SegmentSizeBits;
                var indexWithinSegment = value & IndexWithinSegmentMask;
                
                return new Index(segmentIndex, indexWithinSegment);
            }
            
            public bool Equals(Index other)
            {
                return SegmentIndex == other.SegmentIndex && IndexWithinSegment == other.IndexWithinSegment;
            }

            public override bool Equals(object obj)
            {
                return obj is Index other && Equals(other);
            }

            public override int GetHashCode() => this;

            public void Deconstruct(out int segmentIndex, out int indexWithinSegment)
            {
                segmentIndex = SegmentIndex;
                indexWithinSegment = IndexWithinSegment;
            }
        }
    }
}