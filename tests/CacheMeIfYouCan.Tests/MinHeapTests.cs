using System;
using System.Collections.Generic;
using System.Linq;
using CacheMeIfYouCan.Internal;
using FluentAssertions;
using Xunit;

namespace CacheMeIfYouCan.Tests
{
    public class MinHeapTests
    {
        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(10000)]
        public void ValuesReturnedInAscendingOrder(int valuesCount)
        {
            var valuesInOrder = Enumerable.Range(0, valuesCount).ToArray();

            var heap = new MinHeap<int>(Comparer<int>.Default);

            foreach (var value in Shuffle(valuesInOrder))
                heap.Add(value);

            for (var i = 0; i < valuesCount; i++)
            {
                heap.TryPeek(out var peekedValue).Should().BeTrue();
                peekedValue.Should().Be(i);

                heap.TryTake(out var takenValue).Should().BeTrue();
                takenValue.Should().Be(i);
            }

            heap.TryPeek(out _).Should().BeFalse();
            heap.TryTake(out _).Should().BeFalse();
        }

        private static int[] Shuffle(int[] input)
        {
            return input.OrderBy(_ => Guid.NewGuid()).ToArray();
        }
    }
}