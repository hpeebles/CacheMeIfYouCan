using System;
using Xunit;

namespace CacheMeIfYouCan.Tests.Keys
{
    public class AsStringSafe
    {
        [Fact]
        public void AsStringSafeDoesntThrow()
        {
            var key = new Key<int>(123, default(Func<int, string>));

            Assert.ThrowsAny<Exception>(() => key.AsString);
            
            Assert.Equal("123", key.AsStringSafe);
        }
    }
}