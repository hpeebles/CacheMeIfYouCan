using System;
using FluentAssertions;
using Xunit;

namespace CacheMeIfYouCan.Tests.Keys
{
    public class AsStringSafe
    {
        [Fact]
        public void AsStringSafeDoesntThrow()
        {
            var key = new Key<int>(123, default(Func<int, string>));

            Func<string> func = () => key.AsString;
            func.Should().Throw<Exception>();
            
            key.AsStringSafe.Should().Be("123");
        }
    }
}