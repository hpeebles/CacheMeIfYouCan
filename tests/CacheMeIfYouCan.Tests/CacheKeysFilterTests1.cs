using System.Collections.Generic;
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
        [Theory]
        [MemberData(nameof(EnumGenerator<CollectionType>.Generate), MemberType = typeof(EnumGenerator<CollectionType>))]
        public void AllExcluded_WorkAsExpected(CollectionType type)
        {
            var input = BuildCollection(type);

            var filtered = CacheKeysFilter<int>.Filter(input, _ => true, out var pooledArray);

            filtered.Should().BeEmpty();
            pooledArray.Should().BeNull();
        }

        [Theory]
        [MemberData(nameof(EnumGenerator<CollectionType>.Generate), MemberType = typeof(EnumGenerator<CollectionType>))]
        public void AllIncluded_WorksAsExpected(CollectionType type)
        {
            var input = BuildCollection(type);

            var filtered = CacheKeysFilter<int>.Filter(input, _ => false, out var pooledArray);

            if (type != CollectionType.Other)
            {
                filtered.Should().BeSameAs(input);
                pooledArray.Should().BeNull();
            }
            else
            {
                filtered.Should().BeEquivalentTo(input);
            }
        }

        [Theory]
        [MemberData(nameof(EnumAndBoolGenerator<CollectionType>.Generate), MemberType = typeof(EnumAndBoolGenerator<CollectionType>))]
        public void SomeIncluded_SomeExcluded_WorksAsExpected(CollectionType type, bool firstKeyIsIncluded)
        {
            var start = firstKeyIsIncluded ? 0 : -1;
            var input = BuildCollection(type, start);

            var filtered = CacheKeysFilter<int>.Filter(input, k => k % 2 != 0, out _);

            filtered.Should().BeEquivalentTo(Enumerable.Range(0, 5).Select(i => i * 2));
        }

        [Theory]
        [MemberData(nameof(EnumAndIntGenerator<CollectionType>.Generate), 0, 10, MemberType = typeof(EnumAndIntGenerator<CollectionType>))]
        public void OneExcluded_WorksAsExpected(CollectionType type, int indexOfExcluded)
        {
            var input = BuildCollection(type);

            var filtered = CacheKeysFilter<int>.Filter(input, k => k == indexOfExcluded, out _);

            var expected = Enumerable.Range(0, 10).Where(i => i != indexOfExcluded).ToArray();
            
            filtered.Should().BeEquivalentTo(expected);
        }
        
        [Theory]
        [MemberData(nameof(EnumAndIntGenerator<CollectionType>.Generate), 0, 10, MemberType = typeof(EnumAndIntGenerator<CollectionType>))]
        public void OneIncluded_WorksAsExpected(CollectionType type, int indexOfIncluded)
        {
            var input = BuildCollection(type);

            var filtered = CacheKeysFilter<int>.Filter(input, k => k != indexOfIncluded, out _);

            var expected = new[] { indexOfIncluded };
            
            filtered.Should().BeEquivalentTo(expected);
        }

        private static IReadOnlyCollection<int> BuildCollection(CollectionType type, int start = 0)
        {
            var array = Enumerable.Range(start, 10).ToArray();

            return type switch
            {
                CollectionType.Array => array,
                CollectionType.List => array.ToList(),
                _ => (IReadOnlyCollection<int>)new HashSet<int>(array),
            };
        }
    }
}