using System;
using System.Collections.Generic;
using System.Linq;
using CacheMeIfYouCan.Internal;
using FluentAssertions;
using Xunit;

namespace CacheMeIfYouCan.Tests
{
    public class MissingKeysResolverTests
    {
        [Fact]
        public void EmptyDictionary_ReturnsInput()
        {
            var inputKeys = Enumerable.Range(0, 10).ToArray();

            var missingKeys = MissingKeysResolver<int, int>.GetMissingKeys(
                inputKeys,
                new Dictionary<int, int>(),
                out var pooledArray);

            missingKeys.Should().BeSameAs(inputKeys);
            pooledArray.Should().BeNull();
        }
        
        [Theory]
        [InlineData(2, 1)]
        [InlineData(10, 1)]
        [InlineData(50, 25)]
        [InlineData(100, 5)]
        [InlineData(100, 99)]
        [InlineData(1000, 500)]
        public void DictionaryIsSubsetOfInputKeys_CorrectlyReturnsOnlyThoseMissing(int inputKeysCount, int keysFoundCount)
        {
            var inputKeys = Enumerable.Range(0, inputKeysCount).ToArray();

            var dictionary = inputKeys
                .OrderBy(_ => Guid.NewGuid())
                .Take(keysFoundCount)
                .ToDictionary(k => k);
            
            var missingKeys = MissingKeysResolver<int, int>.GetMissingKeys(
                inputKeys,
                dictionary,
                out var pooledArray);

            missingKeys.Should().BeEquivalentTo(inputKeys.Except(dictionary.Keys));
            pooledArray.Should().NotBeNull();
        }
        
        [Theory]
        [InlineData(2, 1, 1)]
        [InlineData(10, 5, 1)]
        [InlineData(50, 25, 5)]
        [InlineData(100, 99, 99)]
        [InlineData(100, 110, 20)]
        [InlineData(1000, 500, 100)]
        public void DictionaryContainsKeysNotInInput_CorrectlyReturnsOnlyThoseMissing(
            int inputKeysCount, int keysFoundCount, int keysNotInInputCount)
        {
            var inputKeys = Enumerable.Range(0, inputKeysCount).ToArray();

            var dictionary = inputKeys
                .OrderBy(_ => Guid.NewGuid())
                .Take(keysFoundCount - keysNotInInputCount)
                .Concat(Enumerable.Range(1, keysNotInInputCount).Select(k => -k))
                .ToDictionary(k => k);
            
            var missingKeys = MissingKeysResolver<int, int>.GetMissingKeys(
                inputKeys,
                dictionary,
                out _);

            missingKeys.Should().BeEquivalentTo(inputKeys.Except(dictionary.Keys));
        }

        [Theory]
        [InlineData(1)]
        [InlineData(100)]
        public void DictionaryContainsAllKeysInInput_ReturnsNull(int inputKeysCount)
        {
            var inputKeys = Enumerable.Range(0, inputKeysCount).ToArray();

            var dictionary = inputKeys.ToDictionary(k => k);

            var missingKeys = MissingKeysResolver<int, int>.GetMissingKeys(inputKeys, dictionary, out var pooledArray);

            missingKeys.Should().BeNull();
            pooledArray.Should().BeNull();
        }
    }
}