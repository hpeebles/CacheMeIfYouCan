using System.Linq;
using CacheMeIfYouCan.Internal;
using FluentAssertions;
using Xunit;

namespace CacheMeIfYouCan.Tests.Misc
{
    public class RandomNumberGeneratorTests
    {
        [Fact]
        public void NumbersAreAllWithinRange()
        {
            var rng = new RandomNumberGenerator(-100, 100);

            var numbers = Enumerable
                .Range(0, 1000)
                .Select(i => rng.GetNext())
                .OrderBy(i => i)
                .ToArray();
            
            numbers.First().Should().BeGreaterThan(-100).And.BeLessThan(-80);

            numbers[500].Should().BeGreaterThan(-30).And.BeLessThan(30);

            numbers.Last().Should().BeGreaterThan(80).And.BeLessThan(100);
        }
    }
}