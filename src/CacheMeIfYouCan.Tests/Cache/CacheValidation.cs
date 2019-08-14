using System;
using System.Collections.Generic;
using CacheMeIfYouCan.Caches;
using FluentAssertions;
using Xunit;

namespace CacheMeIfYouCan.Tests.Cache
{
    [Collection(TestCollections.Cache)]
    public class CacheValidation
    {
        private readonly CacheSetupLock _setupLock;

        public CacheValidation(CacheSetupLock setupLock)
        {
            _setupLock = setupLock;
        }
        
        [Fact]
        public void RequiresKeySerializerThrowsIfNoKeySerializerSet()
        {
            Func<List<int>,int> func = x => x.Count;
            using (_setupLock.Enter())
            {
                Action shouldThrow = () => func
                    .Cached()
                    .WithMemoryCache()
                    .Build();

                shouldThrow.Should().Throw<Exception>().And.Message.Should().StartWith("No KeySerializer defined.");
            }
        }
        
        [Fact]
        public void RequiresKeyComparerThrowsIfNoKeyComparerSet()
        {
            Func<List<int>,int> func = x => x.Count;
            using (_setupLock.Enter())
            {
                Action shouldThrow = () => func
                    .Cached()
                    .WithDictionaryCache()
                    .Build();

                shouldThrow.Should().Throw<Exception>().And.Message.Should().StartWith("No KeyComparer defined.");
            }
        }
    }
}