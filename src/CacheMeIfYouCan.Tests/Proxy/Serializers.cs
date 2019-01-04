using System.Threading.Tasks;
using Xunit;

namespace CacheMeIfYouCan.Tests.Proxy
{
    public class Serializers : CacheTestBase
    {
        [Fact]
        public async Task SetKeySerializer()
        {
            var serializerA = new TestSerializer();
            var serializerB = new TestSerializer();

            ITest impl = new TestImpl();
            ITest proxy;
            using (EnterSetup(false))
            {
                proxy = impl
                    .Cached()
                    .WithDistributedCacheFactory(new TestCacheFactory())
                    .WithKeySerializers(c => c
                        .SetDefault(serializerA)
                        .Set<string>(serializerB))
                    .Build();
            }

            await proxy.LongToInt(123);
            Assert.Equal(1, serializerA.SerializeCount);
            
            serializerA.ResetCounts();
            
            for (var i = 1; i < 10; i++)
            {
                await proxy.LongToInt(123);

                Assert.Equal(i, serializerA.SerializeCount);
            }
            
            serializerA.ResetCounts();
            
            Assert.Equal(0, serializerB.SerializeCount);
            
            await proxy.StringToString("abc");
            Assert.Equal(1, serializerB.SerializeCount);
            
            serializerB.ResetCounts();
            
            for (var i = 1; i < 10; i++)
            {
                await proxy.StringToString("abc");

                Assert.Equal(i, serializerB.SerializeCount);
            }
            
            Assert.Equal(0, serializerA.SerializeCount);
        }
        
        [Fact]
        public async Task SetValueSerializer()
        {
            var serializerA = new TestSerializer();
            var serializerB = new TestSerializer();

            ITest impl = new TestImpl();
            ITest proxy;
            using (EnterSetup(false))
            {
                proxy = impl
                    .Cached()
                    .WithDistributedCacheFactory(new TestCacheFactory())
                    .WithValueSerializers(c => c
                        .SetDefault(serializerA)
                        .Set<string>(serializerB))
                    .Build();
            }

            await proxy.LongToInt(123);
            Assert.Equal(1, serializerA.SerializeCount);
            
            serializerA.ResetCounts();
            
            for (var i = 1; i < 10; i++)
            {
                await proxy.LongToInt(123);

                Assert.Equal(0, serializerA.SerializeCount);
                Assert.Equal(i, serializerA.DeserializeCount);
            }
            
            serializerA.ResetCounts();
            
            Assert.Equal(0, serializerB.SerializeCount);
            Assert.Equal(0, serializerB.DeserializeCount);
            
            await proxy.StringToString("abc");
            Assert.Equal(1, serializerB.SerializeCount);
            
            serializerB.ResetCounts();
            
            for (var i = 1; i < 10; i++)
            {
                await proxy.StringToString("abc");

                Assert.Equal(0, serializerB.SerializeCount);
                Assert.Equal(i, serializerB.DeserializeCount);
            }
            
            Assert.Equal(0, serializerA.SerializeCount);
            Assert.Equal(0, serializerA.DeserializeCount);
        }
    }
}
