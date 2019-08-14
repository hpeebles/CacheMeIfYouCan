using System;
using CacheMeIfYouCan.Notifications;
using CacheMeIfYouCan.Tests.Common;
using CacheMeIfYouCan.Tests.Helpers;
using FluentAssertions;
using Xunit;

namespace CacheMeIfYouCan.Tests.FunctionCache
{
    [Collection(TestCollections.FunctionCache)]
    public class KeyComparer
    {
        private readonly CacheSetupLock _setupLock;

        public KeyComparer(CacheSetupLock setupLock)
        {
            _setupLock = setupLock;
        }
        
        [Fact]
        public void WithKeyComparerSucceeds()
        {
            var comparer = new TestEqualityComparer<string>();
            
            Func<string, string> echo = new EchoSync();
            Func<string, string> cachedEcho;
            using (_setupLock.Enter())
            {
                cachedEcho = echo
                    .Cached()
                    .WithKeyComparer(comparer)
                    .CatchDuplicateRequests()
                    .Build();
            }

            comparer.GetHashCodeCount.Should().Be(0);
            
            cachedEcho("abc");

            comparer.GetHashCodeCount.Should().BeGreaterThan(0);
        }

        [Fact]
        public void MultiParamKeyComparersSucceed()
        {
            var comparer1 = new TestEqualityComparer<string>();
            var comparer2 = new TestEqualityComparer<int>();

            Func<string, int, string> func = (s, i) => s + i;
            Func<string, int, string> cachedFunc;
            using (_setupLock.Enter())
            {
                cachedFunc = func
                    .Cached()
                    .WithKeyComparer(comparer1)
                    .WithKeyComparer(comparer2)
                    .CatchDuplicateRequests()
                    .Build();
            }

            comparer1.GetHashCodeCount.Should().Be(0);
            comparer2.GetHashCodeCount.Should().Be(0);
            
            cachedFunc("abc", 123);

            comparer1.GetHashCodeCount.Should().BeGreaterThan(0);
            comparer2.GetHashCodeCount.Should().BeGreaterThan(0);
        }

        [Fact]
        public void NoKeyComparerSetThrowsOnBuild()
        {
            Func<TypeWithNoEqualityComparer, int> func = x => x.Value;
            using (_setupLock.Enter())
            {
                var configManager = func
                    .Cached()
                    .WithDictionaryCache()
                    .WithKeySerializer(k => k.Value.ToString());

                Action action = () => configManager.Build();

                action.Should().Throw<Exception>().Which.Message.Should().Contain("KeyComparer");
            }
        }
    }
}
