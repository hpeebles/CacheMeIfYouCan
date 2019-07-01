using System;
using FluentAssertions;
using Xunit;

namespace CacheMeIfYouCan.Tests.Proxy
{
    [Collection(TestCollections.Proxy)]
    public class WithUnsupportedMethods
    {
        private readonly CacheSetupLock _setupLock;

        public WithUnsupportedMethods(CacheSetupLock setupLock)
        {
            _setupLock = setupLock;
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ThrowsIfNotAllowedInDefaultSettings(bool allowed)
        {
            ITest impl = new TestImpl();
            using (_setupLock.Enter(true))
            {
                DefaultSettings.Cache.AllowInterfacesWithUnsupportedMethods(allowed);
                
                Func<ITest> func = () => impl
                    .Cached()
                    .Build();

                if (allowed)
                    func.Should().NotThrow();
                else
                    func.Should().Throw<Exception>();

                DefaultSettings.Cache.AllowInterfacesWithUnsupportedMethods();
            }
        }
        
        [Fact]
        public void UnsupportedFuncSucceeds()
        {
            ITest impl = new TestImpl();
            ITest proxy;
            using (_setupLock.Enter())
            {
                proxy = impl
                    .Cached()
                    .Build();
            }

            proxy.UnsupportedFunc(1, 2, 3, 4, 5).Should().Be("1_2_3_4_5");
        }
        
        [Fact]
        public void UnsupportedActionSucceeds()
        {
            ITest impl = new TestImpl();
            ITest proxy;
            using (_setupLock.Enter())
            {
                proxy = impl
                    .Cached()
                    .Build();
            }

            proxy.UnsupportedAction(1);
        }
        
        [Fact]
        public void UnsupportedPropertySucceeds()
        {
            ITest impl = new TestImpl();
            ITest proxy;
            using (_setupLock.Enter())
            {
                proxy = impl
                    .Cached()
                    .Build();
            }

            proxy.UnsupportedProperty = 1;
            proxy.UnsupportedProperty.Should().Be(1);
        }
    }
}