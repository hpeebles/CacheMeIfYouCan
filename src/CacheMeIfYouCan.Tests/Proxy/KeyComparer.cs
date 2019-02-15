using CacheMeIfYouCan.Tests.Common;
using FluentAssertions;
using Xunit;

namespace CacheMeIfYouCan.Tests.Proxy
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
            
            ITest impl = new TestImpl();
            ITest proxy;
            using (_setupLock.Enter())
            {
                proxy = impl
                    .Cached()
                    .WithKeyComparer(comparer)
                    .Build();
            }

            comparer.GetHashCodeCount.Should().Be(0);

            proxy.StringToString("123");

            comparer.GetHashCodeCount.Should().BeGreaterThan(0);
        }
    }
}