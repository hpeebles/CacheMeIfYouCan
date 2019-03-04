using System;
using System.Linq;
using CacheMeIfYouCan.Internal;
using FluentAssertions;
using Xunit;

namespace CacheMeIfYouCan.Tests.Misc
{
    public class StringSeparatorTests
    {
        [Fact]
        public void SingleValue()
        {
            var input = Guid.NewGuid().ToString();
            
            var separator = new StringSeparator(input, "_");

            separator.TryGetNext(out var value).Should().BeTrue();

            value.Should().Be(input);

            separator.TryGetNext(out _).Should().BeFalse();
        }
        
        [Theory]
        [InlineData("_")]
        [InlineData("____")]
        [InlineData("$%^&*")]
        public void TwoValues(string separatorString)
        {
            var input1 = Guid.NewGuid().ToString();
            var input2 = Guid.NewGuid().ToString();
            
            var separator = new StringSeparator(input1 + separatorString + input2, separatorString);

            separator.TryGetNext(out var value).Should().BeTrue();

            value.Should().Be(input1);

            separator.TryGetNext(out value).Should().BeTrue();

            value.Should().Be(input2);

            separator.TryGetNext(out _).Should().BeFalse();
        }
        
        [Theory]
        [InlineData("_")]
        [InlineData("____")]
        [InlineData("$%^&*")]
        public void MultipleValues(string separatorString)
        {
            var input = String.Join(separatorString, Enumerable.Range(0, 10).Select(i => i));
            
            var separator = new StringSeparator(input, separatorString);

            for (var i = 0; i < 10; i++)
            {
                separator.TryGetNext(out var value).Should().BeTrue();

                value.Should().Be(i.ToString());
            }

            separator.TryGetNext(out _).Should().BeFalse();
        }
    }
}