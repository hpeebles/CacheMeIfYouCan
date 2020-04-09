using System.Collections.Generic;
using System.Linq;
using CacheMeIfYouCan.Internal;
using FluentAssertions;
using Xunit;

namespace CacheMeIfYouCan.Tests
{
    /// <summary>
    /// Tests for <see cref="CacheKeysFilter{TOuterKey,TInnerKey}"/>
    /// </summary>
    public class CacheKeysFilterTests2
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void AllExcluded_WorkAsExpected(bool isArray)
        {
            var array = Enumerable.Range(0, 10).ToArray();
            var input = isArray ? (IReadOnlyCollection<int>) array : new HashSet<int>(array);

            var filtered = CacheKeysFilter<int, int>.Filter(0, input, (_, __) => true, out var pooledArray);

            filtered.Should().BeEmpty();
            pooledArray.Should().BeNull();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void AllIncluded_WorksAsExpected(bool isArray)
        {
            var array = Enumerable.Range(0, 10).ToArray();
            var input = isArray ? (IReadOnlyCollection<int>) array : new HashSet<int>(array);

            var filtered = CacheKeysFilter<int, int>.Filter(0, input, (_, __) => false, out var pooledArray);

            if (isArray)
            {
                filtered.Should().BeSameAs(array);
                pooledArray.Should().BeNull();
            }
            else
            {
                filtered.Should().BeEquivalentTo(array);
            }
        }

        [Theory]
        [MemberData(nameof(BoolGenerator.GetAllCombinations), 2, MemberType = typeof(BoolGenerator))]
        public void SomeIncluded_SomeExcluded_WorksAsExpected(bool isArray, bool firstKeyIsIncluded)
        {
            var start = firstKeyIsIncluded ? 0 : -1;
            var array = Enumerable.Range(start, 10).ToArray();
            var input = isArray ? (IReadOnlyCollection<int>) array : new HashSet<int>(array);

            var filtered = CacheKeysFilter<int, int>.Filter(0, input, (_, k) => k % 2 != 0, out _);

            filtered.Should().BeEquivalentTo(Enumerable.Range(0, 5).Select(i => i * 2));
        }

        [Theory]
        [MemberData(nameof(BoolAndIntGenerator.Generate), 0, 10, MemberType = typeof(BoolAndIntGenerator))]
        public void OneExcluded_WorksAsExpected(bool isArray, int indexOfExcluded)
        {
            var array = Enumerable.Range(0, 10).ToArray();
            var input = isArray ? (IReadOnlyCollection<int>) array : new HashSet<int>(array);

            var filtered = CacheKeysFilter<int, int>.Filter(0, input, (_, k) => k == indexOfExcluded, out _);

            var expected = Enumerable.Range(0, 10).Where(i => i != indexOfExcluded).ToArray();
            
            filtered.Should().BeEquivalentTo(expected);
        }
        
        [Theory]
        [MemberData(nameof(BoolAndIntGenerator.Generate), 0, 10, MemberType = typeof(BoolAndIntGenerator))]
        public void OneIncluded_WorksAsExpected(bool isArray, int indexOfIncluded)
        {
            var array = Enumerable.Range(0, 10).ToArray();
            var input = isArray ? (IReadOnlyCollection<int>) array : new HashSet<int>(array);

            var filtered = CacheKeysFilter<int, int>.Filter(0, input, (_, k) => k != indexOfIncluded, out _);

            var expected = new[] { indexOfIncluded };
            
            filtered.Should().BeEquivalentTo(expected);
        }
    }
}