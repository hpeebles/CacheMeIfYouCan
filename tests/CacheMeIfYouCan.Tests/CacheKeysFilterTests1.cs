using System.Linq;
using CacheMeIfYouCan.Internal;
using CacheMeIfYouCan.Tests.Generators;
using FluentAssertions;
using Xunit;

namespace CacheMeIfYouCan.Tests
{
    /// <summary>
    /// Tests for <see cref="CacheKeysFilter{TKey}"/>
    /// </summary>
    public class CacheKeysFilterTests1
    {
        [Fact]
        public void AllExcluded_WorkAsExpected()
        {
            var input = Enumerable.Range(0, 10).ToArray();

            var filtered = CacheKeysFilter<int>.Filter(input, _ => true, out var pooledArray);

            filtered.IsEmpty.Should().BeTrue();
            pooledArray.Should().BeNull();
        }

        [Fact]
        public void AllIncluded_ReturnsEmpty()
        {
            var input = Enumerable.Range(0, 10).ToArray();

            var filtered = CacheKeysFilter<int>.Filter(input, _ => false, out var pooledArray);

            filtered.ToArray().Should().BeEquivalentTo(input);
            pooledArray.Should().BeNull();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void SomeIncluded_SomeExcluded_WorksAsExpected(bool firstKeyIsIncluded)
        {
            var start = firstKeyIsIncluded ? 0 : -1;
            var input = Enumerable.Range(start, 10).ToArray();

            var filtered = CacheKeysFilter<int>.Filter(input, k => k % 2 != 0, out _);

            filtered.ToArray().Should().BeEquivalentTo(Enumerable.Range(0, 5).Select(i => i * 2));
        }

        [Theory]
        [MemberData(nameof(IntGenerator<CollectionType>.Generate), 0, 10, MemberType = typeof(IntGenerator<CollectionType>))]
        public void OneExcluded_WorksAsExpected(int indexOfExcluded)
        {
            var input = Enumerable.Range(0, 10).ToArray();

            var filtered = CacheKeysFilter<int>.Filter(input, k => k == indexOfExcluded, out _);

            var expected = Enumerable.Range(0, 10).Where(i => i != indexOfExcluded).ToArray();
            
            filtered.ToArray().Should().BeEquivalentTo(expected);
        }
        
        [Theory]
        [MemberData(nameof(IntGenerator<CollectionType>.Generate), 0, 10, MemberType = typeof(IntGenerator<CollectionType>))]
        public void OneIncluded_WorksAsExpected(int indexOfIncluded)
        {
            var input = Enumerable.Range(0, 10).ToArray();

            var filtered = CacheKeysFilter<int>.Filter(input, k => k != indexOfIncluded, out _);

            var expected = new[] { indexOfIncluded };
            
            filtered.ToArray().Should().BeEquivalentTo(expected);
        }
    }
}